using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FFC.Payment.Integrations.Function.Helpers;
using FFC.Payment.Integrations.Function.Models;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FFC.Payment.Integrations.Function.Services;

/// <summary>
/// Service to communicate with Dynamics 365 (Customer Relationship Management - CRM)
/// </summary>
public class CrmService : ICrmService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly IDateFunctions _dateFunctions;
    private readonly string _tokenEndpoint;
    private readonly string _crmBaseUrl;

    private static readonly string GET_ORGANISATION_URL = "/api/data/v9.2/accounts?$select=name,accountid&$filter=rpa_capfirmid eq";
    private static readonly string CREATE_CASE_URL = "/api/data/v9.2/incidents?$select=incidentid";
    private static readonly string CREATE_ACTIVITY_URL = "/api/data/v9.2/rpa_customernotifications?$select=activityid";
    private static readonly string CREATE_METADATA_URL = "/api/data/v9.2/rpa_activitymetadatas";
    private static readonly string CREATE_LOG_URL = "/api/data/v9.2/rpa_integrationinboundqueues";

    private static readonly string FFC_ERROR = "927350006";
    private static readonly string RLE1_ERROR = "927350005";

    /// <summary>
    /// Constructor for CRM service
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="httpClientFactory"></param>
    /// <param name="dateFunctions"></param>
    public CrmService(IConfiguration configuration, IHttpClientFactory httpClientFactory, IDateFunctions dateFunctions)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("crm-client");
        _dateFunctions = dateFunctions;
        var adLoginBaseUrl = _configuration.GetSection("AdLoginBaseUrl").Value;
        var portalTenantId = _configuration.GetSection("PortalTenantId").Value;
        _tokenEndpoint = $"{adLoginBaseUrl}/{portalTenantId}/oauth2/token";
        _crmBaseUrl = _configuration.GetSection("CrmBaseUrl").Value;
    }

    /// <summary>
    /// Gets an auth token for access to the CRM
    /// </summary>
    /// <returns>auth token</returns>
    public async Task<CrmAuthToken> GetAuthToken()
    {
        var portalClientId = _configuration.GetSection("PortalClientId").Value;
        var portalClientSecret = _configuration.GetSection("PortalClientSecret").Value;
        var body = $"grant_type=client_credentials&client_id={portalClientId}&client_secret={HttpUtility.UrlEncode(portalClientSecret)}&resource={HttpUtility.UrlEncode(_crmBaseUrl)}";
        var msg = new HttpRequestMessage(HttpMethod.Post, _tokenEndpoint);
        msg.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/x-www-form-urlencoded");
        var httpResponse = await _httpClient.SendAsync(msg);
        string resp = await httpResponse.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<CrmAuthToken>(resp);
    }

    /// <summary>
    /// Retrieves the details of an organisation (based on id)
    /// </summary>
    /// <param name="frn">organisation id (known as 'frn')</param>
    /// <param name="authToken">auth token previously obtained</param>
    /// <returns>class populated with org info</returns>
    public async Task<CrmOrganisation> LookupOrganisation(string frn, CrmAuthToken authToken)
    {
        var msg = new HttpRequestMessage(HttpMethod.Get, $"{_crmBaseUrl}/{GET_ORGANISATION_URL} '{frn}'");
        AddAuthHeader(msg, authToken);
        var httpResponse = await _httpClient.SendAsync(msg);
        var resp = await httpResponse.Content.ReadAsStringAsync();
        var crmGenericObj = JsonConvert.DeserializeObject<CrmGenericGetResponse<CrmOrganisation>>(resp);
        return crmGenericObj?.value[0];
    }

    /// <summary>
    /// Create a Case (the table in the CRM DB is 'incidents')
    /// </summary>
    /// <param name="organisationId">org id</param>
    /// <param name="payloadYear">4-digit year</param>
    /// <param name="authToken">auth token previously obtained</param>
    /// <returns>case id</returns>
    public async Task<string> CreateCase(string organisationId, int payloadYear, CrmAuthToken authToken)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, $"{_crmBaseUrl}{CREATE_CASE_URL}");
        AddAuthHeader(msg, authToken);
        AddReturnTypeHeader(msg);
        var body = $"{{  \"caseorigincode\": \"100000002\", \"casetypecode\": \"927350013\", \"customerid_contact@odata.bind\": \"/contacts(65879706-0798-e411-9412-00155deb6486)\", \"rpa_Contact@odata.bind\": \"/contacts(65879706-0798-e411-9412-00155deb6486)\", \"rpa_Organisation@odata.bind\": \"/accounts({organisationId})\", \"rpa_isunknowncontact\": \"true\", \"rpa_isunknownorganisation\": \"false\", \"title\": \"SFI {payloadYear} Payment Statement File - Sent to the Customer via Notify\"}}";
        msg.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        return await PostAndParseResultAsync(msg, "incidentid");
    }

    /// <summary>
    /// Create a Notification Activity (linked to a case). The table in the CRM DB is rpa-customernotifications
    /// </summary>
    /// <param name="caseId">case id to be linked to</param>
    /// <param name="organisationId">org id</param>
    /// <param name="payloadYear">4-digit year</param>
    /// <param name="authToken">auth token previously obtained</param>
    /// <returns>activity id</returns>
    public async Task<string> CreateNotificationActivity(string caseId, string organisationId, int payloadYear, CrmAuthToken authToken)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, $"{_crmBaseUrl}{CREATE_ACTIVITY_URL}");
        AddAuthHeader(msg, authToken);
        AddReturnTypeHeader(msg);
        var utcDateTime = _dateFunctions.GetUtcNow();
        var body = $"{{ \"regardingobjectid_incident_rpa_customernotification@odata.bind\": \"/incidents({caseId})\", \"rpa_contact_rpa_customernotification@odata.bind\": \"/contacts(65879706-0798-e411-9412-00155deb6486)\", \"rpa_datesent\": \"{utcDateTime}\", \"rpa_documenttype_rpa_customernotification@odata.bind\": \"/rpa_documenttypeses(3de06e3d-2b5c-ed11-9562-0022489931ca)\", \"rpa_hasattachment\": \"true\", \"rpa_organisation_rpa_customernotification@odata.bind\": \"/accounts({organisationId})\", \"subject\": \"SFI {payloadYear} Payment Statement File - Sent via Notify\"}}";
        msg.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        return await PostAndParseResultAsync(msg, "activityid");
    }

    /// <summary>
    /// Create a metadata record (linked to case and activity). The table in the CRM DB is rpa_activitymetadatas
    /// </summary>
    /// <param name="caseId">case id to be linked to</param>
    /// <param name="activityId">activity id to be linked to</param>
    /// <param name="organisationId">org id</param>
    /// <param name="sbi">sbi</param>
    /// <param name="functionEndpoint">endpoint for triggering retrieval function app</param>
    /// <param name="functionSasToken">SAS token to authorise function app endpoint</param>
    /// <param name="filename">filename</param>
    /// <param name="authToken">auth token previously obtained</param>
    /// <returns></returns>
    public async Task CreateMetadata(string caseId, string activityId, string organisationId, string sbi, string functionEndpoint, string functionSasToken, string filename, CrmAuthToken authToken)
    {
        var msg = new HttpRequestMessage(HttpMethod.Post, $"{_crmBaseUrl}{CREATE_METADATA_URL}");
        AddAuthHeader(msg, authToken);
        var body = $"{{ \"rpa_CustomerNotificationId@odata.bind\": \"/rpa_customernotifications({activityId})\", \"rpa_RelatedCase@odata.bind\": \"/incidents({caseId})\", \"rpa_contact@odata.bind\": \"/contacts(65879706-0798-e411-9412-00155deb6486)\", \"rpa_direction\": \"false\", \"rpa_docrefproxy\": \"SFIPS\", \"rpa_fileabsoluteurl\": \"{functionEndpoint}{functionSasToken}&id={filename}\", \"rpa_filename\": \"{filename}\", \"rpa_organisation@odata.bind\": \"/accounts({organisationId})\", \"rpa_sbi\": \"{sbi}\"}}";
        msg.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        await PostAndParseResultAsync(msg);
    }

    /// <summary>
    /// Create a record to log an error in the CRM. The table in the CRM DB is rpa_integrationinboundqueues
    /// </summary>
    /// <param name="runId">run id of function trigger</param>
    /// <param name="runType">FFC</param>
    /// <param name="errorReason">Summary of error</param>
    /// <param name="progressText">Text detailing how far the processing got before erroring</param>
    /// <param name="authToken">auth token previously obtained</param>
    /// <returns></returns>
    public async Task CreateLogRecord(string runId, string runType, string errorReason, string progressText, CrmAuthToken authToken = null)
    {
        // The error may have occurred at a point before we got a CRM auth token
        if (authToken == null)
        {
            authToken = await GetAuthToken();
        }

        var msg = new HttpRequestMessage(HttpMethod.Post, $"{_crmBaseUrl}{CREATE_LOG_URL}");
        AddAuthHeader(msg, authToken);

        var runTypeId = runType == "FFC" ? FFC_ERROR : RLE1_ERROR;

        var body = $"{{ \"rpa_name\": \"{runId}\", \"rpa_processingentity\": \"{runTypeId}\", \"rpa_xmlmessage\": \"Failed function app: {errorReason} \nError text: {FfcHelper.EscapeDoubleQuotes(progressText)}\"}}";
        msg.Content = new StringContent(body, System.Text.Encoding.UTF8, "application/json");
        await PostAndParseResultAsync(msg);
    }

    /// <summary>
    /// Determines if the HTTP call is from a CRM web page (or just a user pasting a link into a browser)
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>true if from a CRM web page</returns>
    public bool IsCallFromCrm(HttpRequestData req)
    {
        if (req == null)
        {
            return false;
        }

        IEnumerable<string> referVals = null;
        req.Headers?.TryGetValues("Referer", out referVals);
        return (referVals != null && referVals.FirstOrDefault() != null && referVals.FirstOrDefault().StartsWith(_crmBaseUrl));
    }

    /// <inheritdoc/>
    public string GetUtcNow()
    {
        return DateTime.UtcNow.ToString();
    }

    /// <summary>
    /// Adds the bearer token to the request
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="authToken"></param>
    private void AddAuthHeader(HttpRequestMessage msg, CrmAuthToken authToken)
    {
        msg.Headers.Add("Authorization", $"Bearer {authToken.access_token}");
    }

    /// <summary>
    /// This header is needed by the CRM in order to return creation details in json format
    /// </summary>
    /// <param name="msg"></param>
    private void AddReturnTypeHeader(HttpRequestMessage msg)
    {
        msg.Headers.Add("Prefer", "return=representation");
    }

    /// <summary>
    /// Post data to CRM and parse the result, returning the specified field value (usually the PK)
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="returnKeyName"></param>
    /// <returns>Value of the element defined by returnKeyName</returns>
    private async Task<string> PostAndParseResultAsync(HttpRequestMessage msg, string returnKeyName = null)
    {
        var httpResponse = await _httpClient.SendAsync(msg);
        if (httpResponse.IsSuccessStatusCode)
        {
            string resp = await httpResponse.Content.ReadAsStringAsync();
            if (!string.IsNullOrEmpty(resp) && returnKeyName != null)
            {
                dynamic returnDetails = JObject.Parse(resp);
                return returnDetails?[returnKeyName];
            }
        }
        else
        {
            throw new HttpRequestException($"HTTP error calling {msg.RequestUri} : {httpResponse.StatusCode} {httpResponse.ReasonPhrase}");
        }
        return string.Empty;
    }
}

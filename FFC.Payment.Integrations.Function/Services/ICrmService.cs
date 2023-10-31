
using System.Threading.Tasks;
using FFC.Payment.Integrations.Function.Models;
using Microsoft.Azure.Functions.Worker.Http;

namespace FFC.Payment.Integrations.Function.Services;

/// <summary>
/// Service to communicate with Dynamics 365 (Customer Relationship Management - CRM)
/// </summary>
public interface ICrmService
{
    /// <summary>
    /// Gets an auth token for access to the CRM
    /// </summary>
    /// <returns>auth token</returns>
    Task<CrmAuthToken> GetAuthToken();

    /// <summary>
    /// Retrieves the details of an organisation (based on id)
    /// </summary>
    /// <param name="frn">organisation id (known as 'frn')</param>
    /// <param name="authToken">auth token previously obtained</param>
    /// <returns>class populated with org info</returns>
    Task<CrmOrganisation> LookupOrganisation(string frn, CrmAuthToken authToken);

    /// <summary>
    /// Create a Case (the table in the CRM DB is 'incidents')
    /// </summary>
    /// <param name="organisationId">org id</param>
    /// <param name="payloadYear">4-digit year</param>
    /// <param name="authToken">auth token previously obtained</param>
    /// <returns>case id</returns>
    Task<string> CreateCase(string organisationId, int payloadYear, CrmAuthToken authToken);

    /// <summary>
    /// Create a Notification Activity (linked to a case). The table in the CRM DB is rpa-customernotifications
    /// </summary>
    /// <param name="caseId">case id to be linked to</param>
    /// <param name="organisationId">org id</param>
    /// <param name="payloadYear">4-digit year</param>
    /// <param name="authToken">auth token previously obtained</param>
    /// <returns>activity id</returns>
    Task<string> CreateNotificationActivity(string caseId, string organisationId, int payloadYear, CrmAuthToken authToken);

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
    Task CreateMetadata(string caseId, string activityId, string organisationId, string sbi, string functionEndpoint, string functionSasToken, string filename, CrmAuthToken authToken);

    /// <summary>
    /// Create a record to log an error in the CRM. The table in the CRM DB is rpa_integrationinboundqueues
    /// </summary>
    /// <param name="runId">run id of function trigger</param>
    /// <param name="runType">FFC</param>
    /// <param name="errorReason">Summary of error</param>
    /// <param name="progressText">Text detailing how far the processing got before erroring</param>
    /// <param name="authToken">auth token previously obtained</param>
    /// <returns></returns>
    Task CreateLogRecord(string runId, string runType, string errorReason, string progressText, CrmAuthToken authToken = null);

    /// <summary>
    /// Determines if the HTTP call is from a CRM web page (or just a user pasting a link into a browser)
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <returns>true if from a CRM web page</returns>
    bool IsCallFromCrm(HttpRequestData req);
}

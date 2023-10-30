using System;
using FFC.Payment.Integrations.Function.Services;
using FFC.Payment.Integrations.Function.Tests.Mocks;
using Microsoft.Azure.Functions.Worker;
using Moq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using FFC.Payment.Integrations.Function.Tests.Helpers;
using System.Collections.Generic;
using System.Web;
using FFC.Payment.Integrations.Function.Models;
using FFC.Payment.Integrations.Function.Helpers;

namespace FFC.Payment.Integrations.Function.Tests.Services
{
	public class CrmServiceTests
	{
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly Mock<IDateFunctions> _mockDateFunctions;

        private static readonly string FrozenDateTimeUtcNow = "2023-01-01 12:34:56";

        public CrmServiceTests()
        {
            // Config items
            var configItems = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("AdLoginBaseUrl", "https://adloginbase.com"),
                new KeyValuePair<string,string>("PortalClientId", "11111111-22222222-33333333"),
                new KeyValuePair<string,string>("PortalClientSecret", "client-secret"),
                new KeyValuePair<string,string>("PortalTenantId", "12345678-12345678-12345678"),
                new KeyValuePair<string,string>("CrmBaseUrl", "https://crmbase.com")
            };
            _mockConfiguration = ConfigHelper.SetupConfigForSectionAccess(configItems);

            var mockHttpClientHelper = new MockHttpClientHelper("crm-client");
            _mockHttpClientFactory = mockHttpClientHelper.GetMockHttpClientFactory();
            _mockHttpMessageHandler = mockHttpClientHelper.GetMockHttpMessageHandler();

            _mockDateFunctions = new Mock<IDateFunctions>();
            _mockDateFunctions.Setup(x => x.GetUtcNow()).Returns(FrozenDateTimeUtcNow);
        }

        [Fact]
        public async Task GetAuthToken_Builds_Correct_Body()
        {

            var crmService = new CrmService(_mockConfiguration.Object, _mockHttpClientFactory.Object, _mockDateFunctions.Object);

            var authToken = await crmService.GetAuthToken();

            var authTokenExpectedBody = $"grant_type=client_credentials&client_id=11111111-22222222-33333333&client_secret=client-secret&resource={HttpUtility.UrlEncode("https://crmbase.com")}";
            MockHttpClientHelper.Verify(_mockHttpMessageHandler, x => x.Content != null ? x.Content.ReadAsStringAsync().Result.Contains(authTokenExpectedBody) : false);
        }

        [Fact]
        public async Task LookupOrganisation_Uses_Correct_Uri()
        {

            var crmService = new CrmService(_mockConfiguration.Object, _mockHttpClientFactory.Object, _mockDateFunctions.Object);

            var authToken = new CrmAuthToken();

            var expectedUri = $"https://crmbase.com//api/data/v9.2/accounts?$select=name,accountid&$filter=rpa_capfirmid eq '101202'";

            var org = await crmService.LookupOrganisation("101202", authToken);

            MockHttpClientHelper.Verify(_mockHttpMessageHandler, x => x.RequestUri != null ? x.RequestUri.ToString() == expectedUri : false);
        }

        [Fact]
        public async Task CreateCase_Builds_Correct_Body()
        {

            var crmService = new CrmService(_mockConfiguration.Object, _mockHttpClientFactory.Object, _mockDateFunctions.Object);

            var authToken = new CrmAuthToken();

            var organisationId = "101202";
            var payloadYear = 2023;

            var expectedBody = $"{{  \"caseorigincode\": \"100000002\", \"casetypecode\": \"927350013\", \"customerid_contact@odata.bind\": \"/contacts(65879706-0798-e411-9412-00155deb6486)\", \"rpa_Contact@odata.bind\": \"/contacts(65879706-0798-e411-9412-00155deb6486)\", \"rpa_Organisation@odata.bind\": \"/accounts({organisationId})\", \"rpa_isunknowncontact\": \"true\", \"rpa_isunknownorganisation\": \"false\", \"title\": \"SFI {payloadYear} Payment Statement File - Sent to the Customer via Notify\"}}";

            var newCase = await crmService.CreateCase(organisationId, payloadYear, authToken);

            MockHttpClientHelper.Verify(_mockHttpMessageHandler, x => x.Content != null ? x.Content.ReadAsStringAsync().Result == expectedBody : false);

            Assert.NotNull(newCase);
        }

        [Fact]
        public async Task CreateNotificationActivity_Builds_Correct_Body()
        {

            var crmService = new CrmService(_mockConfiguration.Object, _mockHttpClientFactory.Object, _mockDateFunctions.Object);

            var authToken = new CrmAuthToken();

            var organisationId = "101202";
            var payloadYear = 2023;
            var utcDateTime = FrozenDateTimeUtcNow;
            var caseId = "445566";

            var expectedBody = $"{{ \"regardingobjectid_incident_rpa_customernotification@odata.bind\": \"/incidents({caseId})\", \"rpa_contact_rpa_customernotification@odata.bind\": \"/contacts(65879706-0798-e411-9412-00155deb6486)\", \"rpa_datesent\": \"{utcDateTime}\", \"rpa_documenttype_rpa_customernotification@odata.bind\": \"/rpa_documenttypeses(3de06e3d-2b5c-ed11-9562-0022489931ca)\", \"rpa_hasattachment\": \"true\", \"rpa_organisation_rpa_customernotification@odata.bind\": \"/accounts({organisationId})\", \"subject\": \"SFI {payloadYear} Payment Statement File - Sent via Notify\"}}";

            var newActivity = await crmService.CreateNotificationActivity(caseId, organisationId, payloadYear, authToken);

            MockHttpClientHelper.Verify(_mockHttpMessageHandler, x => x.Content != null ? x.Content.ReadAsStringAsync().Result == expectedBody : false);

            Assert.NotNull(newActivity);
        }

        [Fact]
        public async Task CreateMetadata_Builds_Correct_Body()
        {

            var crmService = new CrmService(_mockConfiguration.Object, _mockHttpClientFactory.Object, _mockDateFunctions.Object);

            var authToken = new CrmAuthToken();

            var caseId = "202303";
            var activityId = "333444";
            var functionEndpoint = "https://functionendpoint.com";
            var functionSasToken = "sas-token";
            var filename = "myfilename.pdf";
            var organisationId = "101202";
            var sbi = "999";

            var expectedBody = $"{{ \"rpa_CustomerNotificationId@odata.bind\": \"/rpa_customernotifications({activityId})\", \"rpa_RelatedCase@odata.bind\": \"/incidents({caseId})\", \"rpa_contact@odata.bind\": \"/contacts(65879706-0798-e411-9412-00155deb6486)\", \"rpa_direction\": \"false\", \"rpa_docrefproxy\": \"SFIPS\", \"rpa_fileabsoluteurl\": \"{functionEndpoint}{functionSasToken}&id={filename}\", \"rpa_filename\": \"{filename}\", \"rpa_organisation@odata.bind\": \"/accounts({organisationId})\", \"rpa_sbi\": \"{sbi}\"}}";

            await crmService.CreateMetadata(caseId, activityId, organisationId, sbi, functionEndpoint, functionSasToken, filename, authToken);

            MockHttpClientHelper.Verify(_mockHttpMessageHandler, x => x.Content != null ? x.Content.ReadAsStringAsync().Result == expectedBody : false);
        }

        [Fact]
        public async Task CreateLogRecord_Builds_Correct_Body()
        {

            var crmService = new CrmService(_mockConfiguration.Object, _mockHttpClientFactory.Object, _mockDateFunctions.Object);

            var authToken = new CrmAuthToken();

            var runId = "111222";
            var runType = "FFC";
            var runTypeId = "927350006";
            var errorReason = "some error reason";
            var progressText = "we got this far";

            var expectedBody = $"{{ \"rpa_name\": \"{runId}\", \"rpa_processingentity\": \"{runTypeId}\", \"rpa_xmlmessage\": \"Failed function app: {errorReason} \nError text: {FfcHelper.EscapeDoubleQuotes(progressText)}\"}}";

            await crmService.CreateLogRecord(runId, runType, errorReason, progressText, authToken);

            MockHttpClientHelper.Verify(_mockHttpMessageHandler, x => x.Content != null ? x.Content.ReadAsStringAsync().Result == expectedBody : false);
        }

        [Fact]
        public void IsCallFromCrm_Returns_False_When_Null_Request()
        {
            var crmService = new CrmService(_mockConfiguration.Object, _mockHttpClientFactory.Object, _mockDateFunctions.Object);
            Assert.False(crmService.IsCallFromCrm(null));
        }

        [Fact]
        public void IsCallFromCrm_Returns_False_When_Not_From_CRM()
        {
            var crmService = new CrmService(_mockConfiguration.Object, _mockHttpClientFactory.Object, _mockDateFunctions.Object);
            var mockFunctionContext = new Mock<FunctionContext>();
            var httpUrl = "https://my-function-app.com";
            var mockReq = new MockHttpRequestData(mockFunctionContext.Object, new Uri(httpUrl));
            Assert.False(crmService.IsCallFromCrm(mockReq));
        }

        [Fact]
        public void IsCallFromCrm_Returns_True_When_From_CRM()
        {
            var crmService = new CrmService(_mockConfiguration.Object, _mockHttpClientFactory.Object, _mockDateFunctions.Object);
            var mockFunctionContext = new Mock<FunctionContext>();
            var httpUrl = "https://my-function-app.com";
            var mockReq = new MockHttpRequestData(mockFunctionContext.Object, new Uri(httpUrl));
            mockReq.Headers.Add("referer", "https://crmbase.com");
            Assert.True(crmService.IsCallFromCrm(mockReq));
        }
    }
}


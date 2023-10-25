using FFC.Payment.Integrations.Function.Services;
using FFC.Payment.Integrations.Function.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker;
using System.Collections.Generic;
using System.Net;
using Microsoft.Azure.Functions.Worker.Http;
using FFC.Payment.Integrations.Function.Models;
using Newtonsoft.Json;
using FFC.Payment.Integrations.Function.Helpers;
using System.Net.Http;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using System.Text;
using System.Threading;
using Azure.Messaging.ServiceBus;

namespace FFC.Payment.Integrations
{
    /// <summary>
    /// Function App triggers
    /// </summary>
    public class FunctionTriggers
    {
        private readonly ICrmService _crmService;
        private readonly INotifyService _notifyService;
        private readonly IConfiguration _configuration;
        private readonly ILogger _logger;

        /// <summary>
        /// Constructor for Triggers
        /// </summary>
        /// <param name="crmService"></param>
        /// <param name="notifyService"></param>
        /// <param name="configuration"></param>
        /// <param name="loggerFactory"></param>
        public FunctionTriggers(ICrmService crmService, INotifyService notifyService, IConfiguration configuration, ILoggerFactory loggerFactory)
        {
            _crmService = crmService;
            _notifyService = notifyService;
            _configuration = configuration;
            _logger = loggerFactory.CreateLogger<FunctionTriggers>();
        }

        /// <summary>
        /// Receives a message from FFC pay statement generator and creates appropriate records in the CRM
        /// </summary>
        /// <param name="message"></param>
        /// <param name="messageActions"></param>
        /// <returns></returns>
        [Function("ReceivePaymentStatement")]
        public async Task ReceivePaymentStatement(
            [ServiceBusTrigger("%ServiceBusTopicName%", "%ServiceBusSubscriptionName%", Connection = "ServiceBusConnectionString")] ServiceBusReceivedMessage message, ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation($"FFC ReceivePaymentStatement trigger function processing message_id: {message.MessageId}");

            var statementMsg = Encoding.UTF8.GetString(message.Body);
            CrmAuthToken authToken = null;
            var progressText = $"Starting: incomingMsg {statementMsg}";

            try
            {
                var isValid = ValidateMessage.IsValid(statementMsg);

                if (!isValid)
                {
                    _logger.LogError("Invalid message: {notificationMsg}", statementMsg);
                    var errors = ValidateMessage.GetValidationErrors(statementMsg);
                    var errorText = string.Join(" - ", errors);
                    _logger.LogError($"JSON errors: {errorText}");
                    progressText += $": {errorText}";
                    throw new HttpRequestException("Invalid JSON");
                }

                var incomingMessage = JsonConvert.DeserializeObject<IncomingQueueMessage>(statementMsg);

                // Extract year
                var year = FfcHelper.ExtractYear(incomingMessage.ApiLink);
                progressText += ": got year {year}";
                var yearInt = int.Parse(year);
                progressText += ": year is a valid integer";

                // Get CRM auth token
                authToken = await _crmService.GetAuthToken();
                progressText += ": got CRM auth token";

                // Lookup organisation
                var org = await _crmService.LookupOrganisation(incomingMessage.Frn, authToken);
                progressText += $": found CRM org id {org.AccountId} name {org.Name}";

                // Extract filename
                var filename = FfcHelper.ExtractFilename(incomingMessage.ApiLink);

                // Create case
                var caseId = await _crmService.CreateCase(org.AccountId, yearInt, authToken);
                progressText += $": created CRM case id {caseId}";

                // Create activity
                var activityId = await _crmService.CreateNotificationActivity(caseId, org.AccountId, yearInt, authToken);
                progressText += $": created CRM activity id {activityId}";

                // Create metadata
                var functionEndpoint = _configuration.GetSection("FunctionEndpoint").Value;
                var functionSasToken = _configuration.GetSection("FunctionSasToken").Value;
                await _crmService.CreateMetadata(caseId, activityId, org.AccountId, incomingMessage.Sbi, functionEndpoint, functionSasToken, filename, authToken);
                progressText += $": created CRM metadata";

                // The message will auto-complete
            }
            catch (Exception exc)
            {
                _logger.LogError(exc, $"Error occurred processing incoming message. Progress: {progressText}");
                await ProcessFailure(message.CorrelationId, $"Exception: {exc.Message}", progressText, authToken);
                //throw;  // This will force a retry to prevent losing the message.
                // Dead-letter the message
                await messageActions.DeadLetterMessageAsync(message);
            }
        }

        /// <summary>
        /// Test HTTP trigger
        /// </summary>
        /// <param name="req"></param>
        /// <returns></returns>
        [Function("ServeStatement")]
        public async Task<HttpResponseData> ServeStatement(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
        {
            _logger.LogInformation("FFC ServeStatement trigger function processing: {req}", req);

            await Task.FromResult(false);

            return req.CreateResponse(HttpStatusCode.OK);
        }

        private async Task ProcessFailure(string runId, string reason, string progressText, CrmAuthToken authToken = null)
        {
            await _crmService.CreateLogRecord(runId, "FFC", reason, progressText, authToken);
        }
    }
}

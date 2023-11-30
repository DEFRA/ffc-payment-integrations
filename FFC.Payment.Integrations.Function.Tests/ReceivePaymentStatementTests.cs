using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using FFC.Payment.Integrations.Function.Models;
using FFC.Payment.Integrations.Function.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Newtonsoft.Json;
using Notify.Models.Responses;
using Xunit;

namespace FFC.Payment.Integrations.Function.Tests
{
	public class ReceivePaymentStatementTests
	{
        private readonly Mock<ICrmService> _mockCrmService;
        private readonly Mock<IPdfService> _mockPdfService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly FunctionTriggers _functionTriggers;

        public ReceivePaymentStatementTests()
        {
            _mockCrmService = new Mock<ICrmService>();
            _mockPdfService = new Mock<IPdfService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _loggerFactory = LoggerFactory.Create(c => c
               .AddConsole()
               .SetMinimumLevel(LogLevel.Debug)
               );

            var functionEndpointSectionMock = new Mock<IConfigurationSection>();
            functionEndpointSectionMock.Setup(s => s.Value).Returns("https://testservice.com");
            var functionSasTokenSectionMock = new Mock<IConfigurationSection>();
            functionSasTokenSectionMock.Setup(s => s.Value).Returns("secret-sas-token");
            _mockConfiguration.Setup(x => x.GetSection("FunctionEndpoint")).Returns(functionEndpointSectionMock.Object);
            _mockConfiguration.Setup(x => x.GetSection("FunctionSasToken")).Returns(functionSasTokenSectionMock.Object);

            _mockCrmService.Setup(x => x.LookupOrganisation(It.IsAny<string>(), It.IsAny<CrmAuthToken>())).ReturnsAsync(new CrmOrganisation("orgName", "12345"));
            _mockCrmService.Setup(x => x.CreateCase(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CrmAuthToken>())).ReturnsAsync("newCaseId");
            _mockCrmService.Setup(x => x.CreateNotificationActivity(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CrmAuthToken>())).ReturnsAsync("newActivityId");
            _mockCrmService.Setup(x => x.CreateMetadata(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CrmAuthToken>()));
            _functionTriggers = new FunctionTriggers(_mockCrmService.Object, _mockPdfService.Object, _mockConfiguration.Object, _loggerFactory);
        }

        [Fact]
        public async Task ReceivePaymentStatement_InvalidMessage_DeadLetters_the_Message()
        {
            var mockMessageActions = new Mock<ServiceBusMessageActions>();

            var incomingMessage = new {
                sbix = 27,
                frn = 1102077240,
                apiLink = "https://myStatementRetrievalApiEndpoint/statement-receiver/statement/v1/FFC_PaymentSchedule_SFI_2022_1000000002_2023072703002347.pdf",
                documentType = "Payment statement", 
                scheme = "SFI"
            };

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(new BinaryData(JsonConvert.SerializeObject(incomingMessage)));

            await _functionTriggers.ReceivePaymentStatement(sbMessage, mockMessageActions.Object);

            mockMessageActions.Verify(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, null, null, default), Times.Once);
        }

        [Fact]
        public async Task ReceivePaymentStatement_ValidMessage_Finishes_Without_DeadLettering_the_Message()
        {
            var mockMessageActions = new Mock<ServiceBusMessageActions>();

            var incomingMessage = new
            {
                sbi = 27,
                frn = 1102077240,
                apiLink = "https://myStatementRetrievalApiEndpoint/statement-receiver/statement/v1/FFC_PaymentSchedule_SFI_2022_1000000002_2023072703002347.pdf",
                documentType = "Payment statement",
                scheme = "SFI"
            };

            var sbMessage = ServiceBusModelFactory.ServiceBusReceivedMessage(new BinaryData(JsonConvert.SerializeObject(incomingMessage)));

            await _functionTriggers.ReceivePaymentStatement(sbMessage, mockMessageActions.Object);

            mockMessageActions.Verify(x => x.DeadLetterMessageAsync(It.IsAny<ServiceBusReceivedMessage>(), null, null, null, default), Times.Never);
        }
    }
}
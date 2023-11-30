using FFC.Payment.Integrations.Function.Services;
using Moq;
using System.Linq;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using FFC.Payment.Integrations.Function.Tests.Helpers;
using System.Collections.Generic;
using FFC.Payment.Integrations.Function.Tests.Mocks;
using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;

namespace FFC.Payment.Integrations.Function.Tests.Services
{
    public class PdfServiceTests
    {
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public PdfServiceTests()
        {
            // Config items
            var configItems = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string,string>("PdfServiceBaseUrl", "https://pdf-service-base.com")
            };
            _mockConfiguration = ConfigHelper.SetupConfigForSectionAccess(configItems);

            var mockHttpClientHelper = new MockHttpClientHelper("pdf-client");
            _mockHttpClientFactory = mockHttpClientHelper.GetMockHttpClientFactory();
            _mockHttpMessageHandler = mockHttpClientHelper.GetMockHttpMessageHandler();
        }

        [Fact]
        public async Task GetPdfContent_Builds_Correct_Uri()
        {
            var pdfService = new PdfService(_mockConfiguration.Object, _mockHttpClientFactory.Object);
            var filename = "my-pdf-file.pdf";
            var expectedUri = $"https://pdf-service-base.com/statements/statement/{filename}";
            var contents = await pdfService.GetPdfContent(filename);

            MockHttpClientHelper.Verify(_mockHttpMessageHandler, x => x.RequestUri?.ToString() == expectedUri);
        }

        [Fact]
        public void ServePdfContents_Adds_Correct_Headers()
        {

            var pdfService = new PdfService(_mockConfiguration.Object, _mockHttpClientFactory.Object);

            var mockFunctionContext = new Mock<FunctionContext>();
            var httpUrl = "https://my-function-app.com";
            var mockReq = new MockHttpRequestData(mockFunctionContext.Object, new Uri(httpUrl));

            var bytes = System.Text.Encoding.UTF8.GetBytes("pdf-content-as-a-byte-array");

            var response = pdfService.ServePDFContents(mockReq, bytes);

            var contentTypeHeader = response.Headers.GetValues("Content-Type").FirstOrDefault();
            var contentDispositionHeader = response.Headers.GetValues("Content-Disposition").FirstOrDefault();

            Assert.NotEmpty(contentTypeHeader);
            Assert.NotEmpty(contentDispositionHeader);

            Assert.Equal("application/pdf", contentTypeHeader);
            Assert.Equal("inline", contentDispositionHeader);
        }
    }
}


using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FFC.Payment.Integrations.Function.Services;
using FFC.Payment.Integrations.Function.Tests.Mocks;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace FFC.Payment.Integrations.Function.Tests
{
    public class ServeStatementTests
    {
        private readonly Mock<ICrmService> _mockCrmService;
        private readonly Mock<IPdfService> _mockPdfService;
        private readonly Mock<IConfiguration> _mockConfiguration;
        private readonly ILoggerFactory _loggerFactory;
        private readonly FunctionTriggers _functionTriggers;

        public ServeStatementTests()
        {
            _mockCrmService = new Mock<ICrmService>();
            _mockPdfService = new Mock<IPdfService>();
            _mockConfiguration = new Mock<IConfiguration>();
            _loggerFactory = LoggerFactory.Create(c => c
               .AddConsole()
               .SetMinimumLevel(LogLevel.Debug)
               );

            var pdfBytes = Encoding.UTF32.GetBytes("dummy pdf content");
            _mockPdfService.Setup(x => x.GetPdfContent(It.IsAny<string>())).ReturnsAsync(pdfBytes);

            _mockCrmService.Setup(x => x.IsCallFromCrm(It.IsAny<HttpRequestData>())).Returns(true);
            _functionTriggers = new FunctionTriggers(_mockCrmService.Object, _mockPdfService.Object, _mockConfiguration.Object, _loggerFactory);
        }

        [Fact]
        public async Task ServeStatement_InvalidId1_Returns_BadRequest()
        {
            var mockFunctionContext = new Mock<FunctionContext>();
            var httpUrl = "https://my-function-app.com/statements?id=";
            var mockReq = new MockHttpRequestData(mockFunctionContext.Object, new Uri(httpUrl));

            var mockResponse = new MockHttpResponseData((new Mock<FunctionContext>()).Object);
            _mockPdfService.Setup(x => x.ServePDFContents(It.IsAny<HttpRequestData>(), It.IsAny<byte[]>())).Returns(mockResponse);

            var res = await _functionTriggers.ServeStatement(mockReq);

            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task ServeStatement_InvalidId2_Returns_BadRequest()
        {
            var mockFunctionContext = new Mock<FunctionContext>();
            var httpUrl = "https://my-function-app.com/statements";
            var mockReq = new MockHttpRequestData(mockFunctionContext.Object, new Uri(httpUrl));

            var mockResponse = new MockHttpResponseData((new Mock<FunctionContext>()).Object);
            _mockPdfService.Setup(x => x.ServePDFContents(It.IsAny<HttpRequestData>(), It.IsAny<byte[]>())).Returns(mockResponse);

            var res = await _functionTriggers.ServeStatement(mockReq);

            Assert.Equal(HttpStatusCode.BadRequest, res.StatusCode);
        }

        [Fact]
        public async Task ServeStatement_NotFound_Returns_NotFound()
        {
            var mockFunctionContext = new Mock<FunctionContext>();
            var httpUrl = "https://my-function-app.com/statements?id=myfilename.pdf";
            var mockReq = new MockHttpRequestData(mockFunctionContext.Object, new Uri(httpUrl));

            var mockResponse = new MockHttpResponseData((new Mock<FunctionContext>()).Object);
            _mockPdfService.Setup(x => x.ServePDFContents(It.IsAny<HttpRequestData>(), It.IsAny<byte[]>())).Returns(mockResponse);

            _mockPdfService.Setup(x => x.GetPdfContent(It.IsAny<string>())).ReturnsAsync(Array.Empty<byte>);
            var res = await _functionTriggers.ServeStatement(mockReq);

            Assert.Equal(HttpStatusCode.NotFound, res.StatusCode);
        }
    }
}
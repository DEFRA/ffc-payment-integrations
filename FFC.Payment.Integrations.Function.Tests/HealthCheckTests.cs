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
    public class HealthCheckTests
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly HealthCheck _healthCheck;

        public HealthCheckTests()
        {
            _loggerFactory = LoggerFactory.Create(c => c
               .AddConsole()
               .SetMinimumLevel(LogLevel.Debug)
               );

            _healthCheck = new HealthCheck(_loggerFactory);
        }

        [Fact]
        public void HealthCheck_Health_Y_Returns_OK()
        {
            var mockFunctionContext = new Mock<FunctionContext>();
            var httpUrl = "https://my-function-app.com";
            var mockReq = new MockHttpRequestData(mockFunctionContext.Object, new Uri(httpUrl));

            var mockResponse = new MockHttpResponseData((new Mock<FunctionContext>()).Object);

            var res = _healthCheck.RunHealthy(mockReq);

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }

        [Fact]
        public void HealthCheck_Health_Z_Returns_OK()
        {
            var mockFunctionContext = new Mock<FunctionContext>();
            var httpUrl = "https://my-function-app.com";
            var mockReq = new MockHttpRequestData(mockFunctionContext.Object, new Uri(httpUrl));

            var mockResponse = new MockHttpResponseData((new Mock<FunctionContext>()).Object);

            var res = _healthCheck.RunHealthz(mockReq);

            Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        }
   }
}
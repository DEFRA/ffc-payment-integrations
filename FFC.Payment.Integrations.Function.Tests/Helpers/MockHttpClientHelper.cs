using Moq;
using Moq.Protected;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FFC.Payment.Integrations.Function.Tests.Helpers
{
	public class MockHttpClientHelper
	{
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;

        public MockHttpClientHelper(string clientName)
        {
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
            _mockHttpClientFactory = SetupMockHttpClientFactory(clientName);
        }

        public Mock<IHttpClientFactory> SetupMockHttpClientFactory(string clientName)
        {
            HttpResponseMessage result = new HttpResponseMessage();

            _mockHttpMessageHandler
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(result)
                .Verifiable();

            var httpClient = new HttpClient(_mockHttpMessageHandler.Object) { };

            var mockHttpClientFactory = new Mock<IHttpClientFactory>();
            mockHttpClientFactory.Setup(_ => _.CreateClient(clientName)).Returns(httpClient);

            return mockHttpClientFactory;
        }

        public Mock<IHttpClientFactory> GetMockHttpClientFactory()
        {
            return _mockHttpClientFactory;
        }

        public Mock<HttpMessageHandler> GetMockHttpMessageHandler()
        {
            return _mockHttpMessageHandler;
        }

        public static void Verify(Mock<HttpMessageHandler> mock, Func<HttpRequestMessage, bool> match)
        {
            mock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1), // expected a single request
                ItExpr.Is<HttpRequestMessage>(req => match(req)),
                ItExpr.IsAny<CancellationToken>()
            );
        }
    }
}


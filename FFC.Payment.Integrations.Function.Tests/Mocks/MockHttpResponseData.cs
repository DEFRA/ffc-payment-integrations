using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.IO;
using System.Net;
using Moq;

namespace FFC.Payment.Integrations.Function.Tests.Mocks
{
    public class MockHttpResponseData : HttpResponseData
    {
        public MockHttpResponseData(FunctionContext functionContext) : base(functionContext)
        {
        }

        public override HttpStatusCode StatusCode { get; set; } = HttpStatusCode.OK;

        public override HttpHeadersCollection Headers { get; set; } = new HttpHeadersCollection();

        public override Stream Body { get; set; } = new MemoryStream();

        public override HttpCookies Cookies { get; } = (new Mock<HttpCookies>()).Object;
    }
}


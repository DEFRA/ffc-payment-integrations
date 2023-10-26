using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using Moq;

namespace FFC.Payment.Integrations.Function.Tests.Mocks
{
    public class MockHttpRequestData : HttpRequestData
    {
        public MockHttpRequestData(FunctionContext functionContext, Uri url, Stream? body = null) : base(functionContext)
        {
            Url = url;
            Body = body ?? new MemoryStream();
        }

        public override Stream Body { get; } = new MemoryStream();

        public override HttpHeadersCollection Headers { get; } = new HttpHeadersCollection();

        public override IReadOnlyCollection<IHttpCookie> Cookies { get; } = new Mock<IReadOnlyCollection<IHttpCookie>>().Object;

        public override Uri Url { get; }

        public override IEnumerable<ClaimsIdentity> Identities { get; } = new Mock<IEnumerable<ClaimsIdentity>>().Object;

        public override string Method { get; } = "GET";

        public override HttpResponseData CreateResponse()
        {
            return new MockHttpResponseData(FunctionContext);
        }
    }
}


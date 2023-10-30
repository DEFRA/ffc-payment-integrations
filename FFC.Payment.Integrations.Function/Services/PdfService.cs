using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using FFC.Payment.Integrations.Function.Helpers;
using FFC.Payment.Integrations.Function.Models;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FFC.Payment.Integrations.Function.Services;

/// <inheritdoc/>
public class PdfService : IPdfService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _serviceBaseUrl;

    /// <inheritdoc/>
    public PdfService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("pdf-client");
        _serviceBaseUrl = _configuration.GetSection("PdfServiceBaseUrl").Value;
    }

    /// <inheritdoc/>
    public async Task<byte[]> GetPdfContent(string filename)
    {
        return await _httpClient.GetByteArrayAsync($"{_serviceBaseUrl}/statements/statement/{filename}");
    }

    /// <inheritdoc/>
    public HttpResponseData ServePDFContents(HttpRequestData req, byte[] content)
    {
        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteBytes(content);
        response.Headers.Add("Content-Type", "application/pdf");
        response.Headers.Add("Content-Disposition", "inline");
        return response;
    }
}

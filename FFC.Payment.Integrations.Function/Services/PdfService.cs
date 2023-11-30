using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;

namespace FFC.Payment.Integrations.Function.Services;

/// <summary>
/// Service to communicate with the FFC payment statement receiver PDF service
/// </summary>
public class PdfService : IPdfService
{
    private readonly IConfiguration _configuration;
    private readonly HttpClient _httpClient;
    private readonly string _serviceBaseUrl;

    /// <summary>
    /// Constructore for PdfService
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="httpClientFactory"></param>
    public PdfService(IConfiguration configuration, IHttpClientFactory httpClientFactory)
    {
        _configuration = configuration;
        _httpClient = httpClientFactory.CreateClient("pdf-client");
        _serviceBaseUrl = _configuration.GetSection("PdfServiceBaseUrl").Value;
    }

    /// <summary>
    /// Retrieves the byte contents of a PDF. The endpoint of the service is set in configuration settings
    /// </summary>
    /// <param name="filename">filename of PDF</param>
    /// <returns>byte content of PDF</returns>
    public async Task<byte[]> GetPdfContent(string filename)
    {
        return await _httpClient.GetByteArrayAsync($"{_serviceBaseUrl}/statements/statement/{filename}");
    }

    /// <summary>
    /// Serve the contents of a PDF to the browser
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="content">byte content of PDF</param>
    /// <returns></returns>
    public HttpResponseData ServePDFContents(HttpRequestData req, byte[] content)
    {
        HttpResponseData response = req.CreateResponse(HttpStatusCode.OK);
        response.WriteBytes(content);
        response.Headers.Add("Content-Type", "application/pdf");
        response.Headers.Add("Content-Disposition", "inline");
        return response;
    }
}

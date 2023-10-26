using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.Functions.Worker.Http;

namespace FFC.Payment.Integrations.Function.Services;

/// <summary>
/// Service to communicate with the FFC payment statement receiver PDF service
/// </summary>
public interface IPdfService
{
    /// <summary>
    /// Retrieves the byte contents of a PDF. The endpoint of the service is set in configuration settings
    /// </summary>
    /// <param name="filename">filename of PDF</param>
    /// <returns>byte content of PDF</returns>
    Task<byte[]> GetPdfContent(string filename);

    /// <summary>
    /// Serve the contents of a PDF to the browser
    /// </summary>
    /// <param name="req">HTTP request</param>
    /// <param name="content">byte content of PDF</param>
    /// <returns></returns>
    HttpResponseData ServePDFContents(HttpRequestData req, byte[] content);
}

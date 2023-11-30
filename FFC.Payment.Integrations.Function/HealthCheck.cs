using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace FFC.Payment.Integrations;

/// <summary>
/// HealthCheck endpoints to satisfy AKS
/// </summary>
public class HealthCheck
{
    private readonly ILogger _logger;

    /// <summary>
    /// Constructor for HealthCheck
    /// </summary>
    /// <param name="loggerFactory"></param>
    public HealthCheck(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<HealthCheck>();
    }

    /// <summary>
    /// Health(y) endpoint
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("healthy")]
    public HttpResponseData RunHealthy(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Healthy check.");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
        return response;
    }

    /// <summary>
    /// Health(z) endpoint
    /// </summary>
    /// <param name="req"></param>
    /// <returns></returns>
    [Function("healthz")]
    public HttpResponseData RunHealthz(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequestData req)
    {
        _logger.LogInformation("Healthz check");

        var response = req.CreateResponse(HttpStatusCode.OK);
        response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

        return response;
    }
}

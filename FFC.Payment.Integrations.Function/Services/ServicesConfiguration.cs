using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using System;
using Azure.Identity;
using System.Diagnostics.CodeAnalysis;

namespace FFC.Payment.Integrations.Function.Services
{
    /// <summary>
    /// Register service-tier services.
    /// </summary>
	[ExcludeFromCodeCoverage]
    public static class ServicesConfiguration
    {
        /// <summary>
        /// Method to register service-tier services.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void AddQueueAndTableServices(this IServiceCollection services, IConfiguration configuration)
        {
/*            services.AddSingleton<IEventQueueService>(_ =>
            {
                var queueCredential = configuration.GetSection("QueueConnectionString:Credential").Value;
                var queueName = configuration.GetSection("EventQueueName").Value;
                if (IsManagedIdentity(queueCredential))
                {
                    var queueServiceUrl = configuration.GetSection("QueueConnectionString:QueueServiceUri").Value;
                    var queueUri = new Uri($"{queueServiceUrl}{queueName}");
                    Console.WriteLine($"Startup.QueueClient using Managed Identity with url {queueUri}");
                    return new EventQueueService(new QueueClient(queueUri, new DefaultAzureCredential()));
                }
                else
                {
                    return new EventQueueService(new QueueClient(configuration.GetSection("QueueConnectionString").Value, queueName));
                }
            });
*/
        }

        private static bool IsManagedIdentity(string credentialName)
        {
            return credentialName != null && credentialName.ToLower() == "managedidentity";
        }
    }
}
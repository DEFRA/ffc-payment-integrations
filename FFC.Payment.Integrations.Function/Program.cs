using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Notify.Client;
using Notify.Interfaces;
using FFC.Payment.Integrations.Function.Services;

var host = new HostBuilder()
    .ConfigureAppConfiguration(config => config
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddUserSecrets<Program>(true)
                    .AddEnvironmentVariables())
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(services =>
    {
        Console.WriteLine("Startup.ConfigureServices() called");
        var serviceProvider = services.BuildServiceProvider();

        var configuration = serviceProvider.GetRequiredService<IConfiguration>();

        services.AddHttpClient();
        services.AddSingleton<INotificationClient>(_ => new NotificationClient(configuration.GetSection("NotifyApiKey").Value));
        services.AddSingleton<INotifyService, NotifyService>(); // Left this in if we want to send alert emails in case of run errors
        services.AddSingleton<ICrmService, CrmService>();
        services.AddSingleton<IPdfService, PdfService>();

        services.AddQueueAndTableServices(configuration);
    })
    .Build();

host.Run();

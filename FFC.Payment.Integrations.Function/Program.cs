using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using FFC.Payment.Integrations.Function.Services;
using FFC.Payment.Integrations.Function.Helpers;
using System.Diagnostics.CodeAnalysis;

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
        services.AddSingleton<ICrmService, CrmService>();
        services.AddSingleton<IPdfService, PdfService>();
        services.AddSingleton<IDateFunctions, DateFunctions>();

        services.AddQueueAndTableServices(configuration);
    })
    .Build();

host.Run();

/// <summary>
/// Dummy constructor so the attribute [ExcludeFromCodeCoverage] can be used
/// </summary>
[ExcludeFromCodeCoverage]
public partial class Program { }
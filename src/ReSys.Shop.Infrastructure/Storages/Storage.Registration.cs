using System.Diagnostics;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Services.Storage.Services;
using ReSys.Shop.Infrastructure.Storages.Options;
using ReSys.Shop.Infrastructure.Storages.Providers;

using Serilog;

namespace ReSys.Shop.Infrastructure.Storages;

public static class StorageConfiguration
{
    public static IServiceCollection AddStorageServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            Log.Information(messageTemplate: "Configuring storage services");

            services
                .AddOptions<StorageOptions>()
                .Bind(config: configuration.GetSection(key: StorageOptions.Section))
                .ValidateOnStart();

            services.AddScoped<IStorageService>(implementationFactory: sp =>
            {
                var options = sp.GetRequiredService<IOptions<StorageOptions>>().Value;

                Log.Information(
                    messageTemplate: "Storage provider selected: {Provider}",
                    propertyValue: options.Provider);

                return options.Provider switch
                {
                    StorageProvider.Local =>
                        ActivatorUtilities.CreateInstance<LocalStorageService>(provider: sp),

                    StorageProvider.Azure =>
                        ActivatorUtilities.CreateInstance<AzureStorageService>(provider: sp),

                    StorageProvider.GoogleCloud =>
                        ActivatorUtilities.CreateInstance<GoogleCloudStorageService>(provider: sp),

                    _ => throw new InvalidOperationException(
                        message: $"Unsupported StorageProvider: {options.Provider}")
                };
            });

            sw.Stop();

            Log.Information(
                messageTemplate: "Storage services configured in {Duration}ms",
                propertyValue: sw.Elapsed.TotalMilliseconds);

            return services;
        }
        catch (Exception ex)
        {
            sw.Stop();

            Log.Fatal(
                exception: ex,
                messageTemplate: LogTemplates.ComponentStartupFailed,
                propertyValue0: "Storage",
                propertyValue1: ex.Message);

            throw;
        }
    }
}

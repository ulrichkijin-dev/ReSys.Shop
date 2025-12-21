using Microsoft.Extensions.DependencyInjection;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Infrastructure.Seeders.Contexts;
using ReSys.Shop.Infrastructure.Seeders.Orchestrators;

using Serilog;

namespace ReSys.Shop.Infrastructure.Seeders;

internal static class SeedersServiceCollectionExtensions
{
    /// <summary>
    /// Registers data seeding services and orchestrators for initial data population.
    /// </summary>
    internal static void AddDataSeeders(this IServiceCollection services)
    {
        services.AddTransient<IDataSeeder, IdentityDataSeeder>();
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
            propertyValue0: nameof(IdentityDataSeeder),
            propertyValue1: "Transient");

        services.AddTransient<IDataSeeder, LocationDataSeeder>();
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
            propertyValue0: nameof(LocationDataSeeder),
            propertyValue1: "Transient");

        services.AddTransient<IDataSeeder, FashionProductDataSeeder>();
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
            propertyValue0: nameof(FashionProductDataSeeder),
            propertyValue1: "Transient");

        services.AddHostedService<SeederOrchestrator>();
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
            propertyValue0: nameof(SeederOrchestrator),
            propertyValue1: "Singleton");

        Log.Information(messageTemplate: "Data seeders registered successfully.");
    }
}
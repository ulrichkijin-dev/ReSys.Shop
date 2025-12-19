using Microsoft.Extensions.Hosting;

using ReSys.Shop.Infrastructure.Seeders.Contexts;

using Serilog;

namespace ReSys.Shop.Infrastructure.Seeders.Orchestrators;

public class SeederOrchestrator(IEnumerable<IDataSeeder> seeders) : IHostedService
{
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (IDataSeeder seeder in seeders.OrderBy(keySelector: m => m.Order))
        {
            string name = seeder.GetType().Name;
            Log.Information(messageTemplate: "[SeedOrchestrator] Running {Seeder}",
                propertyValue: name);
            await seeder.SeedAsync(cancellationToken: cancellationToken);
            Log.Information(messageTemplate: "[SeedOrchestrator] {Seeder} completed",
                propertyValue: name);
        }

        Log.Information(messageTemplate: "[SeedOrchestrator] Seeding completed.");
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
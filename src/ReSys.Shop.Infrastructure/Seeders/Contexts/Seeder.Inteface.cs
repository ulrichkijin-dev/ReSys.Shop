namespace ReSys.Shop.Infrastructure.Seeders.Contexts;

public interface IDataSeeder
{
    Task SeedAsync(CancellationToken cancellationToken);
    int Order { get; }
}
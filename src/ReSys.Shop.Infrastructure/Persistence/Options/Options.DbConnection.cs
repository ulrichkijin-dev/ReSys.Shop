namespace ReSys.Shop.Infrastructure.Persistence.Options;

/// <summary>
/// Defines database provider options and connection string keys for the application.
/// </summary>
public static class DbConnectionOptions
{
    /// <summary>
    /// The default database provider used in production environments.
    /// </summary>
    public const string DefaultProvider = Postgres;

    /// <summary>
    /// The default connection string key used for database configuration.
    /// </summary>
    public const string DefaultConnectionString = "DefaultConnection";

    public const string Postgres = "PostgreSQL";
    public const string SqlServer = "SqlServer";
    public const string Sqlite = "Sqlite";
    public const string InMemory = "InMemory";

    public const string MigrationsHistoryTable = "__EFMigrationsHistory";
    public const string MigrationsSchema = "eshopdb";

    public const string TestDatabaseName = "TestDb";
    public const string TestEnvironmentName = "Test";
}
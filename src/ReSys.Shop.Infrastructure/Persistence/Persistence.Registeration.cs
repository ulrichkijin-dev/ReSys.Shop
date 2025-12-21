using System.Diagnostics;

using Ardalis.GuardClauses;

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Domain.Concerns;
using ReSys.Shop.Core.Domain.Auditing;
using ReSys.Shop.Core.Domain.Catalog.Products;
using ReSys.Shop.Core.Domain.Catalog.Products.Images;
using ReSys.Shop.Core.Domain.Catalog.Products.Reviews;
using ReSys.Shop.Core.Domain.Catalog.PropertyTypes;
using ReSys.Shop.Core.Domain.Identity.Permissions;
using ReSys.Shop.Core.Domain.Identity.UserAddresses;
using ReSys.Shop.Core.Domain.Inventories.Locations;
using ReSys.Shop.Core.Domain.Orders;
using ReSys.Shop.Core.Domain.Orders.Adjustments;
using ReSys.Shop.Core.Domain.Orders.Payments;
using ReSys.Shop.Core.Domain.Settings;
using ReSys.Shop.Infrastructure.Persistence.Caching;
using ReSys.Shop.Infrastructure.Persistence.Contexts;
using ReSys.Shop.Infrastructure.Persistence.Interceptors;
using ReSys.Shop.Infrastructure.Persistence.Options;
using ReSys.Shop.Infrastructure.Seeders;

using Serilog;

namespace ReSys.Shop.Infrastructure.Persistence;

/// <summary>
/// Extension methods for configuring database and persistence services.
/// </summary>
internal static class DatabaseServiceCollectionExtensions
{
    #region Public Methods

    /// <summary>
    /// Registers all persistence-related services including EF Core,
    /// interceptors, Unit of Work, and data seeding components.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">The configuration provider.</param>
    /// <param name="environment">The hosting environment.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for chaining.</returns>
    public static IServiceCollection AddPersistence(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var stopwatch = Stopwatch.StartNew();
        Log.Information(messageTemplate: LogTemplates.ModuleRegistered,
            propertyValue0: "Persistence",
            propertyValue1: 0);

        try
        {
            var count = 0;

            services.AddEfCoreInterceptors();
            count++;

            services.AddDatabase(configuration: configuration, environment: environment);
            count++;

            services.AddCaching(configuration: configuration, environment: environment);
            count++;

            services.AddUnitOfWork();
            count++;

            services.AddDataSeeders();
            count++;

            stopwatch.Stop();
            Log.Information(messageTemplate: LogTemplates.ModuleRegistered, propertyValue0: "Persistence", propertyValue1: count);
            Log.Debug(messageTemplate: "Persistence configured in {Duration:0.0000}ms", propertyValue: stopwatch.Elapsed.TotalMilliseconds);

            return services;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Fatal(exception: ex, messageTemplate: LogTemplates.ComponentStartupFailed, propertyValue0: "Persistence", propertyValue1: ex.Message);
            throw;
        }
    }

    #endregion

    #region EF Core Interceptors

    /// <summary>
    /// Registers EF Core save-change interceptors for audit tracking and domain event dispatching.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    private static void AddEfCoreInterceptors(this IServiceCollection services)
    {
        services.AddScoped<ISaveChangesInterceptor, PersistenceInterceptors.ActionTracking>();
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered, propertyValue0: nameof(PersistenceInterceptors.ActionTracking), propertyValue1: "Scoped");

        services.AddScoped<ISaveChangesInterceptor, PersistenceInterceptors.DispatchDomainEvent>();
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered, propertyValue0: nameof(PersistenceInterceptors.DispatchDomainEvent), propertyValue1: "Scoped");

        services.AddScoped<ISaveChangesInterceptor, PersistenceInterceptors.AuditingLog>();

        Log.Information(messageTemplate: "EF Core interceptors registered (3 total)");
    }

    #endregion

    #region Database Configuration

    /// <summary>
    /// Registers the application's database context with an appropriate EF Core provider
    /// based on the current hosting environment.
    /// </summary>
    /// <param name="services">The service collection to extend.</param>
    /// <param name="configuration">The configuration provider.</param>
    /// <param name="environment">The hosting environment.</param>
    private static void AddDatabase(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            var connectionString = configuration.GetConnectionString(name: DbConnectionOptions.DefaultConnectionString);
            Guard.Against.NullOrWhiteSpace(input: connectionString, parameterName: nameof(connectionString));

            services.AddDbContext<ApplicationDbContext>(optionsAction: (sp, options) =>
            {
                options.AddInterceptors(interceptors: sp.GetServices<ISaveChangesInterceptor>());

                if (environment.IsEnvironment(environmentName: DbConnectionOptions.TestEnvironmentName))
                {
                    ConfigureInMemoryDatabase(options: options);
                    Log.Information(messageTemplate: LogTemplates.DbConnected, propertyValue0: "Test", propertyValue1: "InMemory-TestDb");
                }
                else if (environment.IsDevelopment())
                {
                    ConfigurePostgreSqlDatabase(options: options, connectionString: connectionString, enableSensitiveLogging: true);
                    Log.Information(messageTemplate: LogTemplates.DbConnected, propertyValue0: "Development", propertyValue1: "PostgreSQL");
                }
                else
                {
                    ConfigurePostgreSqlDatabase(options: options, connectionString: connectionString, enableSensitiveLogging: false);
                    Log.Information(messageTemplate: LogTemplates.DbConnected, propertyValue0: environment.EnvironmentName, propertyValue1: "PostgreSQL");
                }
            });

            services.AddScoped<IApplicationDbContext>(implementationFactory: sp =>
                sp.GetRequiredService<ApplicationDbContext>());

            Log.Debug(messageTemplate: LogTemplates.ServiceRegistered, propertyValue0: nameof(ApplicationDbContext), propertyValue1: "Scoped");

            sw.Stop();
            LogDatabaseConfiguration(environment: environment, durationMs: sw.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            Log.Error(exception: ex, messageTemplate: LogTemplates.OperationFailed, propertyValue0: nameof(AddDatabase), propertyValue1: ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Configures the in-memory database provider for testing.
    /// </summary>
    private static void ConfigureInMemoryDatabase(DbContextOptionsBuilder options)
    {
        options.UseInMemoryDatabase(databaseName: DbConnectionOptions.TestDatabaseName)
               .EnableDetailedErrors()
               .EnableSensitiveDataLogging();
    }

    /// <summary>
    /// Configures the PostgreSQL database provider with optimized settings.
    /// </summary>
    private static void ConfigurePostgreSqlDatabase(
        DbContextOptionsBuilder options,
        string connectionString,
        bool enableSensitiveLogging)
    {
        options.UseNpgsql(connectionString: connectionString, npgsqlOptionsAction: npgsqlOptions =>
        {
            npgsqlOptions.UseVector();

            npgsqlOptions.MigrationsHistoryTable(
                tableName: DbConnectionOptions.MigrationsHistoryTable,
                schema: DbConnectionOptions.MigrationsSchema);

            npgsqlOptions.ConfigureDataSource(dataSourceBuilderAction: dataSourceBuilder =>
            {
                dataSourceBuilder.EnableDynamicJson();
                dataSourceBuilder.MapEnum<DisplayOn>();
                dataSourceBuilder.MapEnum<AuditSeverity>();
                dataSourceBuilder.MapEnum<ProductImage.ProductImageType>();
                dataSourceBuilder.MapEnum<Product.ProductStatus>();
                dataSourceBuilder.MapEnum<Review.ReviewStatus>();
                dataSourceBuilder.MapEnum<PropertyType.PropertyKind>();
                dataSourceBuilder.MapEnum<ConfigurationValueType>();
                dataSourceBuilder.MapEnum<AccessPermission.PermissionCategory>();
                dataSourceBuilder.MapEnum<AddressType>();
                dataSourceBuilder.MapEnum<LocationType>();
                dataSourceBuilder.MapEnum<OrderAdjustment.AdjustmentScope>();
                dataSourceBuilder.MapEnum<Order.OrderState>();
                dataSourceBuilder.MapEnum<Payment.PaymentState>();
            });

            npgsqlOptions.CommandTimeout(commandTimeout: 30);
        })
            .EnableDetailedErrors()
            .UseSnakeCaseNamingConvention();

        if (enableSensitiveLogging)
        {
            options.EnableSensitiveDataLogging();
        }

        options.ConfigureWarnings(warningsConfigurationBuilderAction: warnings =>
        {
            warnings.Ignore(eventIds: RelationalEventId.MultipleCollectionIncludeWarning);
        });
    }

    /// <summary>
    /// Logs the database configuration details.
    /// </summary>
    private static void LogDatabaseConfiguration(IHostEnvironment environment, double durationMs)
    {
        var provider = environment.IsEnvironment(environmentName: DbConnectionOptions.TestEnvironmentName)
            ? DbConnectionOptions.InMemory
            : DbConnectionOptions.Postgres;

        Log.Information(messageTemplate: LogTemplates.ConfigLoaded, propertyValue0: "Database", propertyValue1: new
        {
            Environment = environment.EnvironmentName,
            Provider = provider,
            Interceptors = true,
            SnakeCaseNaming = provider == DbConnectionOptions.Postgres,
            VectorSupport = provider == DbConnectionOptions.Postgres,
            RetryOnFailure = provider == DbConnectionOptions.Postgres
        });

        Log.Debug(messageTemplate: "Database configured in {Duration:0.0000}ms", propertyValue: durationMs);
    }

    #endregion

    #region Unit of Work

    /// <summary>
    /// Registers the Unit of Work pattern implementation.
    /// </summary>
    private static void AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IApplicationDbContext>(implementationFactory: sp =>
            sp.GetRequiredService<ApplicationDbContext>());
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered, propertyValue0: nameof(IApplicationDbContext), propertyValue1: "Scoped");
    }

    #endregion

    #region Migration Utilities

    /// <summary>
    /// Ensures the database is created and migrations are applied.
    /// Use with caution in production environments.
    /// </summary>
    public static async Task EnsureDatabaseAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var sw = Stopwatch.StartNew();
        Log.Information(messageTemplate: "Ensuring database exists and migrations are applied...");

        try
        {
            await context.Database.MigrateAsync(cancellationToken: cancellationToken);
            sw.Stop();

            Log.Information(messageTemplate: "Database ensured successfully in {Duration:0.0000}ms", propertyValue: sw.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            sw.Stop();
            Log.Error(exception: ex, messageTemplate: "Failed to ensure database: {Error}", propertyValue: ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Gets pending migrations that haven't been applied to the database.
    /// </summary>
    public static async Task<IEnumerable<string>> GetPendingMigrationsAsync(
        this IServiceProvider serviceProvider,
        CancellationToken cancellationToken = default)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        return await context.Database.GetPendingMigrationsAsync(cancellationToken: cancellationToken);
    }

    #endregion
}
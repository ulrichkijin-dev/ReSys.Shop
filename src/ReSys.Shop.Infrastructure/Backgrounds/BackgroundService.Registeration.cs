using System.Diagnostics;

using Ardalis.GuardClauses;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using Quartz;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Options.Systems;
using ReSys.Shop.Infrastructure.Backgrounds.Jobs;
using ReSys.Shop.Infrastructure.Backgrounds.Options;
using ReSys.Shop.Infrastructure.Persistence.Options;

using Serilog;

namespace ReSys.Shop.Infrastructure.Backgrounds;

/// <summary>
/// Configures Quartz.NET background job processing with environment-specific storage
/// and recurring job registration for e-commerce operations.
/// </summary>
public static class BackgroundServiceConfiguration
{
    #region Service Registration

    /// <summary>
    /// Registers Quartz services with environment-specific storage providers
    /// and server configuration for background job processing.
    /// </summary>
    public static IServiceCollection AddBackgroundServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Log.Information(messageTemplate: "Configuring background services (Quartz.NET)");

            services.Configure<BackgroundServicesOptions>(config: configuration.GetSection(key: BackgroundServicesOptions.Section))
                .AddOptionsWithValidateOnStart<BackgroundServicesOptions>();

            Log.Information(
                messageTemplate: LogTemplates.ConfigLoaded,
                propertyValue0: "BackgroundServicesOptions",
                propertyValue1: new { BackgroundServicesOptions.Section });

            services.AddQuartz(configure: configurator =>
            {
                var backgroundOptions = configuration
                    .GetSection(key: BackgroundServicesOptions.Section)
                    .Get<BackgroundServicesOptions>() ?? new BackgroundServicesOptions();

                QuartzServerOptions serverConfig = backgroundOptions.Server;

                configurator.SchedulerId = $"Scheduler_{Guid.NewGuid():N}";
                configurator.SchedulerName = "QuartzScheduler";
                configurator.MaxBatchSize = 10;
                configurator.InterruptJobsOnShutdown = true;
                configurator.InterruptJobsOnShutdownWithWait = true;

                configurator.UseDefaultThreadPool(configure: tp =>
                {
                    tp.MaxConcurrency = serverConfig.MaxConcurrency;
                });

                configurator.MisfireThreshold = TimeSpan.FromSeconds(seconds: serverConfig.MisfireThresholdSeconds);

                ConfigureQuartzStorage(configurator: configurator,
                    configuration: configuration,
                    environment: environment);

                RegisterJobs(configurator: configurator);
            });

            services.AddHttpClient("ImageSearchService", (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<ImageServiceOption>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.DefaultRequestHeaders.Add("X-API-Key", options.ApiKey);
            });

            Log.Information(
                messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "Quartz",
                propertyValue1: "Singleton");

            services.AddQuartzHostedService(configure: options =>
            {
                options.WaitForJobsToComplete = true;
                options.AwaitApplicationStarted = true;
            });

            Log.Information(
                messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "QuartzHostedService",
                propertyValue1: "Singleton");

            stopwatch.Stop();

            Log.Information(
                messageTemplate: LogTemplates.BackgroundServiceStarted,
                propertyValue: "Quartz.NET");

            Log.Information(
                messageTemplate: "Background services configured in {Duration:0.0000}ms",
                propertyValue: stopwatch.Elapsed.TotalMilliseconds);

            return services;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Log.Fatal(
                exception: ex,
                messageTemplate: LogTemplates.ComponentStartupFailed,
                propertyValue0: "BackgroundServices",
                propertyValue1: ex.Message);

            throw;
        }
    }

    #endregion

    #region Middleware Configuration

    /// <summary>
    /// Configures Quartz and schedules recurring jobs for the application.
    /// </summary>
    public static IApplicationBuilder UseBackgroundServices(
        this IApplicationBuilder app,
        IHostEnvironment environment)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Log.Information(messageTemplate: "Configuring background services middleware");

            IServiceProvider serviceProvider = app.ApplicationServices;
            var backgroundOptions = serviceProvider
                .GetRequiredService<IOptions<BackgroundServicesOptions>>()
                .Value;

            if (backgroundOptions.EnableDashboard)
            {
                Log.Warning(messageTemplate: "Quartz dashboard requested but not implemented. Consider using CrystalQuartz package.");
            }

            if (!environment.IsEnvironment(environmentName: "Test"))
            {
                var schedulerFactory = serviceProvider.GetRequiredService<ISchedulerFactory>();
                var scheduler = schedulerFactory.GetScheduler().GetAwaiter().GetResult();

                ScheduleRecurringJobs(scheduler: scheduler);
            }
            else
            {
                Log.Information(messageTemplate: "Recurring job scheduling skipped (Test environment)");
            }

            stopwatch.Stop();

            Log.Information(
                messageTemplate: "Background services middleware configured in {Duration:0.0000}ms",
                propertyValue: stopwatch.Elapsed.TotalMilliseconds);

            return app;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Log.Error(
                exception: ex,
                messageTemplate: LogTemplates.OperationFailed,
                propertyValue0: "UseBackgroundServices",
                propertyValue1: ex.Message);

            throw;
        }
    }

    #endregion

    #region Storage Configuration

    /// <summary>
    /// Configures Quartz storage provider based on environment.
    /// Uses in-memory for development/testing, PostgresSQL for production.
    /// </summary>
    private static void ConfigureQuartzStorage(
        IServiceCollectionQuartzConfigurator configurator,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        if (environment.IsEnvironment(environmentName: "Test") || environment.IsDevelopment())
        {
            configurator.UseInMemoryStore();

            Log.Information(
                messageTemplate: LogTemplates.ConfigLoaded,
                propertyValue0: "QuartzStorage",
                propertyValue1: new { Environment = environment.EnvironmentName, Provider = "InMemory" });
        }
        else
        {
            string? connectionString = configuration.GetConnectionString(name: DbConnectionOptions.DefaultConnectionString);

            Guard.Against.Null(
                input: connectionString,
                message: "Connection string required for Quartz in production");

            configurator.UsePersistentStore(configure: store =>
            {
                store.UsePostgres(configurer: postgres =>
                {
                    postgres.ConnectionString = connectionString;
                    postgres.TablePrefix = "qrtz_";
                });

                store.UseNewtonsoftJsonSerializer();
                store.UseClustering();
            });

            Log.Information(
                messageTemplate: LogTemplates.ConfigLoaded,
                propertyValue0: "QuartzStorage",
                propertyValue1: new
                {
                    Environment = environment.EnvironmentName,
                    Provider = "PostgreSQL"
                });
        }
    }

    #endregion

    #region Job Registration

    /// <summary>
    /// Registers all job types with Quartz
    /// </summary>
    private static void RegisterJobs(IServiceCollectionQuartzConfigurator configurator)
    {
        configurator.AddJob<RefreshTokenCleanupJob>(configure: opts => opts
            .WithIdentity(key: RefreshTokenCleanupJob.JobKey)
            .WithDescription(description: RefreshTokenCleanupJob.Description)
            .StoreDurably());

        configurator.AddJob<InventoryUpdateJob>(configure: opts => opts
            .WithIdentity(key: InventoryUpdateJob.JobKey)
            .WithDescription(description: InventoryUpdateJob.Description)
            .StoreDurably());

        configurator.AddJob<AbandonedCartEmailJob>(configure: opts => opts
            .WithIdentity(key: AbandonedCartEmailJob.JobKey)
            .WithDescription(description: AbandonedCartEmailJob.Description)
            .StoreDurably());

        configurator.AddJob<DailySalesReportJob>(configure: opts => opts
            .WithIdentity(key: DailySalesReportJob.JobKey)
            .WithDescription(description: DailySalesReportJob.Description)
            .StoreDurably());

        configurator.AddJob<LowStockAlertJob>(configure: opts => opts
            .WithIdentity(key: LowStockAlertJob.JobKey)
            .WithDescription(description: LowStockAlertJob.Description)
            .StoreDurably());

        // Catalog Jobs
        configurator.AddJob<Jobs.Catalog.ImageProcessingJob>(configure: opts => opts
            .WithIdentity(key: Jobs.Catalog.ImageProcessingJob.JobKey)
            .WithDescription(description: Jobs.Catalog.ImageProcessingJob.Description)
            .StoreDurably());
    }

    /// <summary>
    /// Schedules all recurring jobs with their triggers
    /// </summary>
    private static void ScheduleRecurringJobs(IScheduler scheduler)
    {
        var stopwatch = Stopwatch.StartNew();
        var jobCount = 0;

        try
        {
            Log.Information(messageTemplate: "Scheduling recurring background jobs");

            ScheduleJob(scheduler: scheduler,
                jobKey: RefreshTokenCleanupJob.JobKey,
                triggerKey: RefreshTokenCleanupJob.TriggerKey,
                cronExpression: RefreshTokenCleanupJob.CronExpression,
                jobName: nameof(RefreshTokenCleanupJob));
            jobCount++;

            ScheduleJob(scheduler: scheduler,
                jobKey: InventoryUpdateJob.JobKey,
                triggerKey: InventoryUpdateJob.TriggerKey,
                cronExpression: InventoryUpdateJob.CronExpression,
                jobName: nameof(InventoryUpdateJob));
            jobCount++;

            ScheduleJob(scheduler: scheduler,
                jobKey: AbandonedCartEmailJob.JobKey,
                triggerKey: AbandonedCartEmailJob.TriggerKey,
                cronExpression: AbandonedCartEmailJob.CronExpression,
                jobName: nameof(AbandonedCartEmailJob));
            jobCount++;

            ScheduleJob(scheduler: scheduler,
                jobKey: DailySalesReportJob.JobKey,
                triggerKey: DailySalesReportJob.TriggerKey,
                cronExpression: DailySalesReportJob.CronExpression,
                jobName: nameof(DailySalesReportJob));
            jobCount++;

            ScheduleJob(scheduler: scheduler,
                jobKey: LowStockAlertJob.JobKey,
                triggerKey: LowStockAlertJob.TriggerKey,
                cronExpression: LowStockAlertJob.CronExpression,
                jobName: nameof(LowStockAlertJob));
            jobCount++;

            ScheduleJob(scheduler: scheduler,
                jobKey: Jobs.Catalog.ImageProcessingJob.JobKey,
                triggerKey: Jobs.Catalog.ImageProcessingJob.TriggerKey,
                cronExpression: Jobs.Catalog.ImageProcessingJob.CronExpression,
                jobName: nameof(Jobs.Catalog.ImageProcessingJob));
            jobCount++;

            Log.Information(
                messageTemplate: "Recurring jobs scheduled ({JobCount} jobs) in {Duration:0.0000}ms",
                propertyValue0: jobCount,
                propertyValue1: stopwatch.Elapsed.TotalMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Log.Error(
                exception: ex,
                messageTemplate: LogTemplates.JobFailed,
                propertyValue0: "ScheduleRecurringJobs",
                propertyValue1: ex.Message);

            throw;
        }
    }

    private static void ScheduleJob(IScheduler scheduler, JobKey jobKey, TriggerKey triggerKey,
        string cronExpression, string jobName)
    {
        try
        {
            var trigger = TriggerBuilder.Create()
                .WithIdentity(key: triggerKey)
                .WithCronSchedule(cronExpression: cronExpression,
                    action: x => x
                        .InTimeZone(tz: TimeZoneInfo.Local)
                        .WithMisfireHandlingInstructionFireAndProceed())
                .ForJob(jobKey: jobKey)
                .Build();

            scheduler.ScheduleJob(trigger: trigger).GetAwaiter().GetResult();

            Log.Information(
                messageTemplate: "Scheduled job: {JobName} with schedule {CronExpression}",
                propertyValue0: jobName,
                propertyValue1: cronExpression);
        }
        catch (Exception ex)
        {
            Log.Error(exception: ex,
                messageTemplate: "Failed to schedule job {JobName}: {ErrorMessage}",
                propertyValue0: jobName,
                propertyValue1: ex.Message);
            throw;
        }
    }

    #endregion
}
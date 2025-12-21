using System.Diagnostics;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Options.Systems;
using ReSys.Shop.Infrastructure.Backgrounds;
using ReSys.Shop.Infrastructure.Notifications;
using ReSys.Shop.Infrastructure.Persistence;
using ReSys.Shop.Infrastructure.Security;
using ReSys.Shop.Infrastructure.Storages;

using Serilog;

namespace ReSys.Shop.Infrastructure;


/// <summary>
/// Provides extension methods for configuring infrastructure layer services and middleware.
/// Includes persistence, security, identity, notifications, background services, and storage.
/// </summary>
public static class DependencyInjection
{
    #region Service Registration

    /// <summary>
    /// Registers all infrastructure layer services including persistence, security, notifications,
    /// background services, and storage solutions.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="configuration">Application configuration</param>
    /// <param name="environment">Web host environment</param>
    /// <returns>The configured service collection for method chaining</returns>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Log.Information(messageTemplate: LogTemplates.ModuleRegistered,
                propertyValue0: "Infrastructure",
                propertyValue1: 0);

            var serviceCount = 0;

            services.AddSystems(configuration: configuration);
            serviceCount++;
            Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "Systems",
                propertyValue1: "Singleton");

            services.AddPersistence(configuration: configuration,
                environment: environment);
            serviceCount++;
            Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "Persistence",
                propertyValue1: "Scoped");

            services.AddSecurity(configuration: configuration);
            serviceCount++;
            Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "Security",
                propertyValue1: "Singleton");

            services.AddNotificationServices(configuration: configuration,
                environment: environment);
            serviceCount++;
            Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "Notifications",
                propertyValue1: "Scoped");

            services.AddBackgroundServices(configuration: configuration,
                environment: environment);
            serviceCount++;
            Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "BackgroundServices",
                propertyValue1: "Singleton");

            services.AddStorageServices(configuration: configuration,
                environment: environment);
            serviceCount++;
            Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "Storage",
                propertyValue1: "Scoped");

            // Payments
            services.AddScoped<Core.Domain.Orders.Payments.Gateways.IGatewayCredentialProvider, Payments.Gateways.GatewayCredentialProvider>();
            services.AddScoped<Core.Domain.Orders.Payments.Gateways.IPaymentProcessor, Payments.Gateways.CodProcessor>();
            services.AddScoped<Core.Domain.Orders.Payments.Gateways.IPaymentProcessor, Payments.Gateways.PayPalProcessor>();
            services.AddScoped<Core.Domain.Orders.Payments.Gateways.IPaymentProcessor, Payments.Gateways.StripeProcessor>();
            services.AddScoped<Payments.Gateways.PaymentWebhookProcessor>();

            stopwatch.Stop();
            Log.Information(
                messageTemplate: LogTemplates.ModuleRegistered,
                propertyValue0: "Infrastructure",
                propertyValue1: serviceCount);

            Log.Debug(
                messageTemplate: "Infrastructure services registered in {Duration:0.0000}ms",
                propertyValue: stopwatch.Elapsed.TotalMilliseconds);

            return services;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Fatal(
                exception: ex,
                messageTemplate: LogTemplates.ComponentStartupFailed,
                propertyValue0: "Infrastructure",
                propertyValue1: ex.Message);
            throw;
        }
    }

    #endregion

    #region Middleware Configuration

    /// <summary>
    /// Configures infrastructure middleware and initializes background services in the application pipeline.
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="appConfiguration"></param>
    /// <param name="environment">Web host environment</param>
    /// <returns>The configured application builder for method chaining</returns>
    public static IApplicationBuilder UseInfrastructure(this IApplicationBuilder app,
        IConfiguration appConfiguration,
        IWebHostEnvironment environment)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            Log.Information(messageTemplate: "Configuring infrastructure middleware pipeline");
            var middlewareCount = 0;
            
            app.UseBackgroundServices(environment: environment);
            Log.Debug(messageTemplate: LogTemplates.MiddlewareAdded,
                propertyValue0: "BackgroundServices",
                propertyValue1: 1);
            middlewareCount++;

            stopwatch.Stop();
            Log.Information(
                messageTemplate: "Infrastructure middleware configured ({MiddlewareCount} components) in {Duration:0.0000}ms",
                propertyValue0: middlewareCount,
                propertyValue1: stopwatch.Elapsed.TotalMilliseconds);

            return app;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            Log.Error(
                exception: ex,
                messageTemplate: "Infrastructure middleware configuration failed: {ErrorMessage}",
                propertyValue: ex.Message);
            throw;
        }
    }

    #endregion

}
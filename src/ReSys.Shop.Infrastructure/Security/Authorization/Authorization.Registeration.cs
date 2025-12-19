using System.Diagnostics;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Services.Security.Authorization.Claims;
using ReSys.Shop.Infrastructure.Security.Authorization.Options;
using ReSys.Shop.Infrastructure.Security.Authorization.Policies;
using ReSys.Shop.Infrastructure.Security.Authorization.Providers;
using ReSys.Shop.Infrastructure.Security.Authorization.Requirements;

using Serilog;

namespace ReSys.Shop.Infrastructure.Security.Authorization;

/// <summary>
/// Configures authorization services including custom policy providers,
/// requirement handlers, and user authorization caching.
/// </summary>
public static class AuthorizationConfiguration
{
    #region Main Configuration

    /// <summary>
    /// Registers all authorization services including policy providers,
    /// requirement handlers, and user authorization caching.
    /// </summary>
    /// <param name="services">The service collection to configure</param>
    /// <param name="configuration">Application configuration</param>
    /// <returns>The configured service collection for method chaining</returns>
    public static IServiceCollection AddAuthorizationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Log.Information(messageTemplate: "Configuring authorization services");

            var serviceCount = 0;

            services.AddOptions<AuthUserCacheOption>()
                .Bind(config: configuration.GetSection(key: AuthUserCacheOption.Section))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            serviceCount++;
            Log.Debug(
                messageTemplate: LogTemplates.ConfigLoaded,
                propertyValue0: "AuthUserCacheOption",
                propertyValue1: new { AuthUserCacheOption.Section });

            services.AddScoped<IAuthorizeClaimDataProvider, AuthorizeClaimDataProvider>();
            serviceCount++;
            Log.Debug(
                messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "IUserAuthorizationProvider",
                propertyValue1: "Scoped");

            services.AddTransient<IAuthorizationHandler, HasAuthorizeClaimRequirementHandler>();
            serviceCount++;
            Log.Debug(
                messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "HasAuthorizeClaimRequirementHandler",
                propertyValue1: "Transient");

            services.AddSingleton<IAuthorizationPolicyProvider, HasAuthorizationPolicyProvider>();
            serviceCount++;
            Log.Debug(
                messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "HasAuthorizationPolicyProvider",
                propertyValue1: "Singleton");

            services.AddAuthorization();
            serviceCount++;
            Log.Debug(
                messageTemplate: LogTemplates.ServiceRegistered,
                propertyValue0: "Authorization",
                propertyValue1: "Singleton");

            stopwatch.Stop();

            Log.Information(
                messageTemplate: "Authorization configured ({ServiceCount} services) in {Duration:0.0000}ms",
                propertyValue0: serviceCount,
                propertyValue1: stopwatch.Elapsed.TotalMilliseconds);

            Log.Information(
                messageTemplate: LogTemplates.ConfigLoaded,
                propertyValue0: "Authorization",
                propertyValue1: new
                {
                    PolicyProvider = "HasAuthorizationPolicyProvider",
                    RequirementHandler = "HasAuthorizationRequirementHandler",
                    CacheEnabled = true
                });

            return services;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Log.Fatal(
                exception: ex,
                messageTemplate: LogTemplates.ComponentStartupFailed,
                propertyValue0: "Authorization",
                propertyValue1: ex.Message);

            throw;
        }
    }

    /// <summary>
    /// Configures the middleware pipeline for the presentation layer including
    /// documentation, error handling, routing, authentication, and endpoints.
    /// </summary>
    /// <param name="app">The web application builder</param>
    /// <returns>The configured application builder for method chaining</returns>
    public static IApplicationBuilder UseAuthorizationInternal(this IApplicationBuilder app)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Log.Information(messageTemplate: "Configuring presentation middleware pipeline");

            var middlewareCount = 0;

            app.UseAuthorization();
            middlewareCount++;
            Log.Debug(messageTemplate: LogTemplates.MiddlewareAdded,
                propertyValue0: "Authorization",
                propertyValue1: middlewareCount);

            stopwatch.Stop();

            Log.Debug(
                messageTemplate: "Presentation middleware configured ({MiddlewareCount} components) in {Duration:0.0000}ms",
                propertyValue0: middlewareCount,
                propertyValue1: stopwatch.Elapsed.TotalMilliseconds);

            return app;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            Log.Error(
                exception: ex,
                messageTemplate: "Presentation middleware configuration failed: {ErrorMessage}",
                propertyValue: ex.Message);

            throw;
        }
    }

    #endregion
}
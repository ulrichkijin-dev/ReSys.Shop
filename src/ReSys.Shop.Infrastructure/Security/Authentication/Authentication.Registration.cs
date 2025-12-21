using ReSys.Shop.Core.Common.Services.Security.Encryptors.Interfaces;
using ReSys.Shop.Infrastructure.Security.Encryptors;
using System.Diagnostics;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Infrastructure.Security.Authentication.Contexts;
using ReSys.Shop.Infrastructure.Security.Authentication.Externals;
using ReSys.Shop.Infrastructure.Security.Authentication.Identity;
using ReSys.Shop.Infrastructure.Security.Authentication.Tokens;

using Serilog;

namespace ReSys.Shop.Infrastructure.Security.Authentication;

/// <summary>
/// Orchestrates registration of authentication-related services.
/// </summary>
public static class AuthenticationConfiguration
{
    /// <summary>
    /// Registers authentication: options, context services and external token validation.
    /// </summary>
    public static IServiceCollection AddAuthenticationInternal(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var sw = Stopwatch.StartNew();

        try
        {
            Log.Information(messageTemplate: "Configuring authentication services");

            services.AddAuthenticationContext();
            services.AddSingleton<ICredentialEncryptor, AesCredentialEncryptor>();

            services.AddExternalAuthentication(configuration: configuration);
            services.AddTokensAuthentication(configuration: configuration);

            services.AddAuthentication(configureOptions: opts =>
                {
                    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .ConfigureCookiesAuthentication(configuration: configuration)
                .ConfigureTokensAuthentication(configuration: configuration)
                .ConfigureExternalAuthentication(configuration: configuration);

            sw.Stop();
            Log.Information(
                messageTemplate: "Authentication configured in {Duration:0.0000}ms",
                propertyValue: sw.Elapsed.TotalMilliseconds);

            return services;
        }
        catch (Exception ex)
        {
            sw.Stop();
            Log.Fatal(exception: ex,
                messageTemplate: LogTemplates.ComponentStartupFailed,
                propertyValue0: "Authentication",
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
    public static IApplicationBuilder UseAuthenticationInternal(this IApplicationBuilder app)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            Log.Information(messageTemplate: "Configuring presentation middleware pipeline");

            var middlewareCount = 0;

            app.UseAuthentication();
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

}
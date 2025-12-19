using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Infrastructure.Security.Authentication;
using ReSys.Shop.Infrastructure.Security.Authentication.Identity;
using ReSys.Shop.Infrastructure.Security.Authorization;

using Serilog;

namespace ReSys.Shop.Infrastructure.Security;

public static class SecurityConfiguration
{
    public static IServiceCollection AddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddShopIdentityCore();
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
            propertyValue0: "Identity",
            propertyValue1: "Scoped");

        services.AddAuthenticationInternal(configuration: configuration);

        services.AddAuthorizationInternal(configuration: configuration);

        return services;
    }

    public static IApplicationBuilder UseSecurity(this IApplicationBuilder app)
    {
        app.UseAuthenticationInternal();

        app.UseAuthorizationInternal();

        return app;
    }
}
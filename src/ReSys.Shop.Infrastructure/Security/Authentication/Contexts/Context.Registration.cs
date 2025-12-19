using Microsoft.Extensions.DependencyInjection;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Common.Services.Security.Authentication.Contexts;

using Serilog;

namespace ReSys.Shop.Infrastructure.Security.Authentication.Contexts;

/// <summary>
/// Registers the authentication-related context & token services.
/// </summary>
internal static class ContextRegistration
{
    public static IServiceCollection AddAuthenticationContext(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
            propertyValue0: "HttpContextAccessor",
            propertyValue1: "Singleton");

        services.AddScoped<IUserContext, UserContext>();
        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
            propertyValue0: "IUserContext",
            propertyValue1: "Scoped");

        Log.Information(messageTemplate: "Authentication context services registered");
        return services;
    }
}
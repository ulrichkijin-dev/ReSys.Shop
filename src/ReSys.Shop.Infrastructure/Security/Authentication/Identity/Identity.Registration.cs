using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using ReSys.Shop.Core.Common.Constants;
using ReSys.Shop.Core.Domain.Identity.Roles;
using ReSys.Shop.Core.Domain.Identity.Users;
using ReSys.Shop.Infrastructure.Persistence.Contexts;

using Serilog;

namespace ReSys.Shop.Infrastructure.Security.Authentication.Identity;

public static class IdentityExtension
{
    /// <summary>
    /// Alternative configuration using IdentityCore with manually added services
    /// Use this if you specifically need IdentityCore instead of full Identity
    /// </summary>
    public static IServiceCollection AddShopIdentityCore(this IServiceCollection services)
    {
        services
            .AddIdentityCore<User>(setupAction: options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(minutes: 10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddRoles<Role>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders()
            .AddSignInManager<SignInManager<User>>()
            .AddRoleManager<RoleManager<Role>>()
            .AddUserManager<UserManager<User>>();

        services.Configure<DataProtectionTokenProviderOptions>(configureOptions: o =>
        {
            o.TokenLifespan = TimeSpan.FromHours(hours: 2);
        });


        return services;
    }

    /// <summary>
    /// Recommended configuration using full Identity (simpler and more complete)
    /// </summary>
    public static IServiceCollection AddShopIdentity(this IServiceCollection services)
    {
        services
            .AddIdentity<User, Role>(setupAction: options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 4;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(minutes: 10);
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;

                options.User.RequireUniqueEmail = true;
                options.User.AllowedUserNameCharacters =
                    "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        services.Configure<DataProtectionTokenProviderOptions>(configureOptions: o =>
        {
            o.TokenLifespan = TimeSpan.FromHours(hours: 2);
        });

        return services;
    }

    public static AuthenticationBuilder ConfigureCookiesAuthentication(this AuthenticationBuilder auth,
        IConfiguration configuration)
    {
        auth.AddCookie(authenticationScheme: CookieAuthenticationDefaults.AuthenticationScheme,
            configureOptions: cookie =>
            {
                cookie.Cookie.HttpOnly = true;
                cookie.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                cookie.Cookie.SameSite = SameSiteMode.Strict;
                cookie.ExpireTimeSpan = TimeSpan.FromMinutes(minutes: 30);
            });

        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
            propertyValue0: "CookieAuthentication",
            propertyValue1: "Singleton");

        auth.AddCookie(authenticationScheme: IdentityConstants.ExternalScheme,
            configureOptions: cookie =>
            {
                cookie.Cookie.HttpOnly = true;
                cookie.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                cookie.Cookie.SameSite = SameSiteMode.Lax;
                cookie.ExpireTimeSpan = TimeSpan.FromMinutes(minutes: 15);
            });

        Log.Debug(messageTemplate: LogTemplates.ServiceRegistered,
            propertyValue0: "ExternalScheme",
            propertyValue1: "Singleton");

        return auth;
    }
}
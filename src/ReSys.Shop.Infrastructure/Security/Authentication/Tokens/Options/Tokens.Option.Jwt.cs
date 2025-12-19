using System.Text;

using Microsoft.Extensions.Options;

namespace ReSys.Shop.Infrastructure.Security.Authentication.Tokens.Options;

public sealed class JwtOptions : IValidateOptions<JwtOptions>
{
    public const string Section = "Authentication:Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;

    public int AccessTokenLifetimeMinutes { get; init; } = 15;

    public int RefreshTokenLifetimeDays { get; init; } = 7;
    public int RefreshTokenRememberMeLifetimeDays { get; init; } = 30;

    public int AdminRefreshTokenLifetimeDays { get; init; } = 1;
    public int AdminMaxActiveRefreshTokensPerUser { get; init; } = 2;

    public int MaxActiveRefreshTokensPerUser { get; init; } = 5;
    public int RevokedTokenRetentionDays { get; init; } = 5;

    public ValidateOptionsResult Validate(string? name, JwtOptions options)
    {
        List<string> failures = [];

        if (string.IsNullOrWhiteSpace(value: options.Issuer))
            failures.Add(item: "JwtOptions.Issuer is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(value: options.Audience))
            failures.Add(item: "JwtOptions.Audience is required and cannot be empty.");

        if (string.IsNullOrWhiteSpace(value: options.Secret))
            failures.Add(item: "JwtOptions.Secret is required and cannot be empty.");

        if (!string.IsNullOrWhiteSpace(value: options.Secret))
        {
            byte[] secretBytes = Encoding.UTF8.GetBytes(s: options.Secret);
            if (secretBytes.Length < 32) failures.Add(item: "JwtOptions.Secret must be at least 32 characters (256 bits) for HMAC-SHA256 security.");

            if (secretBytes.Length < 64) failures.Add(item: "JwtOptions.Secret should be at least 64 characters (512 bits) for optimal security.");
        }

        if (options.AccessTokenLifetimeMinutes < 5)
            failures.Add(item: "JwtOptions.AccessTokenLifetimeMinutes must be at least 5 minutes.");

        if (options.AccessTokenLifetimeMinutes > 60)
            failures.Add(item: "JwtOptions.AccessTokenLifetimeMinutes should not exceed 60 minutes for security reasons. Use refresh tokens for longer sessions.");

        if (options.RefreshTokenLifetimeDays < 1)
            failures.Add(item: "JwtOptions.RefreshTokenExpiryInDays must be at least 1 day.");

        if (options.AdminRefreshTokenLifetimeDays < 1)
            failures.Add(item: "JwtOptions.AdminRefreshTokenLifetimeDays must be at least 1 day.");

        if (options.AdminMaxActiveRefreshTokensPerUser < 1)
            failures.Add(item: "JwtOptions.AdminMaxActiveRefreshTokensPerUser must be at least 1.");

        if (options.RefreshTokenLifetimeDays > 30)
            failures.Add(item: "JwtOptions.RefreshTokenExpiryInDays should not exceed 30 days for security reasons.");

        if (options.RefreshTokenRememberMeLifetimeDays < options.RefreshTokenLifetimeDays)
            failures.Add(item: "JwtOptions.RefreshTokenExpiryRememberMeInDays must be greater than or equal to RefreshTokenExpiryInDays.");

        if (options.RefreshTokenRememberMeLifetimeDays > 90)
            failures.Add(item: "JwtOptions.RefreshTokenExpiryRememberMeInDays should not exceed 90 days for security reasons.");

        if (options.MaxActiveRefreshTokensPerUser < 1)
            failures.Add(item: "JwtOptions.MaxActiveRefreshTokensPerUser must be at least 1.");

        if (options.MaxActiveRefreshTokensPerUser > 20)
            failures.Add(item: "JwtOptions.MaxActiveRefreshTokensPerUser should not exceed 20 to prevent abuse.");

        if (options.RevokedTokenRetentionDays < 0)
            failures.Add(item: "JwtOptions.RevokedTokenRetentionDays must be non-negative.");

        if (options.RevokedTokenRetentionDays > 365)
            failures.Add(item: "JwtOptions.RevokedTokenRetentionDays should not exceed 365 days to manage storage.");

        if (options.AccessTokenLifetimeMinutes > 30)
            failures.Add(item: "SECURITY WARNING: JwtOptions.AccessTokenLifetimeMinutes exceeds 30 minutes. Consider shorter durations for better security.");

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures: failures);
    }
}
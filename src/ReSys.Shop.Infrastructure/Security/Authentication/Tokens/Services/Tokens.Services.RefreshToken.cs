using ErrorOr;

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using ReSys.Shop.Core.Common.Services.Persistence.Interfaces;
using ReSys.Shop.Core.Common.Services.Security.Authentication.Tokens.Interfaces;
using ReSys.Shop.Core.Common.Services.Security.Authentication.Tokens.Models;
using ReSys.Shop.Core.Domain.Identity.Tokens;
using ReSys.Shop.Core.Domain.Identity.Users;
using ReSys.Shop.Infrastructure.Security.Authentication.Tokens.Options;

namespace ReSys.Shop.Infrastructure.Security.Authentication.Tokens.Services;

public sealed class RefreshTokenService(
    IApplicationDbContext applicationDbContext,
    IOptions<JwtOptions> options,
    UserManager<User> userManager,
    ILogger<RefreshTokenService> logger)
    : IRefreshTokenService
{
    private readonly JwtOptions _options = options.Value;

    public async Task<ErrorOr<TokenResult>> GenerateRefreshTokenAsync(
        string userId, string ipAddress, bool rememberMe = false, CancellationToken cancellationToken = default)
    {
        User? user = await userManager.FindByIdAsync(userId: userId);
        if (user is null)
            return User.Errors.NotFound(credential: userId);

        if (await userManager.IsLockedOutAsync(user: user))
            return User.Errors.LockedOut;

        try
        {
            int lifetimeDays = rememberMe
                ? _options.RefreshTokenRememberMeLifetimeDays
                : _options.RefreshTokenLifetimeDays;

            var rawToken = RefreshToken.GenerateRandomToken();
            var token = RefreshToken.Create(
                user: user,
                token: rawToken,
                lifetime: TimeSpan.FromDays(days: lifetimeDays),
                ipAddress: ipAddress);

            if (token.IsError)
                return token.Errors;

            applicationDbContext.Set<RefreshToken>().Add(entity: token.Value);
            await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            logger.LogInformation(message: "Refresh token generated for user {UserId}",
                args: userId);

            return new TokenResult
            {
                Token = rawToken,
                ExpiresAt = token.Value.ExpiresAt.ToUnixTimeSeconds(),
            };
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex,
                message: "Refresh token generation failed for user {UserId}",
                args: userId);
            return RefreshToken.Errors.GenerationFailed;
        }
    }

    public async Task<ErrorOr<TokenResult>> RotateRefreshTokenAsync(
        string rawCurrentToken, string ipAddress, bool rememberMe = false, CancellationToken cancellationToken = default)
    {
        string hash = RefreshToken.Hash(rawToken: rawCurrentToken);
        RefreshToken? oldToken = await applicationDbContext.Set<RefreshToken>()
            .Include(navigationPropertyPath: t => t.User)
            .FirstOrDefaultAsync(predicate: t => t.TokenHash == hash,
                cancellationToken: cancellationToken);

        if (oldToken is null) return RefreshToken.Errors.RefreshTokenNotFound;

        if (oldToken.IsRevoked)
        {
            logger.LogWarning(
                message: "SECURITY ALERT: Token reuse detected for user {UserId} from IP {IpAddress}. Revoking token family {TokenFamily}",
                args:
                [
                    oldToken.UserId,
                    ipAddress,
                    oldToken.TokenFamily
                ]);

            await RevokeTokenFamilyAsync(tokenFamily: oldToken.TokenFamily!,
                ipAddress: ipAddress,
                reason: "Token reuse detected - potential theft",
                cancellationToken: cancellationToken);
            return RefreshToken.Errors.Revoked;
        }

        if (oldToken.IsExpired) return RefreshToken.Errors.Expired;

        if (!string.IsNullOrEmpty(value: oldToken.CreatedByIp) && oldToken.CreatedByIp != ipAddress)
        {
            logger.LogWarning(
                message: "IP address changed during token rotation for user {UserId}. Old: {OldIp}, New: {NewIp}",
                args:
                [
                    oldToken.UserId,
                    oldToken.CreatedByIp,
                    ipAddress
                ]);
        }

        await using IDbContextTransaction transaction = await applicationDbContext.Database.BeginTransactionAsync(cancellationToken: cancellationToken);

        try
        {
            int lifetimeDays = rememberMe
                ? _options.RefreshTokenRememberMeLifetimeDays
                : _options.RefreshTokenLifetimeDays;

            string rawNewToken = RefreshToken.GenerateRandomToken();
            var newToken = RefreshToken.Create(
                user: oldToken.User,
                token: rawNewToken,
                lifetime: TimeSpan.FromDays(days: lifetimeDays),
                ipAddress: ipAddress,
                tokenFamily: oldToken.TokenFamily);

            if (newToken.IsError)
            {
                await transaction.RollbackAsync(cancellationToken: cancellationToken);
                return newToken.Errors;
            }

            oldToken.Revoke(ipAddress: ipAddress,
                reason: "Token rotated");
            applicationDbContext.Set<RefreshToken>().Update(entity: oldToken);

            applicationDbContext.Set<RefreshToken>().Add(entity: newToken.Value);

            await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
            await transaction.CommitAsync(cancellationToken: cancellationToken);

            logger.LogInformation(message: "Token rotated successfully for user {UserId}",
                args: oldToken.UserId);

            return new TokenResult
            {
                Token = rawNewToken,
                ExpiresAt = newToken.Value.ExpiresAt.ToUnixTimeSeconds(),
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync(cancellationToken: cancellationToken);
            logger.LogError(exception: ex,
                message: "Token rotation failed for user {UserId}",
                args: oldToken.UserId);
            return RefreshToken.Errors.RotationFailed;
        }
    }

    public async Task<ErrorOr<Success>> RevokeTokenAsync(
        string rawToken,
        string ipAddress,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        string hash = RefreshToken.Hash(rawToken: rawToken);
        RefreshToken? token = await applicationDbContext.Set<RefreshToken>()
            .FirstOrDefaultAsync(predicate: t => t.TokenHash == hash,
                cancellationToken: cancellationToken);

        if (token is null || token.IsRevoked)
            return Result.Success;

        try
        {
            token.Revoke(ipAddress: ipAddress,
                reason: reason ?? "Manual revocation");
            applicationDbContext.Set<RefreshToken>().Update(entity: token);
            await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);

            logger.LogInformation(message: "Token revoked successfully for user {UserId}",
                args: token.UserId);
            return Result.Success;
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex,
                message: "Token revocation failed for token {TokenHash}",
                args: token.TokenHash);
            return RefreshToken.Errors.RevocationFailed;
        }
    }

    private async Task RevokeTokenFamilyAsync(string tokenFamily,
        string ipAddress,
        string? reason = null,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(value: tokenFamily)) return;

        try
        {
            var tokens = await applicationDbContext.Set<RefreshToken>()
                .Where(predicate: t => t.TokenFamily == tokenFamily && !t.IsRevoked)
                .ToListAsync(cancellationToken: cancellationToken);

            if (tokens.Count == 0) return;

            foreach (var token in tokens)
            {
                var result = token.Revoke(ipAddress: ipAddress,
                    reason: reason ?? "Token family revoked");
                if (result.IsError)
                {
                    logger.LogWarning(message: "Failed to revoke token {TokenHash}: {Errors}",
                        args:
                        [
                            token.TokenHash,
                            result.Errors
                        ]);
                    continue;
                }
                applicationDbContext.Set<RefreshToken>().Update(entity: token);
            }

            await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
            logger.LogWarning(message: "Revoked {Count} tokens in family {TokenFamily}",
                args:
                [
                    tokens.Count,
                    tokenFamily
                ]);
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex,
                message: "Failed to revoke token family {TokenFamily}",
                args: tokenFamily);
        }
    }

    public async Task<ErrorOr<int>> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            DateTimeOffset retentionCutoff = now.AddDays(days: -_options.RevokedTokenRetentionDays);

            int deletedCount = await applicationDbContext.Set<RefreshToken>()
                .Where(predicate: t => t.ExpiresAt < now || (t.IsRevoked && t.RevokedAt < retentionCutoff))
                .ExecuteDeleteAsync(cancellationToken: cancellationToken);

            if (deletedCount > 0)
                logger.LogInformation(message: "Token cleanup removed {Count} tokens",
                    args: deletedCount);

            return deletedCount;
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex,
                message: "Token cleanup operation failed");
            return RefreshToken.Errors.RevocationFailed;
        }
    }

    public async Task<ErrorOr<RefreshTokenValidationResult>> ValidateRefreshTokenAsync(
        string token,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string hash = RefreshToken.Hash(rawToken: token);
            RefreshToken? stored = await applicationDbContext.Set<RefreshToken>()
                .Include(navigationPropertyPath: t => t.User)
                .FirstOrDefaultAsync(predicate: t => t.TokenHash == hash,
                    cancellationToken: cancellationToken);

            if (stored is null) return RefreshToken.Errors.RefreshTokenNotFound;
            if (stored.IsRevoked) return RefreshToken.Errors.Revoked;
            if (stored.IsExpired) return RefreshToken.Errors.Expired;

            if (await userManager.IsLockedOutAsync(user: stored.User))
                return User.Errors.LockedOut;

            return new RefreshTokenValidationResult
            {
                RefreshToken = stored,
                User = stored.User
            };
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex,
                message: "Refresh token validation failed");
            return RefreshToken.Errors.ValidationFailed;
        }
    }

    public async Task<ErrorOr<int>> RevokeAllUserTokensAsync(
        string userId,
        string ipAddress,
        string? reason = null,
        string? exceptToken = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            string? exceptHash = null;
            if (!string.IsNullOrWhiteSpace(value: exceptToken))
            {
                try
                {
                    exceptHash = RefreshToken.Hash(rawToken: exceptToken);
                }
                catch
                {
                    exceptHash = null;
                }
            }

            List<RefreshToken> tokens = await applicationDbContext.Set<RefreshToken>()
                .Where(predicate: t => t.UserId == userId && !t.IsRevoked && (exceptHash == null || t.TokenHash != exceptHash))
                .ToListAsync(cancellationToken: cancellationToken);

            if (tokens.Count == 0)
                return 0;

            foreach (RefreshToken t in tokens)
            {
                t.Revoke(ipAddress: ipAddress,
                    reason: reason ?? "Revoke all user tokens");
                applicationDbContext.Set<RefreshToken>().Update(entity: t);
            }

            await applicationDbContext.SaveChangesAsync(cancellationToken: cancellationToken);
            logger.LogInformation(message: "Revoked {Count} tokens for user {UserId}",
                args:
                [
                    tokens.Count,
                    userId
                ]);

            return tokens.Count;
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex,
                message: "Failed to revoke all tokens for user {UserId}",
                args: userId);
            return RefreshToken.Errors.RevocationFailed;
        }
    }
}
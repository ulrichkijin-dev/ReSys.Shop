using Microsoft.Extensions.Logging;

using Quartz;

using ReSys.Shop.Core.Common.Services.Security.Authentication.Tokens.Interfaces;

namespace ReSys.Shop.Infrastructure.Backgrounds.Jobs;

/// <summary>
/// Background job to clean up expired and revoked refresh tokens.
/// Runs daily to maintain database hygiene and comply with data retention policies.
/// </summary>
[DisallowConcurrentExecution]
public sealed class RefreshTokenCleanupJob(
    IRefreshTokenService refreshTokenService,
    ILogger<RefreshTokenCleanupJob> logger) : IJob
{
    public static readonly JobKey JobKey = new(name: nameof(RefreshTokenCleanupJob),
        group: "security");
    public static readonly TriggerKey TriggerKey = new(name: $"{nameof(RefreshTokenCleanupJob)}Trigger",
        group: "security");

    public const string CronExpression = "0 0 2 * * ?";
    internal const string Description = "Cleans up expired and revoked refresh tokens.";

    private const int WarningThreshold = 10000;
    private const int ErrorThreshold = 50000;

    public async Task Execute(IJobExecutionContext context)
    {
        var cancellationToken = context.CancellationToken;
        var startTime = DateTimeOffset.UtcNow;

        logger.LogInformation(message: "Starting refresh token cleanup job at {Time}",
            args: startTime);

        try
        {
            ErrorOr<int> result = await refreshTokenService.CleanupExpiredTokensAsync(cancellationToken: cancellationToken);

            if (result.IsError)
            {
                logger.LogError(message: "Failed to clean up refresh tokens: {Errors}",
                    args: string.Join(separator: ", ",
                        values: result.Errors.Select(selector: e => $"{e.Code}:{e.Description}")));

                throw new JobExecutionException(
                    cause: new OperationCanceledException(message: $"Token cleanup failed: {result.Errors.First().Description}"),
                    refireImmediately: false);
            }

            int deletedCount = result.Value;
            var duration = DateTimeOffset.UtcNow - startTime;

            if (deletedCount == 0)
            {
                logger.LogInformation(
                    message: "Refresh token cleanup completed successfully with no tokens to clean. Duration: {Duration}ms",
                    args: duration.TotalMilliseconds);
            }
            else if (deletedCount < WarningThreshold)
            {
                logger.LogInformation(
                    message: "Successfully cleaned up {Count} expired/revoked refresh tokens. Duration: {Duration}ms",
                    args:
                    [
                        deletedCount,
                        duration.TotalMilliseconds
                    ]);
            }
            else if (deletedCount < ErrorThreshold)
            {
                logger.LogWarning(
                    message: "Cleaned up {Count} tokens (above normal threshold of {Threshold}). Consider reviewing token lifecycle. Duration: {Duration}ms",
                    args:
                    [
                        deletedCount,
                        WarningThreshold,
                        duration.TotalMilliseconds
                    ]);
            }
            else
            {
                logger.LogError(
                    message: "Cleaned up {Count} tokens (critically high, threshold: {Threshold}). Investigate token generation/revocation patterns. Duration: {Duration}ms",
                    args:
                    [
                        deletedCount,
                        ErrorThreshold,
                        duration.TotalMilliseconds
                    ]);
            }

            context.Result = new
            {
                DeletedCount = deletedCount,
                Duration = duration.TotalMilliseconds,
                Success = true
            };
        }
        catch (JobExecutionException)
        {
            throw;
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(message: "Refresh token cleanup job was cancelled");
            throw new JobExecutionException(cause: new OperationCanceledException(message: "Job was cancelled"),
                refireImmediately: false);
        }
        catch (Exception ex)
        {
            logger.LogError(exception: ex,
                message: "Refresh token cleanup job failed unexpectedly");

            throw new JobExecutionException(cause: ex,
                refireImmediately: false);
        }
    }
}

using ReSys.Shop.Core.Common.Services.Notification.Interfaces;
using ReSys.Shop.Core.Common.Services.Notification.Models;

namespace ReSys.Shop.Infrastructure.Notifications.Services;

public sealed class EmptyEmailSenderService : IEmailSenderService
{
    public Task<ErrorOr<Success>> AddEmailNotificationAsync(
        EmailNotificationData notificationData,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<ErrorOr<Success>>(
            result: Error.Unexpected(
                code: "EmailSender.Disabled",
                description: "Email sender is not available. Email sending is disabled in this environment."));
    }
}
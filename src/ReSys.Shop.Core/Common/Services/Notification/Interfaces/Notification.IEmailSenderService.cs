using ReSys.Shop.Core.Common.Services.Notification.Models;

namespace ReSys.Shop.Core.Common.Services.Notification.Interfaces;

public interface IEmailSenderService
{
    Task<ErrorOr<Success>> AddEmailNotificationAsync(
        EmailNotificationData notificationData,
        CancellationToken cancellationToken = default);
}

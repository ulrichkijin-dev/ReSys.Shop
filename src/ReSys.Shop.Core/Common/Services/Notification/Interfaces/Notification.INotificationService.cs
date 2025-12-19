using ReSys.Shop.Core.Common.Services.Notification.Models;

namespace ReSys.Shop.Core.Common.Services.Notification.Interfaces;

public interface INotificationService
{
    Task<ErrorOr<Success>> AddNotificationAsync(NotificationData notification, CancellationToken cancellationToken);
}

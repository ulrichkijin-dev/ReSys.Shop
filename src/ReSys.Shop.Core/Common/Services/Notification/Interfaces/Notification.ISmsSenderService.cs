using ReSys.Shop.Core.Common.Services.Notification.Models;

namespace ReSys.Shop.Core.Common.Services.Notification.Interfaces;

public interface ISmsSenderService
{
    public Task<ErrorOr<Success>> AddSmsNotificationAsync(
        SmsNotificationData notificationData, 
        CancellationToken cancellationToken = default);
}

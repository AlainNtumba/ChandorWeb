using ChandorProject.Shared.DTOs.Notification;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface INotificationService
{
    Task<DataResponse<NotificationDto>?> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<NotificationDto>>?> GetAllAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<NotificationDto>>?> GetUserNotExpiredAsync(Guid userId, CancellationToken cancellationToken = default);
}

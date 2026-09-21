using ChandorProject.Shared.DTOs.Notification;
using ChandorProject.Shared.Models;
using ChandorAdmin.Models.Notification;

namespace ChandorAdmin.Interfaces.Api;

public interface INotificationService
{
    Task<DataResponse<NotificationDto>?> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<NotificationDto>>?> GetAllAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<NotificationDto>>?> GetUserNotExpiredAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<DataResponse<PagedResult<NotificationAdminDto>>?> GetAdminAsync(NotificationAdminFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<NotificationSummaryDto>?> GetAdminSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<DataResponse<NotificationAdminDto>?> GetAdminByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResponse<PagedResult<NotificationRecipientAdminDto>>?> GetRecipientsAsync(Guid id, NotificationRecipientFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<NotificationAdminDto>?> CreateAdminAsync(NotificationInputDto dto, CancellationToken cancellationToken = default);
    Task<DataResponse<NotificationAdminDto>?> UpdateAdminAsync(Guid id, NotificationInputDto dto, CancellationToken cancellationToken = default);
    Task<DataResponse<NotificationAdminDto>?> PublishAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResponse<NotificationAdminDto>?> MoveToDraftAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResponse<NotificationAdminDto>?> ArchiveAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteAdminAsync(Guid id, CancellationToken cancellationToken = default);
}

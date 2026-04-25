using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Notification;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class NotificationService(ChandorApiHttp api) : INotificationService
{
    private const string C = "Notification";

    public Task<DataResponse<NotificationDto>?> CreateAsync(CreateNotificationDto dto, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<NotificationDto>($"{C}/create", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<IEnumerable<NotificationDto>>?> GetAllAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<NotificationDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IEnumerable<NotificationDto>>?> GetUserNotExpiredAsync(Guid userId, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<NotificationDto>>($"{C}/get-user-not-expired/{userId}", cancellationToken);
}

using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Notification;
using ChandorProject.Shared.Models;
using ChandorAdmin.Models.Notification;
using System.Text;

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

    public Task<DataResponse<PagedResult<NotificationAdminDto>>?> GetAdminAsync(NotificationAdminFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<PagedResult<NotificationAdminDto>>($"notifications/admin{BuildQuery(
            ("page", filter.Page.ToString()), ("pageSize", filter.PageSize.ToString()), ("status", filter.Status),
            ("audienceType", filter.AudienceType), ("keyword", filter.Keyword), ("fromDate", Date(filter.FromDate)),
            ("toDate", Date(filter.ToDate, true)), ("isActive", filter.IsActive?.ToString().ToLowerInvariant()))}", cancellationToken);

    public Task<DataResponse<NotificationSummaryDto>?> GetAdminSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<NotificationSummaryDto>($"notifications/admin/summary{BuildQuery(("fromDate", Date(fromDate)), ("toDate", Date(toDate, true)))}", cancellationToken);

    public Task<DataResponse<NotificationAdminDto>?> GetAdminByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<NotificationAdminDto>($"notifications/{id:D}", cancellationToken);

    public Task<DataResponse<PagedResult<NotificationRecipientAdminDto>>?> GetRecipientsAsync(Guid id, NotificationRecipientFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<PagedResult<NotificationRecipientAdminDto>>($"notifications/{id:D}/recipients{BuildQuery(
            ("page", filter.Page.ToString()), ("pageSize", filter.PageSize.ToString()), ("state", filter.State), ("keyword", filter.Keyword))}", cancellationToken);

    public Task<DataResponse<NotificationAdminDto>?> CreateAdminAsync(NotificationInputDto dto, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<NotificationAdminDto>("notifications", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<NotificationAdminDto>?> UpdateAdminAsync(Guid id, NotificationInputDto dto, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<NotificationAdminDto>($"notifications/{id:D}", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<NotificationAdminDto>?> PublishAsync(Guid id, CancellationToken cancellationToken = default)
        => PatchAsync(id, "publish", cancellationToken);

    public Task<DataResponse<NotificationAdminDto>?> MoveToDraftAsync(Guid id, CancellationToken cancellationToken = default)
        => PatchAsync(id, "draft", cancellationToken);

    public Task<DataResponse<NotificationAdminDto>?> ArchiveAsync(Guid id, CancellationToken cancellationToken = default)
        => PatchAsync(id, "archive", cancellationToken);

    public Task<DataResponse<bool>?> DeleteAdminAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"notifications/{id:D}", cancellationToken);

    private Task<DataResponse<NotificationAdminDto>?> PatchAsync(Guid id, string action, CancellationToken cancellationToken)
        => api.PatchDataResponseAsync<NotificationAdminDto>($"notifications/{id:D}/{action}", new StringContent("{}", Encoding.UTF8, "application/json"), cancellationToken);

    private static string? Date(DateTime? value, bool endOfDay = false)
    {
        if (!value.HasValue) return null;
        var date = endOfDay ? value.Value.Date.AddDays(1).AddTicks(-1) : value.Value.Date;
        return date.ToString("O");
    }

    private static string BuildQuery(params (string Name, string? Value)[] values)
    {
        var query = values.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{Uri.EscapeDataString(x.Name)}={Uri.EscapeDataString(x.Value!)}");
        var text = string.Join("&", query); return text.Length == 0 ? string.Empty : $"?{text}";
    }
}

using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.MemberRequest;
using ChandorProject.Shared.DTOs.MemberRequest;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class MemberRequestService(ChandorApiHttp api) : IMemberRequestService
{
    private const string Root = "member-requests";

    public Task<DataResponse<PagedResult<MemberRequestDto>>?> GetPagedAsync(MemberRequestFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<PagedResult<MemberRequestDto>>($"{Root}{BuildQuery(
            ("type", filter.Type), ("status", filter.Status), ("memberId", Id(filter.MemberId)),
            ("keyword", filter.Keyword), ("fromDate", Date(filter.FromDate)), ("toDate", Date(filter.ToDate, endOfDay: true)),
            ("page", filter.Page.ToString()), ("pageSize", filter.PageSize.ToString()))}", cancellationToken);

    public Task<DataResponse<MemberRequestSummaryDto>?> GetSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberRequestSummaryDto>($"{Root}/summary{BuildQuery(("fromDate", Date(fromDate)), ("toDate", Date(toDate, endOfDay: true)))}", cancellationToken);

    public Task<DataResponse<MemberRequestDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberRequestDto>($"{Root}/{id:D}", cancellationToken);

    public Task<DataResponse<MemberRequestDto>?> UpdateAsync(Guid id, UpdateMemberRequestDto input, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MemberRequestDto>($"{Root}/{id:D}", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<MemberRequestDto>?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default)
        => api.PatchDataResponseAsync<MemberRequestDto>($"{Root}/{id:D}/status", JsonContent.Create(new UpdateMemberRequestStatusDto { Status = status }), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Root}/{id:D}", cancellationToken);

    private static string? Id(Guid? value) => value is { } id && id != Guid.Empty ? id.ToString("D") : null;
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

using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.MemberRequest;
using ChandorProject.Shared.DTOs.RequestType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class RequestTypeService(ChandorApiHttp api) : IRequestTypeService
{
    private const string Root = "request-types";

    public Task<DataResponse<PagedResult<RequestTypeDto>>?> GetActiveAsync(int pageSize = 100, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<PagedResult<RequestTypeDto>>($"{Root}?page=1&pageSize={pageSize}", cancellationToken);

    public Task<DataResponse<PagedResult<RequestTypeDto>>?> GetAdminAsync(RequestTypeFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<PagedResult<RequestTypeDto>>($"{Root}/admin{BuildQuery(
            ("isActive", filter.IsActive?.ToString().ToLowerInvariant()), ("keyword", filter.Keyword),
            ("page", filter.Page.ToString()), ("pageSize", filter.PageSize.ToString()))}", cancellationToken);

    public Task<DataResponse<RequestTypeDto>?> CreateAsync(CreateRequestTypeDto input, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<RequestTypeDto>(Root, JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<RequestTypeDto>?> UpdateAsync(Guid id, UpdateRequestTypeDto input, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<RequestTypeDto>($"{Root}/{id:D}", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Root}/{id:D}", cancellationToken);

    private static string BuildQuery(params (string Name, string? Value)[] values)
    {
        var query = values.Where(x => !string.IsNullOrWhiteSpace(x.Value)).Select(x => $"{Uri.EscapeDataString(x.Name)}={Uri.EscapeDataString(x.Value!)}");
        return $"?{string.Join("&", query)}";
    }
}

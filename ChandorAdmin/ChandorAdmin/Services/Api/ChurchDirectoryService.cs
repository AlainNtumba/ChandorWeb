using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.ChurchDirectory;
using ChandorProject.Shared.DTOs.ChurchDirectory;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class ChurchDirectoryService(ChandorApiHttp api) : IChurchDirectoryService
{
    private const string Types = "church-directory/types";
    private const string Items = "church-directory/items";

    public Task<DataResponse<PagedResult<ChurchDirectoryTypeDto>>?> GetAdminTypesAsync(DirectoryFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<PagedResult<ChurchDirectoryTypeDto>>($"{Types}/admin{TypeQuery(filter)}", cancellationToken);

    public Task<DataResponse<ChurchDirectoryTypeDto>?> GetAdminTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<ChurchDirectoryTypeDto>($"{Types}/admin/{id:D}", cancellationToken);

    public Task<DataResponse<PagedResult<ChurchDirectoryTypeDto>>?> GetPublicTypesAsync(DirectoryFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<PagedResult<ChurchDirectoryTypeDto>>($"{Types}{TypeQuery(filter)}", cancellationToken);

    public Task<DataResponse<ChurchDirectoryTypeDto>?> CreateTypeAsync(ChurchDirectoryTypeInputDto input, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<ChurchDirectoryTypeDto>(Types, JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<ChurchDirectoryTypeDto>?> UpdateTypeAsync(Guid id, ChurchDirectoryTypeInputDto input, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<ChurchDirectoryTypeDto>($"{Types}/{id:D}", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<ChurchDirectoryTypeDto>?> UploadTypeHeroAsync(Guid id, DirectoryImageUpload image, CancellationToken cancellationToken = default)
        => UploadAsync<ChurchDirectoryTypeDto>($"{Types}/{id:D}/hero-image", image, cancellationToken);

    public Task<DataResponse<bool>?> DeleteTypeHeroAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Types}/{id:D}/hero-image", cancellationToken);

    public Task<DataResponse<bool>?> DeleteTypeAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Types}/{id:D}", cancellationToken);

    public Task<DataResponse<PagedResult<ChurchDirectoryItemDto>>?> GetAdminItemsAsync(DirectoryFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<PagedResult<ChurchDirectoryItemDto>>($"{Items}/admin{ItemQuery(filter)}", cancellationToken);

    public Task<DataResponse<PagedResult<ChurchDirectoryItemDto>>?> GetPublicItemsAsync(Guid typeId, DirectoryFilterState filter, bool rootsOnly = false, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<PagedResult<ChurchDirectoryItemDto>>($"{Items}{BuildQuery(
            ("typeId", typeId.ToString("D")),
            ("page", filter.Page.ToString()),
            ("pageSize", filter.PageSize.ToString()),
            ("isActive", Bool(filter.IsActive ?? true)),
            ("parentId", Id(filter.ParentId)),
            ("keyword", filter.Keyword),
            ("rootsOnly", rootsOnly.ToString().ToLowerInvariant()))}", cancellationToken);

    public Task<DataResponse<IReadOnlyList<ChurchDirectoryTreeItemDto>>?> GetTreeAsync(Guid typeId, bool? isActive, CancellationToken cancellationToken = default)
    {
        var activeValue = isActive.HasValue ? Bool(isActive.Value) : string.Empty;
        return api.GetDataResponseAsync<IReadOnlyList<ChurchDirectoryTreeItemDto>>(
            $"{Items}/tree?typeId={typeId:D}&isActive={Uri.EscapeDataString(activeValue)}", cancellationToken);
    }

    public Task<DataResponse<ChurchDirectoryItemDto>?> CreateItemAsync(ChurchDirectoryItemInputDto input, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<ChurchDirectoryItemDto>(Items, JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<ChurchDirectoryItemDto>?> UpdateItemAsync(Guid id, ChurchDirectoryItemInputDto input, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<ChurchDirectoryItemDto>($"{Items}/{id:D}", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<ChurchDirectoryItemDto>?> UploadItemImageAsync(Guid id, DirectoryImageUpload image, CancellationToken cancellationToken = default)
        => UploadAsync<ChurchDirectoryItemDto>($"{Items}/{id:D}/image", image, cancellationToken);

    public Task<DataResponse<bool>?> DeleteItemImageAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Items}/{id:D}/image", cancellationToken);

    public Task<DataResponse<bool>?> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Items}/{id:D}", cancellationToken);

    private async Task<DataResponse<T>?> UploadAsync<T>(string path, DirectoryImageUpload image, CancellationToken cancellationToken)
    {
        using var form = new MultipartFormDataContent();
        var content = new ByteArrayContent(image.Content);
        content.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
        form.Add(content, "file", image.FileName);
        return await api.PutDataResponseAsync<T>(path, form, cancellationToken);
    }

    private static string TypeQuery(DirectoryFilterState filter) => BuildQuery(
        ("page", filter.Page.ToString()),
        ("pageSize", filter.PageSize.ToString()),
        ("isActive", filter.IsActive.HasValue ? Bool(filter.IsActive.Value) : null),
        ("keyword", filter.Keyword));

    private static string ItemQuery(DirectoryFilterState filter) => BuildQuery(
        ("typeId", Id(filter.TypeId)),
        ("page", filter.Page.ToString()),
        ("pageSize", filter.PageSize.ToString()),
        ("isActive", filter.IsActive.HasValue ? Bool(filter.IsActive.Value) : null),
        ("parentId", Id(filter.ParentId)),
        ("keyword", filter.Keyword));

    private static string? Id(Guid? value) => value is { } id && id != Guid.Empty ? id.ToString("D") : null;
    private static string Bool(bool value) => value.ToString().ToLowerInvariant();

    private static string BuildQuery(params (string Name, string? Value)[] values)
    {
        var query = values.Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .Select(x => $"{Uri.EscapeDataString(x.Name)}={Uri.EscapeDataString(x.Value!)}");
        return $"?{string.Join("&", query)}";
    }
}

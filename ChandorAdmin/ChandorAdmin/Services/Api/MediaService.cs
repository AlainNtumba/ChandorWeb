using System.Net.Http.Headers;
using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.Media;
using ChandorProject.Shared.DTOs.Media;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class MediaService(ChandorApiHttp api) : IMediaService
{
    private const string Root = "media";

    public Task<DataResponse<IReadOnlyList<MediaCategoryDto>>?> GetCategoriesAsync(bool includeInactive = true, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<MediaCategoryDto>>($"{Root}/admin/categories?includeInactive={includeInactive.ToString().ToLowerInvariant()}", cancellationToken);

    public Task<DataResponse<MediaCollectionPageDto>?> GetCollectionsAsync(MediaFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MediaCollectionPageDto>($"{Root}/admin/collections{BuildQuery(
            ("categoryId", Id(filter.CategoryId)),
            ("status", filter.Status),
            ("keyword", filter.Keyword),
            ("page", filter.Page.ToString()),
            ("pageSize", filter.PageSize.ToString()))}", cancellationToken);

    public Task<DataResponse<ContentItemPageDto>?> GetItemsAsync(MediaFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<ContentItemPageDto>($"{Root}/admin/items{BuildQuery(
            ("categoryId", Id(filter.CategoryId)),
            ("collectionId", Id(filter.CollectionId)),
            ("status", filter.Status),
            ("itemType", filter.ItemType),
            ("keyword", filter.Keyword),
            ("page", filter.Page.ToString()),
            ("pageSize", filter.PageSize.ToString()))}", cancellationToken);

    public Task<DataResponse<MediaAssetPageDto>?> GetAssetsAsync(MediaFilterState filter, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MediaAssetPageDto>($"{Root}/admin/assets{BuildQuery(
            ("assetType", filter.AssetType),
            ("provider", filter.Provider),
            ("keyword", filter.Keyword),
            ("page", filter.Page.ToString()),
            ("pageSize", filter.PageSize.ToString()))}", cancellationToken);

    public Task<DataResponse<MediaFeedPageDto>?> GetFeedAsync(Guid categoryId, string period, string keyword, int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MediaFeedPageDto>($"{Root}/feed{BuildQuery(
            ("categoryId", categoryId.ToString("D")),
            ("period", period),
            ("keyword", keyword),
            ("page", page.ToString()),
            ("pageSize", pageSize.ToString()))}", cancellationToken);

    public Task<DataResponse<MediaFeedDetailDto>?> GetFeedItemAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MediaFeedDetailDto>($"{Root}/feed/items/{id:D}", cancellationToken);

    public Task<DataResponse<MediaCategoryDto>?> CreateCategoryAsync(MediaCategoryInputDto input, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MediaCategoryDto>($"{Root}/categories", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<MediaCategoryDto>?> UpdateCategoryAsync(Guid id, MediaCategoryInputDto input, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MediaCategoryDto>($"{Root}/categories/{id:D}", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<bool>?> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Root}/categories/{id:D}", cancellationToken);

    public Task<DataResponse<MediaCollectionDto>?> CreateCollectionAsync(MediaCollectionInputDto input, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MediaCollectionDto>($"{Root}/collections", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<MediaCollectionDto>?> UpdateCollectionAsync(Guid id, MediaCollectionInputDto input, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MediaCollectionDto>($"{Root}/collections/{id:D}", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<bool>?> DeleteCollectionAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Root}/collections/{id:D}", cancellationToken);

    public Task<DataResponse<ContentItemDto>?> CreateItemAsync(ContentItemInputDto input, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<ContentItemDto>($"{Root}/items", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<ContentItemDto>?> UpdateItemAsync(Guid id, ContentItemInputDto input, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<ContentItemDto>($"{Root}/items/{id:D}", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<bool>?> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Root}/items/{id:D}", cancellationToken);

    public Task<DataResponse<MediaAssetDto>?> CreateExternalAssetAsync(MediaAssetInputDto input, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MediaAssetDto>($"{Root}/assets", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<MediaAssetDto>?> UpdateAssetAsync(Guid id, MediaAssetInputDto input, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MediaAssetDto>($"{Root}/assets/{id:D}", JsonContent.Create(input), cancellationToken);

    public async Task<DataResponse<MediaAssetDto>?> UploadAssetAsync(MediaUploadFile file, string? thumbnailUrl, int? durationSeconds, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(file.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, "file", file.FileName);
        if (!string.IsNullOrWhiteSpace(thumbnailUrl))
            form.Add(new StringContent(thumbnailUrl.Trim()), "thumbnailUrl");
        if (durationSeconds.HasValue)
            form.Add(new StringContent(durationSeconds.Value.ToString()), "durationSeconds");
        return await api.PostMultipartDataResponseAsync<MediaAssetDto>($"{Root}/assets/upload", form, cancellationToken);
    }

    public Task<DataResponse<bool>?> DeleteAssetAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Root}/assets/{id:D}", cancellationToken);

    public Task<DataResponse<bool>?> AttachCollectionMediaAsync(Guid collectionId, MediaAttachmentInputDto input, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<bool>($"{Root}/collections/{collectionId:D}/media", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<bool>?> AttachItemMediaAsync(Guid itemId, MediaAttachmentInputDto input, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<bool>($"{Root}/items/{itemId:D}/media", JsonContent.Create(input), cancellationToken);

    public Task<DataResponse<bool>?> DetachCollectionMediaAsync(Guid collectionId, Guid assetId, string? role, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Root}/collections/{collectionId:D}/media/{assetId:D}{BuildQuery(("role", role))}", cancellationToken);

    public Task<DataResponse<bool>?> DetachItemMediaAsync(Guid itemId, Guid assetId, string? role, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Root}/items/{itemId:D}/media/{assetId:D}{BuildQuery(("role", role))}", cancellationToken);

    private static string? Id(Guid? id) => id is { } value && value != Guid.Empty ? value.ToString("D") : null;

    private static string BuildQuery(params (string Name, string? Value)[] values)
    {
        var query = values
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value))
            .Select(pair => $"{Uri.EscapeDataString(pair.Name)}={Uri.EscapeDataString(pair.Value!)}");
        var joined = string.Join("&", query);
        return joined.Length == 0 ? string.Empty : $"?{joined}";
    }
}

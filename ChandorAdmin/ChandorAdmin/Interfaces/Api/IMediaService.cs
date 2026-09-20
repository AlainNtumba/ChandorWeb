using ChandorAdmin.Models.Media;
using ChandorProject.Shared.DTOs.Media;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IMediaService
{
    Task<DataResponse<IReadOnlyList<MediaCategoryDto>>?> GetCategoriesAsync(bool includeInactive = true, CancellationToken cancellationToken = default);
    Task<DataResponse<MediaCollectionPageDto>?> GetCollectionsAsync(MediaFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<ContentItemPageDto>?> GetItemsAsync(MediaFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<MediaAssetPageDto>?> GetAssetsAsync(MediaFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<MediaFeedPageDto>?> GetFeedAsync(Guid categoryId, string period, string keyword, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<DataResponse<MediaFeedDetailDto>?> GetFeedItemAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MediaCategoryDto>?> CreateCategoryAsync(MediaCategoryInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<MediaCategoryDto>?> UpdateCategoryAsync(Guid id, MediaCategoryInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteCategoryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MediaCollectionDto>?> CreateCollectionAsync(MediaCollectionInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<MediaCollectionDto>?> UpdateCollectionAsync(Guid id, MediaCollectionInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteCollectionAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<ContentItemDto>?> CreateItemAsync(ContentItemInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<ContentItemDto>?> UpdateItemAsync(Guid id, ContentItemInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MediaAssetDto>?> CreateExternalAssetAsync(MediaAssetInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<MediaAssetDto>?> UpdateAssetAsync(Guid id, MediaAssetInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<MediaAssetDto>?> UploadAssetAsync(MediaUploadFile file, string? thumbnailUrl, int? durationSeconds, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteAssetAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> AttachCollectionMediaAsync(Guid collectionId, MediaAttachmentInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> AttachItemMediaAsync(Guid itemId, MediaAttachmentInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DetachCollectionMediaAsync(Guid collectionId, Guid assetId, string? role, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DetachItemMediaAsync(Guid itemId, Guid assetId, string? role, CancellationToken cancellationToken = default);
}

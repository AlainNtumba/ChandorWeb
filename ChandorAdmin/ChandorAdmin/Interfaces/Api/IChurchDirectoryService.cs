using ChandorAdmin.Models.ChurchDirectory;
using ChandorProject.Shared.DTOs.ChurchDirectory;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IChurchDirectoryService
{
    Task<DataResponse<PagedResult<ChurchDirectoryTypeDto>>?> GetAdminTypesAsync(DirectoryFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<ChurchDirectoryTypeDto>?> GetAdminTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResponse<PagedResult<ChurchDirectoryTypeDto>>?> GetPublicTypesAsync(DirectoryFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<ChurchDirectoryTypeDto>?> CreateTypeAsync(ChurchDirectoryTypeInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<ChurchDirectoryTypeDto>?> UpdateTypeAsync(Guid id, ChurchDirectoryTypeInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<ChurchDirectoryTypeDto>?> UploadTypeHeroAsync(Guid id, DirectoryImageUpload image, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteTypeHeroAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<PagedResult<ChurchDirectoryItemDto>>?> GetAdminItemsAsync(DirectoryFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<PagedResult<ChurchDirectoryItemDto>>?> GetPublicItemsAsync(Guid typeId, DirectoryFilterState filter, bool rootsOnly = false, CancellationToken cancellationToken = default);
    Task<DataResponse<IReadOnlyList<ChurchDirectoryTreeItemDto>>?> GetTreeAsync(Guid typeId, bool? isActive, CancellationToken cancellationToken = default);
    Task<DataResponse<ChurchDirectoryItemDto>?> CreateItemAsync(ChurchDirectoryItemInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<ChurchDirectoryItemDto>?> UpdateItemAsync(Guid id, ChurchDirectoryItemInputDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<ChurchDirectoryItemDto>?> UploadItemImageAsync(Guid id, DirectoryImageUpload image, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteItemImageAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteItemAsync(Guid id, CancellationToken cancellationToken = default);
}

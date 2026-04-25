using ChandorProject.Shared.DTOs.AgeGroup;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IAgeGroupService
{
    Task<DataResponse<AgeGroupDto>?> CreateAgeGroupAsync(NewAgeGroupDto ageGroup, CancellationToken cancellationToken = default);

    Task<DataResponse<AgeGroupDto>?> UpdateAgeGroupAsync(AgeGroupDto ageGroup, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteAgeGroupAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<AgeGroupDto>?> GetAgeGroupByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<AgeGroupDto>>?> GetAllAgeGroupsAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<AgeGroupDto>>?> GetPagedAgeGroupsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

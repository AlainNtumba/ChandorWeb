using ChandorProject.Shared.DTOs.Ministry;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IMinistryService
{
    Task<DataResponse<MinistryDto>?> CreateMinistryAsync(NewMinistryDto ministry, CancellationToken cancellationToken = default);

    Task<DataResponse<MinistryDto>?> UpdateMinistryAsync(MinistryDto ministry, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteMinistryAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MinistryDto>?> GetMinistryByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<MinistryDto>>?> GetAllMinistriesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<MinistryDto>>?> GetPagedMinistriesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

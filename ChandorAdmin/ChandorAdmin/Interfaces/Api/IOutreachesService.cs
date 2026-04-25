using ChandorProject.Shared.DTOs.Outreaches;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IOutreachesService
{
    Task<DataResponse<OutreachesDto>?> CreateOutreachesAsync(NewOutreachesDto outreaches, CancellationToken cancellationToken = default);

    Task<DataResponse<OutreachesDto>?> UpdateOutreachesAsync(OutreachesDto outreaches, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteOutreachesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<OutreachesDto>?> GetOutreachesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<OutreachesDto>>?> GetAllOutreachesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<OutreachesDto>>?> GetPagedOutreachesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

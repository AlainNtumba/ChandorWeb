using ChandorProject.Shared.DTOs.MinistiesSchedule;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IMinistiesScheduleService
{
    Task<DataResponse<MinistiesScheduleDto>?> CreateMinistiesScheduleAsync(NewMinistiesScheduleDto ministiesSchedule, CancellationToken cancellationToken = default);

    Task<DataResponse<MinistiesScheduleDto>?> UpdateMinistiesScheduleAsync(MinistiesScheduleDto ministiesSchedule, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteMinistiesScheduleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MinistiesScheduleDto>?> GetMinistiesScheduleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<MinistiesScheduleDto>>?> GetAllMinistiesSchedulesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<MinistiesScheduleDto>>?> GetPagedMinistiesSchedulesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

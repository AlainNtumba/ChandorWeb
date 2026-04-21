using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IChurchProgramService
{
    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetCongregationProgramsAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<ChurchProgramDto>?> AddProgramAsync(NewChurchProgramDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<ChurchProgramDto>?> UpdateProgramAsync(ChurchProgramDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteProgramAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetDepartmentProgramAsync(Guid departmentId, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetTeamProgramAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetUpcomingEventsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);
}

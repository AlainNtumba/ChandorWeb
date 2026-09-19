using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.Models;
using ChandorAdmin.Models.ChurchProgram;

namespace ChandorAdmin.Interfaces.Api;

public interface IChurchProgramService
{
    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetCongregationProgramsAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetPeriodicCongregationProgramsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    Task<DataResponse<ChurchProgramDto>?> AddProgramAsync(NewChurchProgramDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<ChurchProgramDto>?> UpdateProgramAsync(ChurchProgramDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteProgramAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetDepartmentProgramAsync(
        Guid departmentId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetTeamProgramAsync(Guid teamId, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetUpcomingEventsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default);

    Task<DataResponse<ChurchProgramDto>?> AddCongregationProgramAsync(
        CongregationProgramDto dto,
        ChurchProgramPosterUpload poster,
        CancellationToken cancellationToken = default);

    Task<DataResponse<ChurchProgramDto>?> AddOrReplacePosterAsync(
        Guid programId,
        ChurchProgramPosterUpload poster,
        CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeletePosterAsync(Guid programId, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetPaginatedCongregationProgramsFeedAsync(
        DateTime? fromDate,
        DateTime? toDate,
        int take = 10,
        int skip = 0,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetCongregationProgramsByKeywordAsync(
        string keyword,
        int take = 10,
        int skip = 0,
        CancellationToken cancellationToken = default);
}

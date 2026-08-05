using System.Net.Http.Json;
using ChandorAdmin.Helpers;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class ChurchProgramService(ChandorApiHttp api) : IChurchProgramService
{
    private const string C = "ChurchProgram";

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetCongregationProgramsAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get_congration_programs", cancellationToken);

    public Task<DataResponse<ChurchProgramDto>?> AddProgramAsync(NewChurchProgramDto dto, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<ChurchProgramDto>($"{C}/add-church-program", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<ChurchProgramDto>?> UpdateProgramAsync(ChurchProgramDto dto, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<ChurchProgramDto>($"{C}/update-church-program", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<bool>?> DeleteProgramAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete_church_program/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetDepartmentProgramAsync(
        Guid departmentId,
        DateTime start,
        DateTime end,
        CancellationToken cancellationToken = default)
    {
        var q =
            $"departmentId={departmentId}" +
            $"&start={ApiDateQueryFormatter.FormatQueryValue(start, asUtc: true)}" +
            $"&end={ApiDateQueryFormatter.FormatQueryValue(end, asUtc: true)}";
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get-department-program?{q}", cancellationToken);
    }

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetTeamProgramAsync(Guid teamId, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get-team-program/{teamId}", cancellationToken);

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetUpcomingEventsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var q = $"start={ApiDateQueryFormatter.FormatQueryValue(start, asUtc: true)}&end={ApiDateQueryFormatter.FormatQueryValue(end, asUtc: true)}";
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get-upcoming-events?{q}", cancellationToken);
    }

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetPeriodicCongregationProgramsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var q = $"start={ApiDateQueryFormatter.FormatQueryValue(start, asUtc: true)}&end={ApiDateQueryFormatter.FormatQueryValue(end, asUtc: true)}";
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get_periodic_congration_programs?{q}", cancellationToken);
    }

    public Task<DataResponse<ChurchProgramDto>?> AddCongregationProgramAsync(CongregationProgramDto dto, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<ChurchProgramDto>($"{C}/add-congregation-program", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetPaginatedCongregationProgramsFeedAsync(
        DateTime? fromDate,
        DateTime? toDate,
        int take = 10,
        int skip = 0,
        CancellationToken cancellationToken = default)
    {
        var parts = new List<string> { $"take={take}", $"skip={skip}" };
        if (fromDate is { } fd)
            parts.Add($"fromDate={ApiDateQueryFormatter.FormatQueryValue(fd, asUtc: true)}");
        if (toDate is { } td)
            parts.Add($"toDate={ApiDateQueryFormatter.FormatQueryValue(td, asUtc: true)}");
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get_paginatedfeed_congration_programs?{string.Join("&", parts)}", cancellationToken);
    }

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetCongregationProgramsByKeywordAsync(
        string keyword,
        int take = 10,
        int skip = 0,
        CancellationToken cancellationToken = default)
    {
        var q = $"keyword={Uri.EscapeDataString(keyword ?? string.Empty)}&take={take}&skip={skip}";
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get_congration_programs_bykeyword?{q}", cancellationToken);
    }
}

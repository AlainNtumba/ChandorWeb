using System.Net.Http.Json;
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

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetDepartmentProgramAsync(Guid departmentId, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get-department-program/{departmentId}", cancellationToken);

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetTeamProgramAsync(Guid teamId, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get-team-program/{teamId}", cancellationToken);

    public Task<DataResponse<IEnumerable<ChurchProgramDto>>?> GetUpcomingEventsAsync(DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var q = $"start={Uri.EscapeDataString(start.ToUniversalTime().ToString("o"))}&end={Uri.EscapeDataString(end.ToUniversalTime().ToString("o"))}";
        return api.GetDataResponseAsync<IEnumerable<ChurchProgramDto>>($"{C}/get-upcoming-events?{q}", cancellationToken);
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
            parts.Add($"fromDate={Uri.EscapeDataString(fd.ToUniversalTime().ToString("o"))}");
        if (toDate is { } td)
            parts.Add($"toDate={Uri.EscapeDataString(td.ToUniversalTime().ToString("o"))}");
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

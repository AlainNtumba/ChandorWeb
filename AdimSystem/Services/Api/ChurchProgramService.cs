using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

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
}

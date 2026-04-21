using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.DepartmentTeamDto;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class DepartmentTeamService(ChandorApiHttp api) : IDepartmentTeamService
{
    private const string C = "DepartmentTeam";

    public Task<DataResponse<DepartmentTeamDto>?> AddDepartmentTeamAsync(NewDepartmentTeamDto dto, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<DepartmentTeamDto>($"{C}/add-department-team", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<DepartmentTeamDto>?> UpdateDepartmentTeamAsync(DepartmentTeamDto dto, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<DepartmentTeamDto>($"{C}/update-department-team", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<bool>?> DeleteDepartmentTeamAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-department-team/{id}", cancellationToken);

    public Task<DataResponse<DepartmentTeamDto>?> GetDepartmentTeamByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<DepartmentTeamDto>($"{C}/get-department-team-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<DepartmentTeamDto>>?> GetDepartmentTeamsByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<DepartmentTeamDto>>($"{C}/get-department-team-by-department-id/{departmentId}", cancellationToken);
}

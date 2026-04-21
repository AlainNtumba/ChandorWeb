using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class DepartmentService(ChandorApiHttp api) : IDepartmentService
{
    private const string C = "Department";

    public Task<DataResponse<DepartmentDto>?> AddDepartmentAsync(NewDepartmentDto dto, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<DepartmentDto>($"{C}/add-department", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<DepartmentDto>?> UpdateDepartmentAsync(UpdateDepartmentDto dto, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<DepartmentDto>($"{C}/update-department", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<bool>?> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-department/{id}", cancellationToken);

    public Task<DataResponse<DepartmentDto>?> GetDepartmentAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<DepartmentDto>($"{C}/get-department/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<DepartmentDto>>?> GetDepartmentsAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<DepartmentDto>>($"{C}/get-departments", cancellationToken);
}

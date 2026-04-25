using System.Net.Http.Json;
using System.Text;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

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

    public Task<DataResponse<IEnumerable<MemberDto>>?> GetDepartmentMembersAsync(Guid departmentId, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<MemberDto>>($"{C}/get-department-members/{departmentId}", cancellationToken);

    public Task<DataResponse<bool>?> AddDepartmentMemberAsync(Guid departmentId, Guid memberId, Guid roleId, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<bool>($"{C}/add-department-member/{departmentId}/{memberId}/{roleId}", new StringContent("{}", Encoding.UTF8, "application/json"), cancellationToken);

    public Task<DataResponse<bool>?> RemoveDepartmentMemberAsync(Guid departmentId, Guid memberId, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/remove-department-member/{departmentId}/{memberId}", cancellationToken);
}

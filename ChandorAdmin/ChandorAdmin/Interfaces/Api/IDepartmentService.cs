using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IDepartmentService
{
    Task<DataResponse<DepartmentDto>?> AddDepartmentAsync(NewDepartmentDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<DepartmentDto>?> UpdateDepartmentAsync(UpdateDepartmentDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<DepartmentDto>?> GetDepartmentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<DepartmentDto>>?> GetDepartmentsAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<MemberDto>>?> GetDepartmentMembersAsync(Guid departmentId, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> AddDepartmentMemberAsync(Guid departmentId, Guid memberId, Guid roleId, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> RemoveDepartmentMemberAsync(Guid departmentId, Guid memberId, CancellationToken cancellationToken = default);

    Task<DataResponse<ChurchDepartmentKeyDto>?> GetChurchDepartmentKeysAsync(CancellationToken cancellationToken = default);
}

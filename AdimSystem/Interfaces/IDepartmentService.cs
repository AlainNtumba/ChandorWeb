using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IDepartmentService
{
    Task<DataResponse<DepartmentDto>?> AddDepartmentAsync(NewDepartmentDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<DepartmentDto>?> UpdateDepartmentAsync(UpdateDepartmentDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteDepartmentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<DepartmentDto>?> GetDepartmentAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<DepartmentDto>>?> GetDepartmentsAsync(CancellationToken cancellationToken = default);
}

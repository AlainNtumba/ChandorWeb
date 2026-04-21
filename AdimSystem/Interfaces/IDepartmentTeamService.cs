using ChandorProject.Shared.DTOs.DepartmentTeamDto;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IDepartmentTeamService
{
    Task<DataResponse<DepartmentTeamDto>?> AddDepartmentTeamAsync(NewDepartmentTeamDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<DepartmentTeamDto>?> UpdateDepartmentTeamAsync(DepartmentTeamDto dto, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteDepartmentTeamAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<DepartmentTeamDto>?> GetDepartmentTeamByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<DepartmentTeamDto>>?> GetDepartmentTeamsByDepartmentIdAsync(Guid departmentId, CancellationToken cancellationToken = default);
}

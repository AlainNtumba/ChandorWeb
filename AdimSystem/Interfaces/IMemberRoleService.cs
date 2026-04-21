using ChandorProject.Shared.DTOs.MemberRole;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IMemberRoleService
{
    Task<DataResponse<MemberRoleDto>?> CreateMemberRoleAsync(NewMemberRoleDto memberRole, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberRoleDto>?> UpdateMemberRoleAsync(MemberRoleDto memberRole, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteMemberRoleAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberRoleDto>?> GetMemberRoleByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<MemberRoleDto>>?> GetAllMemberRolesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<MemberRoleDto>>?> GetPagedMemberRolesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IMemberService
{
    Task<DataResponse<MemberDto>?> AddMemberAsync(NewMemberDto member, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberDto>?> UpdateMemberAsync(UpdateMemberDto member, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberDto>?> GetMemberAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<MemberDto>>?> GetMembersAsync(CancellationToken cancellationToken = default);
}

using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IMemberService
{
    Task<DataResponse<MemberDto>?> AddSimpleMemberAsync(NewMemberDto member, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberDto>?> AddMemberWithUserAsync(MemberRegistrationDto member, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberDto>?> UpdateMemberAsync(UpdateMemberDto member, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberDto>?> GetMemberAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberDto>?> GetMemberByUsernameAsync(string username, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberDto>?> GetMemberByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberDto>?> GetMemberByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberProfileDto>?> GetMemberProfileAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<PasswordResetSessionDto>?> RequestResetPasswordAsync(RequestResetPasswordDto request, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> ConfirmResetPasswordAsync(ConfirmResetPasswordDto request, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<MemberDetailsDto>>?> GetMembersAsync(CancellationToken cancellationToken = default);
}

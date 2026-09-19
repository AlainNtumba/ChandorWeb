using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.Member;
using System.Net.Http.Headers;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class MemberService(ChandorApiHttp api) : IMemberService
{
    private const string C = "Member";

    public Task<DataResponse<MemberDto>?> AddSimpleMemberAsync(NewMemberDto member, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MemberDto>($"{C}/add-simple_member", JsonContent.Create(member), cancellationToken);

    public Task<DataResponse<MemberDto>?> AddMemberWithUserAsync(MemberRegistrationDto member, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MemberDto>($"{C}/add-member-with-user", JsonContent.Create(member), cancellationToken);

    public Task<DataResponse<MemberDto>?> UpdateMemberAsync(UpdateMemberDto member, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MemberDto>($"{C}/update-member", JsonContent.Create(member), cancellationToken);

    public async Task<DataResponse<MemberProfileImageDto>?> UploadProfileImageAsync(
        Guid memberId,
        MemberProfileImageUpload image,
        CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(image.Content);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(image.ContentType);
        form.Add(fileContent, "file", image.FileName);
        return await api.PutDataResponseAsync<MemberProfileImageDto>(
            $"{C}/{memberId:D}/profile-image",
            form,
            cancellationToken).ConfigureAwait(false);
    }

    public Task<DataResponse<bool>?> DeleteProfileImageAsync(Guid memberId, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/{memberId:D}/profile-image", cancellationToken);

    public Task<DataResponse<bool>?> DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-member/{id}", cancellationToken);

    public Task<DataResponse<MemberDto>?> GetMemberAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberDto>($"{C}/get-member/{id}", cancellationToken);

    public Task<DataResponse<MemberDto>?> GetMemberByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberDto>($"{C}/get-member-by-username/{Uri.EscapeDataString(username)}", cancellationToken);

    public Task<DataResponse<MemberDto>?> GetMemberByPhoneNumberAsync(string phoneNumber, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberDto>($"{C}/get-member-by-phone-number/{Uri.EscapeDataString(phoneNumber)}", cancellationToken);

    public Task<DataResponse<MemberDto>?> GetMemberByEmailAddressAsync(string emailAddress, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberDto>($"{C}/get-member-by-email-address/{Uri.EscapeDataString(emailAddress)}", cancellationToken);

    public Task<DataResponse<MemberProfileDto>?> GetMemberProfileAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberProfileDto>($"{C}/get-member-profile/{id}", cancellationToken);

    public Task<DataResponse<PasswordResetSessionDto>?> RequestResetPasswordAsync(RequestResetPasswordDto request, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<PasswordResetSessionDto>($"{C}/request-reset-password", JsonContent.Create(request), cancellationToken);

    public Task<DataResponse<bool>?> ConfirmResetPasswordAsync(ConfirmResetPasswordDto request, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<bool>($"{C}/confirm-reset-password", JsonContent.Create(request), cancellationToken);

    public Task<DataResponse<IEnumerable<MemberDetailsDto>>?> GetMembersAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<MemberDetailsDto>>($"{C}/get-members-details", cancellationToken);

    public Task<DataResponse<IEnumerable<MemberDto>>?> GetAllMembersAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<MemberDto>>($"{C}/get-members", cancellationToken);
}

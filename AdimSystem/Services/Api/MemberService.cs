using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class MemberService(ChandorApiHttp api) : IMemberService
{
    private const string C = "Member";

    public Task<DataResponse<MemberDto>?> AddMemberAsync(NewMemberDto member, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MemberDto>($"{C}/add-member", JsonContent.Create(member), cancellationToken);

    public Task<DataResponse<MemberDto>?> UpdateMemberAsync(UpdateMemberDto member, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MemberDto>($"{C}/update-member", JsonContent.Create(member), cancellationToken);

    public Task<DataResponse<bool>?> DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-member/{id}", cancellationToken);

    public Task<DataResponse<MemberDto>?> GetMemberAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberDto>($"{C}/get-member/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<MemberDto>>?> GetMembersAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<MemberDto>>($"{C}/get-members", cancellationToken);
}

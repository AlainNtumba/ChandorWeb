using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.MemberRole;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class MemberRoleService(ChandorApiHttp api) : IMemberRoleService
{
    private const string C = "MemberRole";

    public Task<DataResponse<MemberRoleDto>?> CreateMemberRoleAsync(NewMemberRoleDto memberRole, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MemberRoleDto>($"{C}/create-member-role", JsonContent.Create(memberRole), cancellationToken);

    public Task<DataResponse<MemberRoleDto>?> UpdateMemberRoleAsync(MemberRoleDto memberRole, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MemberRoleDto>($"{C}/update-member-role", JsonContent.Create(memberRole), cancellationToken);

    public Task<DataResponse<bool>?> DeleteMemberRoleAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-member-role/{id}", cancellationToken);

    public Task<DataResponse<MemberRoleDto>?> GetMemberRoleByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberRoleDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<MemberRoleDto>>?> GetAllMemberRolesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<MemberRoleDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<MemberRoleDto>>?> GetPagedMemberRolesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<MemberRoleDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

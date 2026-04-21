using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.MemberActivity;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class MemberActivityService(ChandorApiHttp api) : IMemberActivityService
{
    private const string C = "MemberActivity";

    public Task<DataResponse<MemberActivityDto>?> CreateMemberActivityAsync(NewMemberActivityDto memberActivity, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MemberActivityDto>($"{C}/create-member-activity", JsonContent.Create(memberActivity), cancellationToken);

    public Task<DataResponse<MemberActivityDto>?> UpdateMemberActivityAsync(MemberActivityDto memberActivity, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MemberActivityDto>($"{C}/update-member-activity", JsonContent.Create(memberActivity), cancellationToken);

    public Task<DataResponse<bool>?> DeleteMemberActivityAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-member-activity/{id}", cancellationToken);

    public Task<DataResponse<MemberActivityDto>?> GetMemberActivityByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberActivityDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<MemberActivityDto>>?> GetAllMemberActivitiesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<MemberActivityDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<MemberActivityDto>>?> GetPagedMemberActivitiesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<MemberActivityDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

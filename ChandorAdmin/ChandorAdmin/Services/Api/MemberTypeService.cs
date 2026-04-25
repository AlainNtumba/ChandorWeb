using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.MemberType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class MemberTypeService(ChandorApiHttp api) : IMemberTypeService
{
    private const string C = "MemberType";

    public Task<DataResponse<MemberTypeDto>?> CreateMemberTypeAsync(NewMemberTypeDto memberType, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MemberTypeDto>($"{C}/create-member-type", JsonContent.Create(memberType), cancellationToken);

    public Task<DataResponse<MemberTypeDto>?> UpdateMemberTypeAsync(MemberTypeDto memberType, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MemberTypeDto>($"{C}/update-member-type", JsonContent.Create(memberType), cancellationToken);

    public Task<DataResponse<bool>?> DeleteMemberTypeAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-member-type/{id}", cancellationToken);

    public Task<DataResponse<MemberTypeDto>?> GetMemberTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MemberTypeDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<MemberTypeDto>>?> GetAllMemberTypesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<MemberTypeDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<MemberTypeDto>>?> GetPagedMemberTypesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<MemberTypeDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

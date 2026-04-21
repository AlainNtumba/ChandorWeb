using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.AgeGroup;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class AgeGroupService(ChandorApiHttp api) : IAgeGroupService
{
    private const string C = "AgeGroup";

    public Task<DataResponse<AgeGroupDto>?> CreateAgeGroupAsync(NewAgeGroupDto ageGroup, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<AgeGroupDto>($"{C}/create-age-group", JsonContent.Create(ageGroup), cancellationToken);

    public Task<DataResponse<AgeGroupDto>?> UpdateAgeGroupAsync(AgeGroupDto ageGroup, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<AgeGroupDto>($"{C}/update-age-group", JsonContent.Create(ageGroup), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAgeGroupAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-age-group/{id}", cancellationToken);

    public Task<DataResponse<AgeGroupDto>?> GetAgeGroupByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<AgeGroupDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<AgeGroupDto>>?> GetAllAgeGroupsAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<AgeGroupDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<AgeGroupDto>>?> GetPagedAgeGroupsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<AgeGroupDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

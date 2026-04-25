using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Ministry;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class MinistryService(ChandorApiHttp api) : IMinistryService
{
    private const string C = "Ministry";

    public Task<DataResponse<MinistryDto>?> CreateMinistryAsync(NewMinistryDto ministry, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MinistryDto>($"{C}/create-ministry", JsonContent.Create(ministry), cancellationToken);

    public Task<DataResponse<MinistryDto>?> UpdateMinistryAsync(MinistryDto ministry, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MinistryDto>($"{C}/update-ministry", JsonContent.Create(ministry), cancellationToken);

    public Task<DataResponse<bool>?> DeleteMinistryAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-ministry/{id}", cancellationToken);

    public Task<DataResponse<MinistryDto>?> GetMinistryByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MinistryDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<MinistryDto>>?> GetAllMinistriesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<MinistryDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<MinistryDto>>?> GetPagedMinistriesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<MinistryDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

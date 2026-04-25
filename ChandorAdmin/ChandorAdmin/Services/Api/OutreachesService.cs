using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Outreaches;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class OutreachesService(ChandorApiHttp api) : IOutreachesService
{
    private const string C = "Outreaches";

    public Task<DataResponse<OutreachesDto>?> CreateOutreachesAsync(NewOutreachesDto outreaches, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<OutreachesDto>($"{C}/create-outreaches", JsonContent.Create(outreaches), cancellationToken);

    public Task<DataResponse<OutreachesDto>?> UpdateOutreachesAsync(OutreachesDto outreaches, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<OutreachesDto>($"{C}/update-outreaches", JsonContent.Create(outreaches), cancellationToken);

    public Task<DataResponse<bool>?> DeleteOutreachesAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-outreaches/{id}", cancellationToken);

    public Task<DataResponse<OutreachesDto>?> GetOutreachesByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<OutreachesDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<OutreachesDto>>?> GetAllOutreachesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<OutreachesDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<OutreachesDto>>?> GetPagedOutreachesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<OutreachesDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

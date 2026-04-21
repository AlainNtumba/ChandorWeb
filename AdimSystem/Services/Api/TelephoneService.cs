using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.Telephone;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class TelephoneService(ChandorApiHttp api) : ITelephoneService
{
    private const string C = "Telephone";

    public Task<DataResponse<TelephoneDto>?> CreateTelephoneAsync(NewTelephoneDto telephone, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<TelephoneDto>($"{C}/create-telephone", JsonContent.Create(telephone), cancellationToken);

    public Task<DataResponse<TelephoneDto>?> UpdateTelephoneAsync(TelephoneDto telephone, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<TelephoneDto>($"{C}/update-telephone", JsonContent.Create(telephone), cancellationToken);

    public Task<DataResponse<bool>?> DeleteTelephoneAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-telephone/{id}", cancellationToken);

    public Task<DataResponse<TelephoneDto>?> GetTelephoneByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<TelephoneDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<TelephoneDto>>?> GetAllTelephonesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<TelephoneDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<TelephoneDto>>?> GetPagedTelephonesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<TelephoneDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

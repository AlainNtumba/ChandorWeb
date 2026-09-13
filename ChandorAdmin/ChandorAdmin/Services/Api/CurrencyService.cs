using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Currency;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class CurrencyService(ChandorApiHttp api) : ICurrencyService
{
    private const string C = "Currency";

    public Task<DataResponse<CurrencyDto>?> CreateAsync(NewCurrencyDto currency, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<CurrencyDto>($"{C}/create-currency", JsonContent.Create(currency), cancellationToken);

    public Task<DataResponse<CurrencyDto>?> UpdateAsync(CurrencyDto currency, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<CurrencyDto>($"{C}/update-currency", JsonContent.Create(currency), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-currency/{id}", cancellationToken);

    public Task<DataResponse<CurrencyDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<CurrencyDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<CurrencyDto>>?> GetAllAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<CurrencyDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<CurrencyDto>>?> GetPagedDataAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<CurrencyDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

using ChandorProject.Shared.DTOs.Currency;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface ICurrencyService
{
    Task<DataResponse<CurrencyDto>?> CreateAsync(NewCurrencyDto currency, CancellationToken cancellationToken = default);

    Task<DataResponse<CurrencyDto>?> UpdateAsync(CurrencyDto currency, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<CurrencyDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<CurrencyDto>>?> GetAllAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<CurrencyDto>>?> GetPagedDataAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

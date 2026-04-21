using ChandorProject.Shared.DTOs.Telephone;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface ITelephoneService
{
    Task<DataResponse<TelephoneDto>?> CreateTelephoneAsync(NewTelephoneDto telephone, CancellationToken cancellationToken = default);

    Task<DataResponse<TelephoneDto>?> UpdateTelephoneAsync(TelephoneDto telephone, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteTelephoneAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<TelephoneDto>?> GetTelephoneByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<TelephoneDto>>?> GetAllTelephonesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<TelephoneDto>>?> GetPagedTelephonesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

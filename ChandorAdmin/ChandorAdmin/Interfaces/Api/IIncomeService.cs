using ChandorProject.Shared.DTOs.Income;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IIncomeService
{
    Task<DataResponse<IncomeDto>?> CreateIncomeAsync(NewIncomeDto income, CancellationToken cancellationToken = default);

    Task<DataResponse<IncomeDto>?> UpdateIncomeAsync(IncomeDto income, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteIncomeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IncomeDto>?> GetIncomeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<IncomeDto>>?> GetAllIncomesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<IncomeDto>>?> GetPagedIncomesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

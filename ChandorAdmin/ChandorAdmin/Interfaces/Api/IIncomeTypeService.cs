using ChandorProject.Shared.DTOs.IncomeType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IIncomeTypeService
{
    Task<DataResponse<IncomeTypeDto>?> CreateIncomeTypeAsync(NewIncomeTypeDto incomeType, CancellationToken cancellationToken = default);

    Task<DataResponse<IncomeTypeDto>?> UpdateIncomeTypeAsync(IncomeTypeDto incomeType, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteIncomeTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IncomeTypeDto>?> GetIncomeTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<IncomeTypeDto>>?> GetAllIncomeTypesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<IncomeTypeDto>>?> GetPagedIncomeTypesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

using ChandorProject.Shared.DTOs.ExpensesType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IExpensesTypeService
{
    Task<DataResponse<ExpensesTypeDto>?> CreateExpensesTypeAsync(NewExpensesTypeDto expensesType, CancellationToken cancellationToken = default);

    Task<DataResponse<ExpensesTypeDto>?> UpdateExpensesTypeAsync(ExpensesTypeDto expensesType, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteExpensesTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<ExpensesTypeDto>?> GetExpensesTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ExpensesTypeDto>>?> GetAllExpensesTypesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<ExpensesTypeDto>>?> GetPagedExpensesTypesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

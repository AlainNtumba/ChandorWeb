using ChandorProject.Shared.DTOs.Expenses;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IExpensesService
{
    Task<DataResponse<ExpensesDto>?> CreateExpensesAsync(NewExpensesDto expenses, CancellationToken cancellationToken = default);

    Task<DataResponse<ExpensesDto>?> UpdateExpensesAsync(ExpensesDto expenses, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteExpensesAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<ExpensesDto>?> GetExpensesByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ExpensesDto>>?> GetAllExpensesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<ExpensesDto>>?> GetPagedExpensesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

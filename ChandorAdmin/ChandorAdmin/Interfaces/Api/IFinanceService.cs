using ChandorProject.Shared.DTOs.Finance;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IFinanceService
{
    Task<DataResponse<IEnumerable<TransactionViewDto>>?> InsertTransactionAsync(
        NewTransactionViewDto request, 
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<TransactionViewDto>>?> InsertChurchTransactionAsync(
        NewChurchTransactionDto request, 
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<TransactionViewDto>>?> GetDepartmentTransactionsAsync(
        Guid? departmentId, 
        DateTime? start, 
        DateTime? end, 
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<TransactionViewDto>>?> GetChurchTransactionsAsync(
        DateTime? start, 
        DateTime? end, 
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<FinanceActivityItemDto>>?> GetFinanceActivitiesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<FinanceSummaryDto>>?> GetFinanceSummariesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<CashflowSeriesPointDto>>?> GetCashflowSeriesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<IncomeCategoryDto>>?> GetIncomeByCategoriesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ExpenseCategoryDto>>?> GetExpensesByCategoriesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<TransactionTypeViewDto>>?> GetTransactionTypesAsync(
        CancellationToken cancellationToken = default);
}

using ChandorProject.Shared.DTOs.Finance;
using ChandorProject.Shared.DTOs.Transaction;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface ITransactionService
{
    Task<DataResponse<TransactionDto>?> CreateAsync(NewTransactionDto transaction, CancellationToken cancellationToken = default);

    Task<DataResponse<TransactionDto>?> UpdateAsync(TransactionDto transaction, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<TransactionDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<TransactionDto>>?> GetPagedDataAsync(int page, int pageSize, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<CurrencyBalanceDto>>?> GetCurrencyBalanceAsync(Guid? departmentId = null, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<TransactionView>>?> GetTransactionsAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<TransactionView>>?> GetTeamTransactionsAsync(
        DateTime start,
        DateTime end,
        Guid departmentTeamId,
        Guid? currencyId = null,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<IncomeCategoryDto>>?> GetIncomeByCategoriesAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<IncomeCategoryDto>>?> GetExpensesByCategoriesAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<FinanceActivityItemDto>>?> GetFinanceActivitiesAsync(
        DateTime start,
        DateTime end,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<FinanceSummaryDto>>?> GetFinanceSummariesAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<CashflowSeriesPointDto>>?> GetCashflowSeriesAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default);
}

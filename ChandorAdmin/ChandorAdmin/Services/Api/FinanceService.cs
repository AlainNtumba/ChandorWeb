using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Finance;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class FinanceService(ChandorApiHttp api) : IFinanceService
{
    private const string C = "Finance";

    // =========================
    // TRANSACTIONS
    // =========================

    public Task<DataResponse<IEnumerable<TransactionDto>>?> InsertTransactionAsync( 
        NewTransactionDto request, 
        CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<IEnumerable<TransactionDto>>($"{C}/insert-transaction", JsonContent.Create(request), cancellationToken);

    public Task<DataResponse<IEnumerable<TransactionDto>>?> InsertChurchTransactionAsync(
        NewChurchTransactionDto request, 
        CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<IEnumerable<TransactionDto>>($"{C}/insert-church-transaction", JsonContent.Create(request), cancellationToken);

    // =========================
    // GET TRANSACTIONS
    // =========================

    public Task<DataResponse<IEnumerable<TransactionDto>>?> GetDepartmentTransactionsAsync(
        Guid? departmentId, 
        DateTime? start, 
        DateTime? end,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{C}/get-department-transactions" +
            $"?departmentId={departmentId}&start={start}&end={end}";

        return api.GetDataResponseAsync<IEnumerable<TransactionDto>>(url, cancellationToken);
    }

    public Task<DataResponse<IEnumerable<TransactionDto>>?> GetChurchTransactionsAsync(
        DateTime? start, 
        DateTime? end, 
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{C}/get-church-transactions" +
            $"?start={start}&end={end}";

        return api.GetDataResponseAsync<IEnumerable<TransactionDto>>(url, cancellationToken);
    }

    public Task<DataResponse<IEnumerable<FinanceActivityItemDto>>?> GetFinanceActivitiesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{C}/get-finance-activities" +
            $"?start={start}&end={end}&departmentId={departmentId}";

        return api.GetDataResponseAsync<IEnumerable<FinanceActivityItemDto>>(url, cancellationToken);
    }

    // =========================
    // FINANCE REPORTING
    // =========================

    public Task<DataResponse<IEnumerable<FinanceSummaryDto>>?> GetFinanceSummariesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{C}/get-finance-summaries" +
            $"?start={start}&end={end}&departmentId={departmentId}";

        return api.GetDataResponseAsync<IEnumerable<FinanceSummaryDto>>(url, cancellationToken);
    }

    public Task<DataResponse<IEnumerable<CashflowSeriesPointDto>>?> GetCashflowSeriesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{C}/get-cashflow-series" +
            $"?start={start}&end={end}&departmentId={departmentId}";

        return api.GetDataResponseAsync<IEnumerable<CashflowSeriesPointDto>>(url, cancellationToken);
    }

    // =========================
    // CATEGORIES ANALYTICS
    // =========================

    public Task<DataResponse<IEnumerable<IncomeCategoryDto>>?> GetIncomeByCategoriesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{C}/get-income-by-categories" +
            $"?start={start}&end={end}&departmentId={departmentId}";

        return api.GetDataResponseAsync<IEnumerable<IncomeCategoryDto>>(url, cancellationToken);
    }

    public Task<DataResponse<IEnumerable<ExpenseCategoryDto>>?> GetExpensesByCategoriesAsync(
        DateTime? start,
        DateTime? end,
        Guid? departmentId,
        CancellationToken cancellationToken = default)
    {
        var url =
            $"{C}/get-expenses-by-categories" +
            $"?start={start}&end={end}&departmentId={departmentId}";

        return api.GetDataResponseAsync<IEnumerable<ExpenseCategoryDto>>(url, cancellationToken);
    }

    // =========================
    // LOOKUPS
    // =========================

    public Task<DataResponse<IEnumerable<TransactionTypeDto>>?> GetTransactionTypesAsync(
        CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<TransactionTypeDto>>($"{C}/get-transactions-types", cancellationToken);


}

using System.Net.Http.Json;
using ChandorAdmin.Helpers;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Finance;
using ChandorProject.Shared.DTOs.Transaction;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class TransactionService(ChandorApiHttp api) : ITransactionService
{
    private const string C = "Transaction";

    public Task<DataResponse<TransactionDto>?> CreateAsync(NewTransactionDto transaction, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<TransactionDto>($"{C}/create-transaction", JsonContent.Create(transaction), cancellationToken);

    public Task<DataResponse<TransactionDto>?> UpdateAsync(TransactionDto transaction, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<TransactionDto>($"{C}/update-transaction", JsonContent.Create(transaction), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-transaction/{id}", cancellationToken);

    public Task<DataResponse<TransactionDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<TransactionDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IReadOnlyList<TransactionDto>>?> GetPagedDataAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<TransactionDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);

    public Task<DataResponse<IEnumerable<CurrencyBalanceDto>>?> GetCurrencyBalanceAsync(
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var path = $"{C}/get-currency-balance";
        if (departmentId is { } id)
            path += $"?departmentId={id}";

        return api.GetDataResponseAsync<IEnumerable<CurrencyBalanceDto>>(path, cancellationToken);
    }

    public Task<DataResponse<IEnumerable<TransactionView>>?> GetTransactionsAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<TransactionView>>(
            $"{C}/get-transactions?{BuildQuery(start, end, currencyId: currencyId, departmentId: departmentId)}",
            cancellationToken);

    public Task<DataResponse<IEnumerable<TransactionView>>?> GetTeamTransactionsAsync(
        DateTime start,
        DateTime end,
        Guid departmentTeamId,
        Guid? currencyId = null,
        CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<TransactionView>>(
            $"{C}/get-team-transactions?{BuildQuery(start, end, departmentTeamId: departmentTeamId, currencyId: currencyId)}",
            cancellationToken);

    public Task<DataResponse<IEnumerable<IncomeCategoryDto>>?> GetIncomeByCategoriesAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<IncomeCategoryDto>>(
            $"{C}/get-income-by-categories?{BuildQuery(start, end, currencyId: currencyId, departmentId: departmentId)}",
            cancellationToken);

    public Task<DataResponse<IEnumerable<IncomeCategoryDto>>?> GetExpensesByCategoriesAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<IncomeCategoryDto>>(
            $"{C}/get-expenses-by-categories?{BuildQuery(start, end, currencyId: currencyId, departmentId: departmentId)}",
            cancellationToken);

    public Task<DataResponse<IEnumerable<FinanceActivityItemDto>>?> GetFinanceActivitiesAsync(
        DateTime start,
        DateTime end,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<FinanceActivityItemDto>>(
            $"{C}/get-finance-activities?{BuildQuery(start, end, departmentId: departmentId)}",
            cancellationToken);

    public Task<DataResponse<IEnumerable<FinanceSummaryDto>>?> GetFinanceSummariesAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<FinanceSummaryDto>>(
            $"{C}/get-finance-summaries?{BuildQuery(start, end, currencyId: currencyId, departmentId: departmentId)}",
            cancellationToken);

    public Task<DataResponse<IEnumerable<CashflowSeriesPointDto>>?> GetCashflowSeriesAsync(
        DateTime start,
        DateTime end,
        Guid? currencyId = null,
        Guid? departmentId = null,
        CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<CashflowSeriesPointDto>>(
            $"{C}/get-cashflow-series?{BuildQuery(start, end, currencyId: currencyId, departmentId: departmentId)}",
            cancellationToken);

    private static string BuildQuery(
        DateTime start,
        DateTime end,
        Guid? departmentTeamId = null,
        Guid? currencyId = null,
        Guid? departmentId = null)
    {
        var parts = new List<string>
        {
            $"start={ApiDateQueryFormatter.FormatQueryValue(start)}",
            $"end={ApiDateQueryFormatter.FormatQueryValue(end)}"
        };

        if (departmentTeamId is { } teamId)
            parts.Add($"departmentTeamId={teamId}");
        if (currencyId is { } cid)
            parts.Add($"currencyId={cid}");
        if (departmentId is { } did)
            parts.Add($"departmentId={did}");

        return string.Join("&", parts);
    }
}

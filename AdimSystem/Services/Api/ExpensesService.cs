using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.Expenses;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class ExpensesService(ChandorApiHttp api) : IExpensesService
{
    private const string C = "Expenses";

    public Task<DataResponse<ExpensesDto>?> CreateExpensesAsync(NewExpensesDto expenses, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<ExpensesDto>($"{C}/create-expenses", JsonContent.Create(expenses), cancellationToken);

    public Task<DataResponse<ExpensesDto>?> UpdateExpensesAsync(ExpensesDto expenses, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<ExpensesDto>($"{C}/update-expenses", JsonContent.Create(expenses), cancellationToken);

    public Task<DataResponse<bool>?> DeleteExpensesAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-expenses/{id}", cancellationToken);

    public Task<DataResponse<ExpensesDto>?> GetExpensesByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<ExpensesDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<ExpensesDto>>?> GetAllExpensesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ExpensesDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<ExpensesDto>>?> GetPagedExpensesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<ExpensesDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.ExpensesType;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class ExpensesTypeService(ChandorApiHttp api) : IExpensesTypeService
{
    private const string C = "ExpensesType";

    public Task<DataResponse<ExpensesTypeDto>?> CreateExpensesTypeAsync(NewExpensesTypeDto expensesType, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<ExpensesTypeDto>($"{C}/create-expenses-type", JsonContent.Create(expensesType), cancellationToken);

    public Task<DataResponse<ExpensesTypeDto>?> UpdateExpensesTypeAsync(ExpensesTypeDto expensesType, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<ExpensesTypeDto>($"{C}/update-expenses-type", JsonContent.Create(expensesType), cancellationToken);

    public Task<DataResponse<bool>?> DeleteExpensesTypeAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-expenses-type/{id}", cancellationToken);

    public Task<DataResponse<ExpensesTypeDto>?> GetExpensesTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<ExpensesTypeDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<ExpensesTypeDto>>?> GetAllExpensesTypesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ExpensesTypeDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<ExpensesTypeDto>>?> GetPagedExpensesTypesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<ExpensesTypeDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

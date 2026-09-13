using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.TransactionType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class TransactionTypeService(ChandorApiHttp api) : ITransactionTypeService
{
    private const string C = "TransactionType";

    public Task<DataResponse<TransactionTypeDto>?> CreateAsync(NewTransactionTypeDto transactionType, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<TransactionTypeDto>($"{C}/create-transaction-type", JsonContent.Create(transactionType), cancellationToken);

    public Task<DataResponse<TransactionTypeDto>?> UpdateAsync(TransactionTypeDto transactionType, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<TransactionTypeDto>($"{C}/update-transaction-type", JsonContent.Create(transactionType), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-transaction-type/{id}", cancellationToken);

    public Task<DataResponse<TransactionTypeDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<TransactionTypeDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<TransactionTypeDto>>?> GetAllAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<TransactionTypeDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<TransactionTypeDto>>?> GetPagedDataAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<TransactionTypeDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

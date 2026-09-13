using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.TransactionCategory;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class TransactionCategoryService(ChandorApiHttp api) : ITransactionCategoryService
{
    private const string C = "TransactionCategory";

    public Task<DataResponse<TransactionCategoryDto>?> CreateAsync(NewTransactionCategoryDto transactionCategory, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<TransactionCategoryDto>($"{C}/create-transaction-category", JsonContent.Create(transactionCategory), cancellationToken);

    public Task<DataResponse<TransactionCategoryDto>?> UpdateAsync(TransactionCategoryDto transactionCategory, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<TransactionCategoryDto>($"{C}/update-transaction-category", JsonContent.Create(transactionCategory), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-transaction-category/{id}", cancellationToken);

    public Task<DataResponse<TransactionCategoryDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<TransactionCategoryDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<TransactionCategoryDto>>?> GetAllAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<TransactionCategoryDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<TransactionCategoryDto>>?> GetPagedDataAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<TransactionCategoryDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

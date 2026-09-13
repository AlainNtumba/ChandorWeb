using ChandorProject.Shared.DTOs.TransactionCategory;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface ITransactionCategoryService
{
    Task<DataResponse<TransactionCategoryDto>?> CreateAsync(NewTransactionCategoryDto transactionCategory, CancellationToken cancellationToken = default);

    Task<DataResponse<TransactionCategoryDto>?> UpdateAsync(TransactionCategoryDto transactionCategory, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<TransactionCategoryDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<TransactionCategoryDto>>?> GetAllAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<TransactionCategoryDto>>?> GetPagedDataAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

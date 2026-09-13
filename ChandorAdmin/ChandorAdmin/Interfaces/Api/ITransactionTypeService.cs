using ChandorProject.Shared.DTOs.TransactionType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface ITransactionTypeService
{
    Task<DataResponse<TransactionTypeDto>?> CreateAsync(NewTransactionTypeDto transactionType, CancellationToken cancellationToken = default);

    Task<DataResponse<TransactionTypeDto>?> UpdateAsync(TransactionTypeDto transactionType, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<TransactionTypeDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<TransactionTypeDto>>?> GetAllAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<TransactionTypeDto>>?> GetPagedDataAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

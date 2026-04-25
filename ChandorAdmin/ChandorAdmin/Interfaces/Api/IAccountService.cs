using ChandorProject.Shared.DTOs.Account;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IAccountService
{
    Task<DataResponse<AccountDto>?> CreateAccountAsync(NewAccountDto account, CancellationToken cancellationToken = default);

    Task<DataResponse<AccountDto>?> UpdateAccountAsync(AccountDto account, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<AccountDto>?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<AccountDto>>?> GetAllAccountsAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<AccountDto>>?> GetPagedAccountsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

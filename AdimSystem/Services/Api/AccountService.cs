using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.Account;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class AccountService(ChandorApiHttp api) : IAccountService
{
    private const string C = "Account";

    public Task<DataResponse<AccountDto>?> CreateAccountAsync(NewAccountDto account, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<AccountDto>($"{C}/create-account", JsonContent.Create(account), cancellationToken);

    public Task<DataResponse<AccountDto>?> UpdateAccountAsync(AccountDto account, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<AccountDto>($"{C}/update-account", JsonContent.Create(account), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAccountAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-account/{id}", cancellationToken);

    public Task<DataResponse<AccountDto>?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<AccountDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<AccountDto>>?> GetAllAccountsAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<AccountDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<AccountDto>>?> GetPagedAccountsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<AccountDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

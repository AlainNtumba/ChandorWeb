using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Income;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class IncomeService(ChandorApiHttp api) : IIncomeService
{
    private const string C = "Income";

    public Task<DataResponse<IncomeDto>?> CreateIncomeAsync(NewIncomeDto income, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<IncomeDto>($"{C}/create-income", JsonContent.Create(income), cancellationToken);

    public Task<DataResponse<IncomeDto>?> UpdateIncomeAsync(IncomeDto income, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<IncomeDto>($"{C}/update-income", JsonContent.Create(income), cancellationToken);

    public Task<DataResponse<bool>?> DeleteIncomeAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-income/{id}", cancellationToken);

    public Task<DataResponse<IncomeDto>?> GetIncomeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IncomeDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<IncomeDto>>?> GetAllIncomesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<IncomeDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<IncomeDto>>?> GetPagedIncomesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<IncomeDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.IncomeType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class IncomeTypeService(ChandorApiHttp api) : IIncomeTypeService
{
    private const string C = "IncomeType";

    public Task<DataResponse<IncomeTypeDto>?> CreateIncomeTypeAsync(NewIncomeTypeDto incomeType, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<IncomeTypeDto>($"{C}/create-income-type", JsonContent.Create(incomeType), cancellationToken);

    public Task<DataResponse<IncomeTypeDto>?> UpdateIncomeTypeAsync(IncomeTypeDto incomeType, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<IncomeTypeDto>($"{C}/update-income-type", JsonContent.Create(incomeType), cancellationToken);

    public Task<DataResponse<bool>?> DeleteIncomeTypeAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-income-type/{id}", cancellationToken);

    public Task<DataResponse<IncomeTypeDto>?> GetIncomeTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IncomeTypeDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<IncomeTypeDto>>?> GetAllIncomeTypesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<IncomeTypeDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<IncomeTypeDto>>?> GetPagedIncomeTypesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<IncomeTypeDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

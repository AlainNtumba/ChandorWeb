using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.ProgramType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class ProgramTypeService(ChandorApiHttp api) : IProgramTypeService
{
    private const string C = "ProgramType";

    public Task<DataResponse<ProgramTypeDto>?> AddProgramTypeAsync(NewProgramTypeDto programType, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<ProgramTypeDto>($"{C}/add-program-type", JsonContent.Create(programType), cancellationToken);

    public Task<DataResponse<ProgramTypeDto>?> UpdateProgramTypeAsync(ProgramTypeDto programType, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<ProgramTypeDto>($"{C}/update-program-type", JsonContent.Create(programType), cancellationToken);

    public Task<DataResponse<bool>?> DeleteProgramTypeAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-program-type/{id}", cancellationToken);

    public Task<DataResponse<ProgramTypeDto>?> GetProgramTypeByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<ProgramTypeDto>($"{C}/get-program-type-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<ProgramTypeDto>>?> GetAllProgramTypesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ProgramTypeDto>>($"{C}/get-all-program-types", cancellationToken);
}

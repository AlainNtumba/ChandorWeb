using ChandorProject.Shared.DTOs.ProgramType;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IProgramTypeService
{
    Task<DataResponse<ProgramTypeDto>?> AddProgramTypeAsync(NewProgramTypeDto programType, CancellationToken cancellationToken = default);

    Task<DataResponse<ProgramTypeDto>?> UpdateProgramTypeAsync(ProgramTypeDto programType, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteProgramTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<ProgramTypeDto>?> GetProgramTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ProgramTypeDto>>?> GetAllProgramTypesAsync(CancellationToken cancellationToken = default);
}

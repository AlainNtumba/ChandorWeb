using ChandorProject.Shared.DTOs.Bible;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IBibleVerseApiService
{
    Task<DataResponse<BibleVersePageDto>?> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<DataResponse<BibleVerseDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResponse<BibleVerseDto>?> GetVerseOfTheDayAsync(CancellationToken cancellationToken = default);
    Task<DataResponse<BibleVerseDto>?> CreateAsync(CreateBibleVerseDto dto, CancellationToken cancellationToken = default);
    Task<DataResponse<BibleVerseDto>?> UpdateAsync(Guid id, UpdateBibleVerseDto dto, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

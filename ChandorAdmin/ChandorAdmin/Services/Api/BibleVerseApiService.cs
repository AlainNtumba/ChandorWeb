using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Bible;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class BibleVerseApiService(ChandorApiHttp api) : IBibleVerseApiService
{
    private const string Root = "bible";

    public Task<DataResponse<BibleVersePageDto>?> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<BibleVersePageDto>($"{Root}?page={Math.Max(1, page)}&pageSize={Math.Clamp(pageSize, 1, 100)}", cancellationToken);

    public Task<DataResponse<BibleVerseDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<BibleVerseDto>($"{Root}/{id:D}", cancellationToken);

    public Task<DataResponse<BibleVerseDto>?> GetVerseOfTheDayAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<BibleVerseDto>($"{Root}/verse-of-the-day", cancellationToken);

    public Task<DataResponse<BibleVerseDto>?> CreateAsync(CreateBibleVerseDto dto, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<BibleVerseDto>(Root, JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<BibleVerseDto>?> UpdateAsync(Guid id, UpdateBibleVerseDto dto, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<BibleVerseDto>($"{Root}/{id:D}", JsonContent.Create(dto), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{Root}/{id:D}", cancellationToken);
}

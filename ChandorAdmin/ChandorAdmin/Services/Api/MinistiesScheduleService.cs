using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.MinistiesSchedule;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class MinistiesScheduleService(ChandorApiHttp api) : IMinistiesScheduleService
{
    private const string C = "MinistiesSchedule";

    public Task<DataResponse<MinistiesScheduleDto>?> CreateMinistiesScheduleAsync(NewMinistiesScheduleDto ministiesSchedule, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<MinistiesScheduleDto>($"{C}/create-ministies-schedule", JsonContent.Create(ministiesSchedule), cancellationToken);

    public Task<DataResponse<MinistiesScheduleDto>?> UpdateMinistiesScheduleAsync(MinistiesScheduleDto ministiesSchedule, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<MinistiesScheduleDto>($"{C}/update-ministies-schedule", JsonContent.Create(ministiesSchedule), cancellationToken);

    public Task<DataResponse<bool>?> DeleteMinistiesScheduleAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-ministies-schedule/{id}", cancellationToken);

    public Task<DataResponse<MinistiesScheduleDto>?> GetMinistiesScheduleByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<MinistiesScheduleDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<MinistiesScheduleDto>>?> GetAllMinistiesSchedulesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<MinistiesScheduleDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<MinistiesScheduleDto>>?> GetPagedMinistiesSchedulesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<MinistiesScheduleDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

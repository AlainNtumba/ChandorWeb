using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Attendance;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class AttendanceService(ChandorApiHttp api) : IAttendanceService
{
    private const string C = "Attendance";

    public Task<DataResponse<AttendanceDto>?> CreateAttendanceAsync(NewAttendanceDto attendance, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<AttendanceDto>($"{C}/create-attendance", JsonContent.Create(attendance), cancellationToken);

    public Task<DataResponse<AttendanceDto>?> UpdateAttendanceAsync(AttendanceDto attendance, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<AttendanceDto>($"{C}/update-attendance", JsonContent.Create(attendance), cancellationToken);

    public Task<DataResponse<bool>?> DeleteAttendanceAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-attendance/{id}", cancellationToken);

    public Task<DataResponse<AttendanceDto>?> GetAttendanceByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<AttendanceDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<AttendanceDto>>?> GetAllAttendancesAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<AttendanceDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<AttendanceDto>>?> GetPagedAttendancesAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<AttendanceDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

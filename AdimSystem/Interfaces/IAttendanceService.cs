using ChandorProject.Shared.DTOs.Attendance;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IAttendanceService
{
    Task<DataResponse<AttendanceDto>?> CreateAttendanceAsync(NewAttendanceDto attendance, CancellationToken cancellationToken = default);

    Task<DataResponse<AttendanceDto>?> UpdateAttendanceAsync(AttendanceDto attendance, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteAttendanceAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<AttendanceDto>?> GetAttendanceByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<AttendanceDto>>?> GetAllAttendancesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<AttendanceDto>>?> GetPagedAttendancesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

using ChandorAdmin.Models.MemberRequest;
using ChandorProject.Shared.DTOs.MemberRequest;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IMemberRequestService
{
    Task<DataResponse<PagedResult<MemberRequestDto>>?> GetPagedAsync(MemberRequestFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<MemberRequestSummaryDto>?> GetSummaryAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<DataResponse<MemberRequestDto>?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<DataResponse<MemberRequestDto>?> UpdateAsync(Guid id, UpdateMemberRequestDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<MemberRequestDto>?> UpdateStatusAsync(Guid id, string status, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

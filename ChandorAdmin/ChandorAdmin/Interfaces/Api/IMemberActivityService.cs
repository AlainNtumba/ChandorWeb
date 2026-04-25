using ChandorProject.Shared.DTOs.MemberActivity;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IMemberActivityService
{
    Task<DataResponse<MemberActivityDto>?> CreateMemberActivityAsync(NewMemberActivityDto memberActivity, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberActivityDto>?> UpdateMemberActivityAsync(MemberActivityDto memberActivity, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteMemberActivityAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberActivityDto>?> GetMemberActivityByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<MemberActivityDto>>?> GetAllMemberActivitiesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<MemberActivityDto>>?> GetPagedMemberActivitiesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

using ChandorProject.Shared.DTOs.MemberType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IMemberTypeService
{
    Task<DataResponse<MemberTypeDto>?> CreateMemberTypeAsync(NewMemberTypeDto memberType, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberTypeDto>?> UpdateMemberTypeAsync(MemberTypeDto memberType, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteMemberTypeAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<MemberTypeDto>?> GetMemberTypeByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<MemberTypeDto>>?> GetAllMemberTypesAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<MemberTypeDto>>?> GetPagedMemberTypesAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

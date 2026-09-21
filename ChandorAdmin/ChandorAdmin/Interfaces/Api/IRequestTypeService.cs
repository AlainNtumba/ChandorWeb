using ChandorAdmin.Models.MemberRequest;
using ChandorProject.Shared.DTOs.RequestType;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IRequestTypeService
{
    Task<DataResponse<PagedResult<RequestTypeDto>>?> GetActiveAsync(int pageSize = 100, CancellationToken cancellationToken = default);
    Task<DataResponse<PagedResult<RequestTypeDto>>?> GetAdminAsync(RequestTypeFilterState filter, CancellationToken cancellationToken = default);
    Task<DataResponse<RequestTypeDto>?> CreateAsync(CreateRequestTypeDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<RequestTypeDto>?> UpdateAsync(Guid id, UpdateRequestTypeDto input, CancellationToken cancellationToken = default);
    Task<DataResponse<bool>?> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}

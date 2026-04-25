using ChandorProject.Shared.DTOs.User;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IUserService
{
    Task<DataResponse<ViewUserDto>?> AddUserAsync(Stream? fileContent, string? fileName, string username, string password, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ViewUserDto>>?> GetAllUsersAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<ViewUserDto>?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default);
}

using ChandorProject.Shared.DTOs.User;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IUserService
{
    Task<DataResponse<ViewUserDto>?> AddUserAsync(Stream? fileContent, string? fileName, string username, string password, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<ViewUserDto>>?> GetAllUsersAsync(CancellationToken cancellationToken = default);
}

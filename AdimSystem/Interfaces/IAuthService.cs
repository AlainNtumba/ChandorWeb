using ChandorProject.Shared.DTOs.User;
using ChandorProject.Shared.Models;

namespace AdimSystem.Interfaces;

public interface IAuthService
{
    Task<DataResponse<string>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    void Logout();

    Task EnsureValidTokenAsync(CancellationToken cancellationToken = default);
}

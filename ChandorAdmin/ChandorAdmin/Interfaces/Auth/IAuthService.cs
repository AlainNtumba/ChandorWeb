using ChandorProject.Shared.DTOs.User;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Auth;

public interface IAuthService
{
    Task<DataResponse<string>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    void Logout();

    Task EnsureValidTokenAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<string>?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);
}

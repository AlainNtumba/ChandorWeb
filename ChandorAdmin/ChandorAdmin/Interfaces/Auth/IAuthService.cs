using ChandorProject.Shared.DTOs.User;
using ChandorProject.Shared.Models;
using ChandorAdmin.Models.Auth;

namespace ChandorAdmin.Interfaces.Auth;

public interface IAuthService
{
    /// <summary>POST <c>Auth/login</c>. Expects <see cref="AuthTokenResponseDto"/> in <c>Data</c>, or legacy <c>Data</c> as a JWT string (access only).</summary>
    Task<DataResponse<AuthTokenResponseDto?>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);

    /// <summary>Clears tokens, invalidates the auth session, and redirects to <c>/login</c> unless suppressed.</summary>
    Task LogoutAsync(CancellationToken cancellationToken = default, bool redirectToLogin = true);

    /// <summary>Proactively refresh when the access token is about to expire (uses refresh token, never re-login with password).</summary>
    Task EnsureValidTokenAsync(CancellationToken cancellationToken = default);

    /// <summary>POST <c>Auth/refresh-token</c> using the in-memory refresh token. Returns <see langword="true"/> on success.</summary>
    Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken = default);
}

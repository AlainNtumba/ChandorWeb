using ChandorAdmin.Interfaces.Auth;

using ChandorProject.Shared.DTOs.User;

namespace ChandorAdmin.Services.Auth;

public sealed class AuthState : IAuthState
{
    private readonly object _gate = new();

    public string? AccessToken { get; private set; }
    public DateTimeOffset? AccessTokenExpiresAtUtc { get; private set; }
    public LoginRequestDto? RefreshCredentials { get; private set; }

    public void SetSession(string accessToken, DateTimeOffset? expiresAtUtc, LoginRequestDto? refreshCredentials)
    {
        lock (_gate)
        {
            AccessToken = accessToken;
            AccessTokenExpiresAtUtc = expiresAtUtc;
            RefreshCredentials = refreshCredentials;
        }
    }

    public void Clear()
    {
        lock (_gate)
        {
            AccessToken = null;
            AccessTokenExpiresAtUtc = null;
            RefreshCredentials = null;
        }
    }
}

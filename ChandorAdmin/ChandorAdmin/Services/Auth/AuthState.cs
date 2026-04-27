using ChandorAdmin.Interfaces.Auth;

namespace ChandorAdmin.Services.Auth;

public sealed class AuthState : IAuthState
{
    private readonly object _gate = new();

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? AccessTokenExpiresAtUtc { get; private set; }

    public event EventHandler? AuthenticationStateChanged;

    public void SetSession(string accessToken, string? refreshToken, DateTimeOffset? accessExpiresAtUtc)
    {
        lock (_gate)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            AccessTokenExpiresAtUtc = accessExpiresAtUtc;
        }

        AuthenticationStateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        lock (_gate)
        {
            AccessToken = null;
            RefreshToken = null;
            AccessTokenExpiresAtUtc = null;
        }

        AuthenticationStateChanged?.Invoke(this, EventArgs.Empty);
    }
}

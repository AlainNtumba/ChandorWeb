namespace ChandorAdmin.Interfaces.Auth;

/// <summary>
/// In-memory session: access and refresh tokens are never written to <c>localStorage</c> or other persistent browser storage.
/// </summary>
public interface IAuthState
{
    string? AccessToken { get; }
    string? RefreshToken { get; }
    DateTimeOffset? AccessTokenExpiresAtUtc { get; }

    event EventHandler? AuthenticationStateChanged;

    void SetSession(string accessToken, string? refreshToken, DateTimeOffset? accessExpiresAtUtc);
    void Clear();
}

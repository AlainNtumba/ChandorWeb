namespace ChandorAdmin.Interfaces.Auth;

/// <summary>
/// Session tokens held in memory and mirrored to browser <c>localStorage</c> so full page reloads keep the user signed in until logout or idle timeout.
/// </summary>
public interface IAuthState
{
    string? AccessToken { get; }
    string? RefreshToken { get; }
    DateTimeOffset? AccessTokenExpiresAtUtc { get; }

    event EventHandler? AuthenticationStateChanged;

    /// <summary>Loads tokens from persistent browser storage when memory is empty (e.g. after F5). Idempotent per app load.</summary>
    void RestorePersistedSessionIfNeeded();

    void SetSession(string accessToken, string? refreshToken, DateTimeOffset? accessExpiresAtUtc);
    void Clear();
}

using ChandorProject.Shared.DTOs.User;

namespace AdimSystem.Services.Auth;

/// <summary>
/// Holds the current JWT and optional credentials for the same Blazor circuit (server session analogue).
/// </summary>
public interface IAuthState
{
    string? AccessToken { get; }
    DateTimeOffset? AccessTokenExpiresAtUtc { get; }

    /// <summary>
    /// When set, used to obtain a new JWT before expiry while the user remains active.
    /// </summary>
    LoginRequestDto? RefreshCredentials { get; }

    void SetSession(string accessToken, DateTimeOffset? expiresAtUtc, LoginRequestDto? refreshCredentials);
    void Clear();
}

namespace ChandorAdmin.Models.Auth;

/// <summary>Typical shape for <c>Auth/login</c> and <c>Auth/refresh-token</c> JSON bodies when <c>Data</c> is an object.</summary>
public sealed class AuthTokenResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
}

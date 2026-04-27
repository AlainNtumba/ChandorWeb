namespace ChandorAdmin.Configuration;

/// <summary>Client-side session options (Blazor WebAssembly). Refresh tokens are kept in memory only.</summary>
public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Minutes of inactivity before automatic logout. Default 5.</summary>
    public int InactivityTimeoutMinutes { get; set; } = 5;

    /// <summary>Proactively refresh the access token when it expires within this many minutes.</summary>
    public int AccessTokenRefreshSkewMinutes { get; set; } = 2;
}

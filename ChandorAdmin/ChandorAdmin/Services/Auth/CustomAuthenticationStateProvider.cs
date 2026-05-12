using ChandorAdmin.Interfaces.Auth;
using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

namespace ChandorAdmin.Services.Auth;

public sealed class CustomAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly IAuthState _authState;
    private readonly ILogger<CustomAuthenticationStateProvider> _logger;

    public CustomAuthenticationStateProvider(IAuthState authState, ILogger<CustomAuthenticationStateProvider> logger)
    {
        _authState = authState;
        _logger = logger;
        _authState.AuthenticationStateChanged += OnAuthStateChanged;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _authState.RestorePersistedSessionIfNeeded();
        return Task.FromResult(BuildState());
    }

    private AuthenticationState BuildState()
    {
        var token = _authState.AccessToken;
        if (string.IsNullOrEmpty(token))
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var principal = JwtClaimsMapper.CreatePrincipalFromAccessToken(token);
        if (principal is null)
        {
            _logger.LogWarning("Access token could not be mapped to a claims principal.");
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        return new AuthenticationState(principal);
    }

    private void OnAuthStateChanged(object? sender, EventArgs e) =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public void Dispose() => _authState.AuthenticationStateChanged -= OnAuthStateChanged;
}

using System.Globalization;
using ChandorAdmin.Interfaces.Auth;
using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;

namespace ChandorAdmin.Services.Auth;

public sealed class AuthState : IAuthState
{
    private const string KeyAccess = "chandor.admin.auth.accessToken";
    private const string KeyRefresh = "chandor.admin.auth.refreshToken";
    private const string KeyExpires = "chandor.admin.auth.accessExpiresAtUtc";

    private readonly object _gate = new();
    private readonly IJSInProcessRuntime? _jsInProcess;
    private bool _restoreAttempted;

    public AuthState(IJSRuntime jsRuntime)
    {
        _jsInProcess = jsRuntime as IJSInProcessRuntime;
    }

    public string? AccessToken { get; private set; }
    public string? RefreshToken { get; private set; }
    public DateTimeOffset? AccessTokenExpiresAtUtc { get; private set; }

    public event EventHandler? AuthenticationStateChanged;

    public void RestorePersistedSessionIfNeeded()
    {
        if (_restoreAttempted)
            return;
        _restoreAttempted = true;

        if (!string.IsNullOrEmpty(AccessToken))
            return;

        if (_jsInProcess is null)
            return;

        string? access;
        string? refresh;
        string? expiresRaw;
        try
        {
            access = _jsInProcess.Invoke<string?>("localStorage.getItem", KeyAccess);
            refresh = _jsInProcess.Invoke<string?>("localStorage.getItem", KeyRefresh);
            expiresRaw = _jsInProcess.Invoke<string?>("localStorage.getItem", KeyExpires);
        }
        catch
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(access))
            return;

        DateTimeOffset? expires = null;
        if (!string.IsNullOrWhiteSpace(expiresRaw)
            && DateTimeOffset.TryParse(expiresRaw, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var parsed))
            expires = parsed;

        var refreshToken = string.IsNullOrWhiteSpace(refresh) ? null : refresh;

        lock (_gate)
        {
            AccessToken = access;
            RefreshToken = refreshToken;
            AccessTokenExpiresAtUtc = expires;
        }

        if (JwtClaimsMapper.CreatePrincipalFromAccessToken(access) is null)
        {
            Clear();
        }
    }

    public void SetSession(string accessToken, string? refreshToken, DateTimeOffset? accessExpiresAtUtc)
    {
        lock (_gate)
        {
            AccessToken = accessToken;
            RefreshToken = refreshToken;
            AccessTokenExpiresAtUtc = accessExpiresAtUtc;
        }

        PersistSessionToBrowser();
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

        ClearBrowserStorage();
        AuthenticationStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void PersistSessionToBrowser()
    {
        if (_jsInProcess is null)
            return;

        try
        {
            if (string.IsNullOrEmpty(AccessToken))
            {
                ClearBrowserStorageInternal();
                return;
            }

            _jsInProcess.InvokeVoid("localStorage.setItem", KeyAccess, AccessToken);
            if (string.IsNullOrEmpty(RefreshToken))
                _jsInProcess.InvokeVoid("localStorage.removeItem", KeyRefresh);
            else
                _jsInProcess.InvokeVoid("localStorage.setItem", KeyRefresh, RefreshToken);

            if (AccessTokenExpiresAtUtc is { } exp)
                _jsInProcess.InvokeVoid("localStorage.setItem", KeyExpires, exp.ToString("O", CultureInfo.InvariantCulture));
            else
                _jsInProcess.InvokeVoid("localStorage.removeItem", KeyExpires);
        }
        catch
        {
            // Ignore storage failures (private mode, disabled storage, etc.)
        }
    }

    private void ClearBrowserStorage() => ClearBrowserStorageInternal();

    private void ClearBrowserStorageInternal()
    {
        if (_jsInProcess is null)
            return;

        try
        {
            _jsInProcess.InvokeVoid("localStorage.removeItem", KeyAccess);
            _jsInProcess.InvokeVoid("localStorage.removeItem", KeyRefresh);
            _jsInProcess.InvokeVoid("localStorage.removeItem", KeyExpires);
        }
        catch
        {
            // Ignore
        }
    }
}

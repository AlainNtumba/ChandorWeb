using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Text.Json;
using ChandorAdmin.Configuration;
using ChandorAdmin.Interfaces.Auth;
using ChandorAdmin.Models.Auth;
using ChandorProject.Shared.DTOs.User;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;

namespace ChandorAdmin.Services.Auth;

public sealed class AuthService : IAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthState _authState;
    private readonly IOptions<AuthOptions> _options;
    private readonly ILogger<AuthService> _logger;
    private readonly NavigationManager _navigation;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);

    public AuthService(
        IHttpClientFactory httpClientFactory,
        IAuthState authState,
        IOptions<AuthOptions> options,
        ILogger<AuthService> logger,
        NavigationManager navigation)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
        _options = options;
        _logger = logger;
        _navigation = navigation;
    }

    public async Task<DataResponse<AuthTokenResponseDto?>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ChandorApi.Auth");
            using var httpResponse = await client.PostAsJsonAsync("Auth/login", request, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            return await ReadLoginResponseAsync(httpResponse, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login request failed.");
            return new DataResponse<AuthTokenResponseDto?>
            {
                Success = false,
                Message = "Login could not be completed.",
                Error = [ex.Message]
            };
        }
    }

    public Task LogoutAsync(CancellationToken cancellationToken = default, bool redirectToLogin = true)
    {
        _ = cancellationToken;
        _authState.Clear();
        if (redirectToLogin)
            _navigation.NavigateTo("/login");
        return Task.CompletedTask;
    }

    public async Task EnsureValidTokenAsync(CancellationToken cancellationToken = default)
    {
        var access = _authState.AccessToken;
        if (string.IsNullOrEmpty(access))
            return;

        var refresh = _authState.RefreshToken;
        var exp = _authState.AccessTokenExpiresAtUtc;
        var skew = TimeSpan.FromMinutes(Math.Max(0, _options.Value.AccessTokenRefreshSkewMinutes));

        if (string.IsNullOrEmpty(refresh))
        {
            if (exp is { } e && e <= DateTimeOffset.UtcNow.Add(skew))
            {
                _logger.LogWarning("Access token expired and no refresh token is available. Logging out.");
                await LogoutAsync(cancellationToken, redirectToLogin: true).ConfigureAwait(false);
            }

            return;
        }

        if (exp is { } ex && ex > DateTimeOffset.UtcNow.Add(skew))
            return;

        if (!await TryRefreshTokenCoreAsync(cancellationToken).ConfigureAwait(false))
        {
            _logger.LogWarning("Token refresh failed during EnsureValidToken. Logging out.");
            await LogoutAsync(cancellationToken, redirectToLogin: true).ConfigureAwait(false);
        }
    }

    public Task<bool> TryRefreshTokenAsync(CancellationToken cancellationToken = default) =>
        TryRefreshTokenCoreAsync(cancellationToken);

    private async Task<bool> TryRefreshTokenCoreAsync(CancellationToken cancellationToken = default)
    {
        var refresh = _authState.RefreshToken;
        if (string.IsNullOrEmpty(refresh))
            return false;

        await _refreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            refresh = _authState.RefreshToken;
            if (string.IsNullOrEmpty(refresh))
                return false;

            var client = _httpClientFactory.CreateClient("ChandorApi.Auth");
            var body = new RefreshTokenRequest { Token = refresh };
            using var httpResponse = await client
                .PostAsJsonAsync("Auth/refresh-token", body, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            if (!httpResponse.IsSuccessStatusCode)
                return false;

            var result = await ReadTokenPayloadEnvelopeAsync(httpResponse, cancellationToken).ConfigureAwait(false);
            if (!result.Success || result.Data is null)
                return false;

            var dto = result.Data;
            if (string.IsNullOrEmpty(dto.AccessToken))
                return false;

            var newRefresh = string.IsNullOrEmpty(dto.RefreshToken) ? refresh : dto.RefreshToken;
            var newExp = TryGetJwtExpiryUtc(dto.AccessToken);
            _authState.SetSession(dto.AccessToken, newRefresh, newExp);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh request failed.");
            return false;
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    public static void CommitSession(IAuthState state, AuthTokenResponseDto? dto)
    {
        if (dto is null || string.IsNullOrEmpty(dto.AccessToken))
            return;
        state.SetSession(dto.AccessToken, string.IsNullOrEmpty(dto.RefreshToken) ? null : dto.RefreshToken, TryGetJwtExpiryUtc(dto.AccessToken));
    }

    private static DateTimeOffset? TryGetJwtExpiryUtc(string jwt)
    {
        try
        {
            var jwtToken = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
            return new DateTimeOffset(jwtToken.ValidTo, TimeSpan.Zero);
        }
        catch
        {
            return null;
        }
    }

    private async Task<DataResponse<AuthTokenResponseDto?>> ReadLoginResponseAsync(
        HttpResponseMessage httpResponse,
        CancellationToken cancellationToken)
    {
        var text = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(text))
        {
            return new DataResponse<AuthTokenResponseDto?>
            {
                Success = false,
                Message = "Empty response from login.",
                Error = [$"HTTP {(int)httpResponse.StatusCode}"]
            };
        }

        var result = ParseTokenEnvelopeJson(text, httpResponse);
        if (result is { Success: true, Data.AccessToken: { Length: > 0 } })
            CommitSession(_authState, result.Data);
        return result;
    }

    private static DataResponse<AuthTokenResponseDto?> ParseTokenEnvelopeJson(
        string text,
        HttpResponseMessage httpResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;
            var result = new DataResponse<AuthTokenResponseDto?>();
            if (root.TryGetProperty("success", out var s))
                result.Success = s.GetBoolean();
            if (root.TryGetProperty("message", out var m))
                result.Message = m.GetString();
            if (root.TryGetProperty("error", out var e) && e.ValueKind is JsonValueKind.Array)
            {
                var err = e.EnumerateArray()
                    .Select(x => x.GetString() ?? string.Empty)
                    .Where(t => t.Length > 0)
                    .ToArray();
                if (err.Length > 0)
                    result.Error = err;
            }

            if (root.TryGetProperty("data", out var d))
            {
                if (d.ValueKind is JsonValueKind.String)
                {
                    var t = d.GetString() ?? string.Empty;
                    result.Data = new AuthTokenResponseDto { AccessToken = t, RefreshToken = null };
                }
                else if (d.ValueKind is JsonValueKind.Object)
                {
                    var dto = JsonSerializer.Deserialize<AuthTokenResponseDto>(d.GetRawText(), JsonOptions);
                    result.Data = dto;
                }
            }

            return result;
        }
        catch
        {
            return new DataResponse<AuthTokenResponseDto?>
            {
                Success = false,
                Message = "Could not read login response.",
                Error = [$"HTTP {(int)httpResponse.StatusCode}"]
            };
        }
    }

    private static async Task<DataResponse<AuthTokenResponseDto?>> ReadTokenPayloadEnvelopeAsync(
        HttpResponseMessage httpResponse,
        CancellationToken cancellationToken)
    {
        var text = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(text))
        {
            return new DataResponse<AuthTokenResponseDto?>
            {
                Success = false,
                Message = "Empty response from refresh."
            };
        }

        return ParseTokenEnvelopeJson(text, httpResponse);
    }
}

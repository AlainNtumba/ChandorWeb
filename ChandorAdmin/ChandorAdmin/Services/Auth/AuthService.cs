using System.Net.Http.Json;
using System.Text.Json;
using ChandorAdmin.Interfaces.Auth;
using ChandorProject.Shared.DTOs.User;
using ChandorProject.Shared.Models;
using System.IdentityModel.Tokens.Jwt;

namespace ChandorAdmin.Services.Auth;

/// <summary>HTTP client for <c>Auth</c> API (POST Auth/login).</summary>
public sealed class AuthService : IAuthService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAuthState _authState;
    private readonly ILogger<AuthService> _logger;

    /// <summary>
    /// Serializes login and proactive refresh so two callers cannot recurse or deadlock on the same credentials flow.
    /// </summary>
    private readonly SemaphoreSlim _loginGate = new(1, 1);

    public AuthService(
        IHttpClientFactory httpClientFactory,
        IAuthState authState,
        ILogger<AuthService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _authState = authState;
        _logger = logger;
    }

    public async Task<DataResponse<string>> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        await _loginGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var client = _httpClientFactory.CreateClient("ChandorApi.Auth");
            using var httpResponse = await client.PostAsJsonAsync("Auth/login", request, JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            var payload = await ReadDataResponseOrEmptyAsync<string>(httpResponse, cancellationToken).ConfigureAwait(false);

            if (httpResponse.IsSuccessStatusCode
                && payload is { Success: true, Data: { } token })
            {
                var exp = TryGetJwtExpiryUtc(token);
                _authState.SetSession(token, exp, request);
                return payload;
            }

            if (payload is not null)
                return payload;

            return FailureFromHttp<string>(httpResponse, "Login failed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Login request failed.");
            return new DataResponse<string>
            {
                Success = false,
                Message = "Login could not be completed.",
                Error = [ex.Message]
            };
        }
        finally
        {
            _loginGate.Release();
        }
    }

    public void Logout() => _authState.Clear();

    public async Task<DataResponse<string>?> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("ChandorApi.Auth");
            using var httpResponse = await client.PostAsJsonAsync("Auth/refresh-token", request, JsonOptions, cancellationToken)
                .ConfigureAwait(false);
            return await ReadDataResponseOrEmptyAsync<string>(httpResponse, cancellationToken).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh (explicit) request failed.");
            return new DataResponse<string>
            {
                Success = false,
                Message = "Refresh could not be completed.",
                Error = [ex.Message]
            };
        }
    }

    public async Task EnsureValidTokenAsync(CancellationToken cancellationToken = default)
    {
        var creds = _authState.RefreshCredentials;
        if (creds is null)
            return;

        var exp = _authState.AccessTokenExpiresAtUtc;
        var token = _authState.AccessToken;
        if (!string.IsNullOrEmpty(token) && exp is { } at && at > DateTimeOffset.UtcNow.AddMinutes(5))
            return;

        await _loginGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            creds = _authState.RefreshCredentials;
            if (creds is null)
                return;

            exp = _authState.AccessTokenExpiresAtUtc;
            token = _authState.AccessToken;
            if (!string.IsNullOrEmpty(token) && exp is { } at2 && at2 > DateTimeOffset.UtcNow.AddMinutes(5))
                return;

            var client = _httpClientFactory.CreateClient("ChandorApi.Auth");
            using var httpResponse = await client.PostAsJsonAsync("Auth/login", creds, JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            var payload = await ReadDataResponseOrEmptyAsync<string>(httpResponse, cancellationToken).ConfigureAwait(false);
            if (httpResponse.IsSuccessStatusCode && payload is { Success: true, Data: { } newToken })
            {
                _authState.SetSession(newToken, TryGetJwtExpiryUtc(newToken), creds);
                return;
            }

            var message = DescribeFailure(payload, httpResponse);
            _logger.LogWarning("Token refresh failed: {Message}", message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Token refresh threw.");
        }
        finally
        {
            _loginGate.Release();
        }
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

    private static async Task<DataResponse<T>?> ReadDataResponseOrEmptyAsync<T>(
        HttpResponseMessage httpResponse,
        CancellationToken cancellationToken)
    {
        try
        {
            return await httpResponse.Content.ReadFromJsonAsync<DataResponse<T>>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }

    private static DataResponse<T> FailureFromHttp<T>(HttpResponseMessage httpResponse, string fallback)
    {
        return new DataResponse<T>
        {
            Success = false,
            Message = string.IsNullOrWhiteSpace(httpResponse.ReasonPhrase) ? fallback : httpResponse.ReasonPhrase,
            Error = [$"HTTP {(int)httpResponse.StatusCode}"]
        };
    }

    private static string DescribeFailure<T>(DataResponse<T>? payload, HttpResponseMessage httpResponse)
    {
        if (payload?.Error is { Length: > 0 } errors)
            return string.Join("; ", errors.Where(e => !string.IsNullOrWhiteSpace(e)));
        if (!string.IsNullOrWhiteSpace(payload?.Message))
            return payload.Message!;
        return $"HTTP {(int)httpResponse.StatusCode} {httpResponse.ReasonPhrase}";
    }
}

using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ChandorAdmin.Configuration;
using ChandorAdmin.Interfaces.Auth;
using ChandorProject.Shared.Models;
using Microsoft.Extensions.Options;

namespace ChandorAdmin.Services.Api;

/// <summary>
/// Scoped HTTP helper for versioned API calls. Attaches the bearer token per request and handles 401 with refresh+retry.
/// </summary>
public sealed class ChandorApiHttp
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;
    private readonly IAuthState _authState;
    private readonly IAuthService _authService;
    private readonly string _versionedRoot;

    public ChandorApiHttp(
        IHttpClientFactory httpClientFactory,
        IOptions<ChandorApiOptions> options,
        IAuthState authState,
        IAuthService authService)
    {
        _http = httpClientFactory.CreateClient("ChandorApi");
        _authState = authState;
        _authService = authService;
        _versionedRoot = options.Value.VersionedRoot;
    }

    public Task<DataResponse<T>?> GetDataResponseAsync<T>(string relativeVersionedPath, CancellationToken cancellationToken = default)
        => SendDataResponseAsync<T>(HttpMethod.Get, relativeVersionedPath, content: null, cancellationToken);

    public Task<DataResponse<T>?> PostDataResponseAsync<T>(string relativeVersionedPath, HttpContent content, CancellationToken cancellationToken = default)
        => SendDataResponseAsync<T>(HttpMethod.Post, relativeVersionedPath, content, cancellationToken);

    public Task<DataResponse<T>?> PostMultipartDataResponseAsync<T>(string relativeVersionedPath, MultipartFormDataContent content, CancellationToken cancellationToken = default)
        => SendDataResponseAsync<T>(HttpMethod.Post, relativeVersionedPath, content, cancellationToken);

    public Task<DataResponse<T>?> PutDataResponseAsync<T>(string relativeVersionedPath, HttpContent content, CancellationToken cancellationToken = default)
        => SendDataResponseAsync<T>(HttpMethod.Put, relativeVersionedPath, content, cancellationToken);

    public Task<DataResponse<T>?> DeleteDataResponseAsync<T>(string relativeVersionedPath, CancellationToken cancellationToken = default)
        => SendDataResponseAsync<T>(HttpMethod.Delete, relativeVersionedPath, content: null, cancellationToken);

    /// <param name="relativeVersionedPath">Path after the version root (e.g. "ChurchProgram/get_congration_programs").</param>
    public async Task<DataResponse<T>?> SendDataResponseAsync<T>(
        HttpMethod method,
        string relativeVersionedPath,
        HttpContent? content,
        CancellationToken cancellationToken = default)
    {
        await _authService.EnsureValidTokenAsync(cancellationToken).ConfigureAwait(false);

        var path = relativeVersionedPath.TrimStart('/');
        var uri = _versionedRoot.TrimEnd('/') + "/" + path;

        byte[]? bodyBuffer = null;
        string? contentType = null;
        if (content is not null)
        {
            bodyBuffer = await content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            contentType = content.Headers.ContentType?.ToString();
        }

        var response = await CreateAndSendAsync(method, uri, bodyBuffer, contentType, cancellationToken).ConfigureAwait(false);
        try
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized
                && !string.IsNullOrEmpty(_authState.RefreshToken)
                && await _authService.TryRefreshTokenAsync(cancellationToken).ConfigureAwait(false))
            {
                response.Dispose();
                response = await CreateAndSendAsync(method, uri, bodyBuffer, contentType, cancellationToken)
                    .ConfigureAwait(false);
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                var hasSession = !string.IsNullOrEmpty(_authState.AccessToken) || !string.IsNullOrEmpty(_authState.RefreshToken);
                if (hasSession)
                    await _authService.LogoutAsync(cancellationToken, redirectToLogin: true).ConfigureAwait(false);

                return await TryReadDataResponseBodyAsync<T>(response, cancellationToken).ConfigureAwait(false);
            }

            return await response.Content.ReadFromJsonAsync<DataResponse<T>>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            response.Dispose();
        }
    }

    private async Task<HttpResponseMessage> CreateAndSendAsync(
        HttpMethod method,
        string uri,
        byte[]? bodyBuffer,
        string? contentType,
        CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(method, uri);
        if (bodyBuffer is not null)
        {
            request.Content = new ByteArrayContent(bodyBuffer);
            if (contentType is not null)
                request.Content!.Headers.ContentType = MediaTypeHeaderValue.Parse(contentType);
        }

        var token = _authState.AccessToken;
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return await _http.SendAsync(request, cancellationToken).ConfigureAwait(false);
    }

    private static async Task<DataResponse<T>?> TryReadDataResponseBodyAsync<T>(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        try
        {
            return await response.Content.ReadFromJsonAsync<DataResponse<T>>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);
        }
        catch
        {
            return null;
        }
    }
}

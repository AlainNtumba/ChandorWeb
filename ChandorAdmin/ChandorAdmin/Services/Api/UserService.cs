using System.Net.Http.Json;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.User;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class UserService(ChandorApiHttp api) : IUserService
{
    private const string C = "User";

    public Task<DataResponse<IEnumerable<ViewUserDto>>?> GetAllUsersAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<ViewUserDto>>($"{C}/get-all-users", cancellationToken);

    public Task<DataResponse<ViewUserDto>?> GetUserByUsernameAsync(string username, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<ViewUserDto>($"{C}/get-user-by-username/{Uri.EscapeDataString(username)}", cancellationToken);

    public async Task<DataResponse<ViewUserDto>?> AddUserAsync(Stream? fileContent, string? fileName, string username, string password, CancellationToken cancellationToken = default)
    {
        using var form = new MultipartFormDataContent();
        if (fileContent is not null && !string.IsNullOrWhiteSpace(fileName))
            form.Add(new StreamContent(fileContent), "file", fileName);

        form.Add(new StringContent(username), "username");
        form.Add(new StringContent(password), "password");

        return await api.PostMultipartDataResponseAsync<ViewUserDto>($"{C}/add-user", form, cancellationToken).ConfigureAwait(false);
    }
}

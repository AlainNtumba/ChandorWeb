using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.AppUser;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Services.Api;

public sealed class AppUserService(ChandorApiHttp api) : IAppUserService
{
    private const string C = "AppUser";
    public Task<DataResponse<AppUserViewDto>?> RegisterAppUserAsync(
        string email,
        string password,
        string phone,
        Stream? fileContent,
        string? fileName,
        CancellationToken cancellationToken = default)
    {
        var form = new MultipartFormDataContent();
        form.Add(new StringContent(email, Encoding.UTF8), "Email");
        form.Add(new StringContent(password, Encoding.UTF8), "Password");
        form.Add(new StringContent(phone ?? string.Empty, Encoding.UTF8), "Phone");
        if (fileContent is not null && !string.IsNullOrWhiteSpace(fileName))
            form.Add(new StreamContent(fileContent), "File", fileName);
        return RegisterAppUserAsync(form, cancellationToken);
    }

    public Task<DataResponse<AppUserViewDto>?> RegisterAppUserAsync(MultipartFormDataContent form, CancellationToken cancellationToken = default)
        => api.PostMultipartDataResponseAsync<AppUserViewDto>($"{C}/register", form, cancellationToken);

    public Task<DataResponse<string>?> LoginAppUserAsync(LoginAppUserDto request, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<string>($"{C}/login", JsonContent.Create(request), cancellationToken);

    public Task<DataResponse<AppUserViewDto>?> ConfirmEmailAsync(ConfirmAppUserEmailDto request, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<AppUserViewDto>($"{C}/confirm-email", JsonContent.Create(request), cancellationToken);

    public Task<DataResponse<AppUserViewDto>?> ResendConfirmationCodeAsync(ResendAppUserEmailCodeDto request, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<AppUserViewDto>($"{C}/resend-confirmation-code", JsonContent.Create(request), cancellationToken);

    public Task<DataResponse<IReadOnlyList<AppUserViewDto>>?> GetAllPagedAsync(int page, int size, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<AppUserViewDto>>($"{C}/get-all-paged?page={page}&size={size}", cancellationToken);
}

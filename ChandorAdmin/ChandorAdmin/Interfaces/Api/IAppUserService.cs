using System.Net.Http;
using ChandorProject.Shared.DTOs.AppUser;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IAppUserService
{
    Task<DataResponse<AppUserViewDto>?> RegisterAppUserAsync(
        string email,
        string password,
        string phone,
        Stream? fileContent,
        string? fileName,
        CancellationToken cancellationToken = default);

    Task<DataResponse<AppUserViewDto>?> RegisterAppUserAsync(MultipartFormDataContent form, CancellationToken cancellationToken = default);

    Task<DataResponse<string>?> LoginAppUserAsync(LoginAppUserDto request, CancellationToken cancellationToken = default);

    Task<DataResponse<AppUserViewDto>?> ConfirmEmailAsync(ConfirmAppUserEmailDto request, CancellationToken cancellationToken = default);

    Task<DataResponse<AppUserViewDto>?> ResendConfirmationCodeAsync(ResendAppUserEmailCodeDto request, CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<AppUserViewDto>>?> GetAllPagedAsync(int page, int size, CancellationToken cancellationToken = default);
}

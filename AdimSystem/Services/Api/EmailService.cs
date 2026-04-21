using System.Net.Http.Json;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.Email;
using ChandorProject.Shared.Models;

namespace AdimSystem.Services.Api;

public sealed class EmailService(ChandorApiHttp api) : IEmailService
{
    private const string C = "Email";

    public Task<DataResponse<EmailDto>?> CreateEmailAsync(NewEmailDto email, CancellationToken cancellationToken = default)
        => api.PostDataResponseAsync<EmailDto>($"{C}/create-email", JsonContent.Create(email), cancellationToken);

    public Task<DataResponse<EmailDto>?> UpdateEmailAsync(EmailDto email, CancellationToken cancellationToken = default)
        => api.PutDataResponseAsync<EmailDto>($"{C}/update-email", JsonContent.Create(email), cancellationToken);

    public Task<DataResponse<bool>?> DeleteEmailAsync(Guid id, CancellationToken cancellationToken = default)
        => api.DeleteDataResponseAsync<bool>($"{C}/delete-email/{id}", cancellationToken);

    public Task<DataResponse<EmailDto>?> GetEmailByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<EmailDto>($"{C}/get-by-id/{id}", cancellationToken);

    public Task<DataResponse<IEnumerable<EmailDto>>?> GetAllEmailsAsync(CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IEnumerable<EmailDto>>($"{C}/get-all", cancellationToken);

    public Task<DataResponse<IReadOnlyList<EmailDto>>?> GetPagedEmailsAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => api.GetDataResponseAsync<IReadOnlyList<EmailDto>>($"{C}/get-paged-data?page={page}&pageSize={pageSize}", cancellationToken);
}

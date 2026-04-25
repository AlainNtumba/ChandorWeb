using ChandorProject.Shared.DTOs.Email;
using ChandorProject.Shared.Models;

namespace ChandorAdmin.Interfaces.Api;

public interface IEmailService
{
    Task<DataResponse<EmailDto>?> CreateEmailAsync(NewEmailDto email, CancellationToken cancellationToken = default);

    Task<DataResponse<EmailDto>?> UpdateEmailAsync(EmailDto email, CancellationToken cancellationToken = default);

    Task<DataResponse<bool>?> DeleteEmailAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<EmailDto>?> GetEmailByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<DataResponse<IEnumerable<EmailDto>>?> GetAllEmailsAsync(CancellationToken cancellationToken = default);

    Task<DataResponse<IReadOnlyList<EmailDto>>?> GetPagedEmailsAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}

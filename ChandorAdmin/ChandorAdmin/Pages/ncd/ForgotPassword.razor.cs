using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Member;
using Microsoft.AspNetCore.Components;

namespace ChandorAdmin.Pages.ncd;

public partial class ForgotPassword
{
    [Inject]
    public IMemberService MemberService { get; set; } = null!;

    public RequestResetPasswordDto Request { get; set; } = new();

    public string StatusMessage { get; set; } = string.Empty;

    public bool StatusIsError { get; set; }

    public bool IsProcessing { get; set; }

    public async Task SubmitAsync()
    {
        StatusMessage = string.Empty;
        StatusIsError = false;

        var hasEmail = !string.IsNullOrWhiteSpace(Request.Email);
        var hasPhone = !string.IsNullOrWhiteSpace(Request.PhoneNumber);
        if (!hasEmail && !hasPhone)
        {
            StatusIsError = true;
            StatusMessage = "Please enter an email address or a phone number.";
            return;
        }

        IsProcessing = true;
        try
        {
            var response = await MemberService.RequestResetPasswordAsync(Request).ConfigureAwait(false);
            if (response is { Success: true })
            {
                StatusIsError = false;
                StatusMessage = "If an account matches the details you entered, reset instructions have been sent.";
                Request = new RequestResetPasswordDto();
            }
            else
            {
                StatusIsError = true;
                StatusMessage = response?.Message is { Length: > 0 } m
                    ? m
                    : "We could not process this request. Please try again later.";
            }
        }
        finally
        {
            IsProcessing = false;
        }
    }
}

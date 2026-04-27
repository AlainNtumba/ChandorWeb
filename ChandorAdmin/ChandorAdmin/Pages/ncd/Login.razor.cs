using ChandorAdmin.Configuration;
using ChandorAdmin.Interfaces.Auth;
using ChandorProject.Shared.DTOs.User;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace ChandorAdmin.Pages.ncd;

public partial class Login
{
    [Inject] public NavigationManager NavMgr { get; set; } = null!;
    [Inject] public IAuthService AuthService { get; set; } = null!;
    [Inject] public AuthenticationStateProvider AuthenticationStateProvider { get; set; } = null!;
    public LoginRequestDto User { get; set; } = new();
    public CustomFormValidator FormValidator { get; set; } = new();
    public Dictionary<string, List<string>> Errors { get; set; } = new();
    public string LoginForm { get; set; } = "login-form";
    public string ErrorMsg { get; set; } = string.Empty;
    public bool IsProcessing { get; set; } = false;

    protected override async Task OnInitializedAsync()
    {
        var s = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        if (s.User.Identity is { IsAuthenticated: true })
            NavMgr.NavigateTo("/");
    }

    public async Task AuthenticateUser()
    {
        ErrorMsg = "";
        Errors.Clear();
        FormValidator.ClearFormErrors();

        if (string.IsNullOrEmpty(User.Username) || string.IsNullOrEmpty(User.Password))
        {
            ErrorMsg = "Invalid username or password";
            Errors.Add(nameof(User.Username), new List<string> { "" });
            Errors.Add(nameof(User.Password), new List<string> { "" });
            FormValidator.DisplayFormErrors(Errors);
            StateHasChanged();
        }
        else
        {
            IsProcessing = true;

            var authUser = await AuthService.LoginAsync(User);

            IsProcessing = false;

            if (authUser != null)
            {
                if (authUser.Success)
                {
                    NavMgr.NavigateTo("/");
                }
                else
                {
                    ErrorMsg = authUser.Message is not null ? authUser.Message : "Failed authentication";
                    Errors.Add(nameof(User.Username), new List<string> { "" });
                    Errors.Add(nameof(User.Password), new List<string> { "" });
                    FormValidator.DisplayFormErrors(Errors);
                    StateHasChanged();
                }
            }
            else
            {
                ErrorMsg = "Failed authentication";
                Errors.Add(nameof(User.Username), new List<string> { "" });
                Errors.Add(nameof(User.Password), new List<string> { "" });
                FormValidator.DisplayFormErrors(Errors);
                StateHasChanged();
            }
        }
    }
}
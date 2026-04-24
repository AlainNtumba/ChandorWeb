using AdimSystem.Components.Shared;
using AdimSystem.Interfaces;
using ChandorProject.Shared.DTOs.User;
using Microsoft.AspNetCore.Components;

namespace AdimSystem.Components.Pages.ncd;

public partial class Login
{
    [Inject] public IAuthService _auth { get; set; } = default!;
    [Inject] public NavigationManager _nav { get; set; } = default!;
    public CustomFormValidator FormValidator { get; set; } = new();
    public LoginRequestDto User { get; set; } = new();
    public Dictionary<string, List<string>> Errors { get; set; } = new();
    public string LoginForm { get; set; } = "login-form";
    public string ErrorMsg { get; set; } = string.Empty;
    public bool IsProcessing { get; set; } = false;

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

            var authUser = await _auth.LoginAsync(User);

            IsProcessing = false;

            if (authUser != null)
            {
                if (authUser.Success)
                {
                    _nav.NavigateTo("/");
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
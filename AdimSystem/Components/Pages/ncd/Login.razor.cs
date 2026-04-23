using AdimSystem.Components.Shared;
using ChandorProject.Shared.Models;

namespace AdimSystem.Components.Pages.ncd;

public partial class Login
{
    public CustomFormValidator FormValidator { get; set; } = new();
    public LoginRequest User { get; set; } = new();
}
using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.User;

public class LoginRequestDto
{
    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Password { get; set; } = string.Empty;
}

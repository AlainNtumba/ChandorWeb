using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.Member;

public class MemberRegistrationDto
{
    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress(ErrorMessage = "Username must be a valid email address.")]
    [StringLength(256, MinimumLength = 5, ErrorMessage = "Username email must be between 5 and 256 characters.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public string Surname { get; set; } = string.Empty;

    public string Postname { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public DateTime Birthday { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public bool Gender { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string PhoneNumber { get; set; } = string.Empty;
}

public class MemberProfileDto
{
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string ProfileLink { get; set; } = string.Empty;
    public Guid MemberTypeId { get; set; }
}

public class RequestResetPasswordDto
{
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? PhoneNumber { get; set; } = string.Empty;
}

public class PasswordResetSessionDto
{
    public Guid SessionId { get; set; }
    public DateTime ExpirationTime { get; set; }
    public string SecretCode { get; set; } = string.Empty;
}

public class ConfirmResetPasswordDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid SessionId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public string SecretCode { get; set; } = string.Empty;
}

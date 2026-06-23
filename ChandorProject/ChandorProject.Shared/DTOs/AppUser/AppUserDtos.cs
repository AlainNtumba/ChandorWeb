using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ChandorProject.Shared.DTOs.AppUser;

public class RegisterAppUserDto
{
    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(256, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Password { get; set; } = string.Empty;

    [StringLength(55, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Phone { get; set; } = string.Empty;

    public IFormFile? File { get; set; }
}

public class LoginAppUserDto
{
    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(256, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Password { get; set; } = string.Empty;
}

public class ConfirmAppUserEmailDto
{
    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(256, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must have 6 characters.")]
    public string Code { get; set; } = string.Empty;
}

public class ResendAppUserEmailCodeDto
{
    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(256, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Email { get; set; } = string.Empty;
}

public class AppUserViewDto
{
    public Guid Id { get; set; }
    public Guid MemberId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string ProfilPicture { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public DateTime Updated { get; set; }
    public AppUserMemberViewDto? Member { get; set; }
}

public class AppUserMemberViewDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Postname { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }
    public bool Gender { get; set; }
    public string Country { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public Guid AgeGroupId { get; set; }
    public Guid MemberTypeId { get; set; }
}

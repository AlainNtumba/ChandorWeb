using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.Email;

public class EmailDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(254, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberId { get; set; }
}

public class NewEmailDto
{
    [Required(ErrorMessage = "This field is required.")]
    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    [StringLength(254, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string EmailAddress { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberId { get; set; }
}

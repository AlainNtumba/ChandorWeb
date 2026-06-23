using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.Telephone;

public class TelephoneDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Number { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberId { get; set; }
}

public class NewTelephoneDto
{
    [Required(ErrorMessage = "This field is required.")]
    [Phone(ErrorMessage = "Invalid phone number format.")]
    [StringLength(20, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Number { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberId { get; set; }
}

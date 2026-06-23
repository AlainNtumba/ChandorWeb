using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.Account;

public class AccountDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string AccountNumber { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Description { get; set; } = string.Empty;
}

public class NewAccountDto
{
    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string AccountNumber { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Description { get; set; } = string.Empty;
}

using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.MemberType;

public class MemberTypeDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Description { get; set; } = string.Empty;
}

public class NewMemberTypeDto
{
    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Description { get; set; } = string.Empty;
}

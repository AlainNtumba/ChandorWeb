using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.AgeGroup;

public class AgeGroupDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string GroupName { get; set; } = string.Empty;

    [Range(0, 150, ErrorMessage = "The field value is out of range.")]
    public int FromAge { get; set; }

    [Range(0, 150, ErrorMessage = "The field value is out of range.")]
    public int ToAge { get; set; }
}

public class NewAgeGroupDto
{
    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string GroupName { get; set; } = string.Empty;

    [Range(0, 150, ErrorMessage = "The field value is out of range.")]
    public int FromAge { get; set; }

    [Range(0, 150, ErrorMessage = "The field value is out of range.")]
    public int ToAge { get; set; }
}

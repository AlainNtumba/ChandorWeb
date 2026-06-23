using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.DepartmentTeamDto;

public class DepartmentTeamDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Address { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid DepartmentId { get; set; }
}

public class NewDepartmentTeamDto
{
    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Address { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid DepartmentId { get; set; }
}

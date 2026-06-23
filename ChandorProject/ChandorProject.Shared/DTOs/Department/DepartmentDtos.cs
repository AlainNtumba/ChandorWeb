using System;
using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.Department;

public class DepartmentDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid UserId { get; set; }
}

public class NewDepartmentDto
{
    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Name { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Description { get; set; } = string.Empty;
}

public class UpdateDepartmentDto : NewDepartmentDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }
}

public class ChurchDepartmentKeyDto
{
    public Guid DepartmentId { get; set; }
    public Guid DepartmentTeamId { get; set; }
}


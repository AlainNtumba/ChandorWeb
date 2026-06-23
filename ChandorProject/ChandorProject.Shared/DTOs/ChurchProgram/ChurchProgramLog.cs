using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.ChurchProgram;

public class ChurchProgramLog 
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime StartTime { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime EndTime { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(150, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Theme { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(200, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Lieu { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Description { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string PosterLink { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public bool IsApproved { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string ProgramType { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string DepartmentTeam { get; set; } = string.Empty;

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime Created { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime Updated { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string ChangedBy { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(50, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string ChangeType { get; set; } = string.Empty;
}

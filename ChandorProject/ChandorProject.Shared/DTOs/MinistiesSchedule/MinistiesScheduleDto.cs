using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.MinistiesSchedule;

public class MinistiesScheduleDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime StartTime { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime EndTime { get; set; }

    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Note { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid ChurchProgramId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MinistryId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberId { get; set; }
}

public class NewMinistiesScheduleDto
{
    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime StartTime { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime EndTime { get; set; }

    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Note { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid ChurchProgramId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MinistryId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberId { get; set; }
}

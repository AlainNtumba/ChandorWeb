using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.Attendance;

public class AttendanceDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime EventDate { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime StartTime { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime EndTime { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "The field value is out of range.")]
    public int Attendants { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "The field value is out of range.")]
    public int NewComers { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "The field value is out of range.")]
    public int BornAgain { get; set; }

    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Note { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid ChurchProgramId { get; set; }
}

public class NewAttendanceDto
{
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime EventDate { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime StartTime { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime EndTime { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "The field value is out of range.")]
    public int Attendants { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "The field value is out of range.")]
    public int NewComers { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "The field value is out of range.")]
    public int BornAgain { get; set; }

    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Note { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid ChurchProgramId { get; set; }
}

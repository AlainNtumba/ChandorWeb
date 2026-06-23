using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.Outreaches;

public class OutreachesDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime OutreachDate { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(200, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string OutreachArea { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "The field value is out of range.")]
    public int TotalOutreached { get; set; }
}

public class NewOutreachesDto
{
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime OutreachDate { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(200, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string OutreachArea { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "The field value is out of range.")]
    public int TotalOutreached { get; set; }
}

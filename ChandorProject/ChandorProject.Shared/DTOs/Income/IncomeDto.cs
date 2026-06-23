using System.ComponentModel.DataAnnotations;
using ChandorProject.Shared.Validation;

namespace ChandorProject.Shared.DTOs.Income;

public class IncomeDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime IncomeDate { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid IncomeTypeId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid DepartmentId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid DepartmentTeamId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid AccountId { get; set; }

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }

    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Note { get; set; } = string.Empty;
}

public class NewIncomeDto
{
    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime IncomeDate { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid IncomeTypeId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid DepartmentId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid DepartmentTeamId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid AccountId { get; set; }

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }

    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Note { get; set; } = string.Empty;
}

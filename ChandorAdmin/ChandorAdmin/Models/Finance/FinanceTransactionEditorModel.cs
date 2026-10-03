using System.ComponentModel.DataAnnotations;
using ChandorProject.Shared.Validation;

namespace ChandorAdmin.Models.Finance;

/// <summary>
/// Edit model for the finance transaction dialog; maps to
/// <see cref="ChandorProject.Shared.DTOs.Transaction.NewTransactionDto"/> on insert
/// and <see cref="ChandorProject.Shared.DTOs.Transaction.TransactionDto"/> on edit.
/// </summary>
public sealed class FinanceTransactionEditorModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Choose a transaction type.")]
    public Guid? TransactionTypeId { get; set; }

    [Required(ErrorMessage = "Date is required.")]
    public DateTime? DateValue { get; set; }

    [Required(ErrorMessage = "Time is required.")]
    public DateTime? TimeValue { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public Guid? CategoryId { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    [DecimalRange(ErrorMessage = "Amount must be at least 0.01.")]
    public decimal? Amount { get; set; }

    [Required(ErrorMessage = "Currency is required.")]
    public Guid? CurrencyId { get; set; }

    [StringLength(1000, ErrorMessage = "Note cannot exceed 1000 characters.")]
    public string TransactionNote { get; set; } = "";

    [Required(ErrorMessage = "Account is required.")]
    public Guid? AccountId { get; set; }

    public Guid DepartmentId { get; set; }
    public Guid DepartmentTeamId { get; set; }
}

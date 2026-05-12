using System.ComponentModel.DataAnnotations;

namespace ChandorAdmin.Models.Finance;

/// <summary>
/// Edit model for the finance transaction dialog; bound to Blazor EditForm with Syncfusion inputs.
/// </summary>
public sealed class FinanceTransactionEditorModel
{
    public string UniqueId { get; set; } = "";

    [Required(ErrorMessage = "Choose Income or Expense.")]
    public string TransactionType { get; set; } = "Expense";

    [Required(ErrorMessage = "Date is required.")]
    public DateTime? DateValue { get; set; }

    [Required(ErrorMessage = "Time is required.")]
    public DateTime? TimeValue { get; set; }

    [Required(ErrorMessage = "Category is required.")]
    public string? Category { get; set; }

    [Required(ErrorMessage = "Amount is required.")]
    public double? Amount { get; set; }

    [StringLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string Description { get; set; } = "";

    [Required(ErrorMessage = "Payment mode is required.")]
    public string PaymentMode { get; set; } = "Cash";
}

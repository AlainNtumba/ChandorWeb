namespace ChandorAdmin.Models.Finance;

/// <summary>
/// Sample transaction row aligned with the Syncfusion Expense Tracker schema (replace with API DTOs later).
/// </summary>
public sealed class FinanceTransaction
{
    public string UniqueId { get; set; } = "";
    public string TransactionType { get; set; } = "Expense";
    public DateTime DateTime { get; set; }
    public string Category { get; set; } = "";
    public string PaymentMode { get; set; } = "";
    public string Description { get; set; } = "";
    public int Amount { get; set; }
    public string MonthShort { get; set; } = "";
    public string MonthFull { get; set; } = "";
    public string FormattedDate { get; set; } = "";
}

public sealed class FinanceCategoryOption
{
    public string Category { get; set; } = "";
    public string Class { get; set; } = "";
    public string Id { get; set; } = "";
}

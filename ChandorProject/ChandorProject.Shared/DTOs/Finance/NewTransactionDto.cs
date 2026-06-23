using System.ComponentModel.DataAnnotations;
using ChandorProject.Shared.Validation;

namespace ChandorProject.Shared.DTOs.Finance;

public class NewTransactionDto : NewChurchTransactionDto
{
    public Guid DepartmentId { get; set; }
    public Guid DepartmentTeamId { get; set; }
}

public class TransactionDto 
{
    public Guid Id { get; set; }
    public DateTime TransactionDate { get; set; }
    public string TransactionCategory { get; set; } = string.Empty;
    public string TransactionType { get; set; } = string.Empty;

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }
    public string AccountName { get; set; } = string.Empty;
    public string TransactionNote { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
}

public class NewChurchTransactionDto
{
    [Required]
    public DateTime TransactionDate { get; set; }
    [Required]
    public string TransactionType { get; set; } = string.Empty;
    public string TransactionNote { get; set; } = string.Empty;

    [Required]
    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }
    public Guid CategoryId { get; set; }
    public Guid AccountId { get; set; }
}

public class TransactionTypeDto
{
    public string Value { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class FinanceActivityItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Subtitle { get; set; } = string.Empty;

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }
    public DateTime OccurredAt { get; set; }
    public bool IsExpense { get; set; }
}

public class CashflowSeriesPointDto
{
    public string MonthLabel { get; set; } = string.Empty;

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Income { get; set; }

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Expenses { get; set; }
}

public class ExpenseCategoryDto
{
    public string Category { get; set; } = string.Empty;

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }
}

public class IncomeCategoryDto
{
    public string Category { get; set; } = string.Empty;

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }
}

public class FinanceSummaryDto
{
    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal TotalIncome { get; set; }

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal TotalExpenses { get; set; }

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Balance { get; set; }
    public int TransactionCount { get; set; }
}
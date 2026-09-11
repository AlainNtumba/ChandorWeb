using ChandorProject.Shared.Validation;

namespace ChandorProject.Shared.DTOs.Transaction;

public class TransactionDto
{
    public Guid Id { get; set; }
    public DateTime TransactionDate { get; set; }

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }
    public string TransactionNote { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public Guid DepartmentTeamId { get; set; }
    public Guid CurrencyId { get; set; }
    public Guid AccountId { get; set; }
    public Guid TransactionCategoryId { get; set; }
    public Guid TransactionTypeId { get; set; }
}

public class TransactionView
{
    public Guid Id { get; set; }
    public DateTime TransactionDate { get; set; }

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }
    public string TransactionNote { get; set; } = string.Empty;
    public Guid DepartmentId { get; set; }
    public string Department { get; set; } = string.Empty;
    public Guid DepartmentTeamId { get; set; }
    public string DepartmentTeam { get; set; } = string.Empty;
    public Guid CurrencyId { get; set; }
    public string Currency { get; set; } = string.Empty;
    public Guid AccountId { get; set; }
    public string Account { get; set; } = string.Empty;
    public Guid TransactionCategoryId { get; set; }
    public string TransactionCategory { get; set; } = string.Empty;
    public Guid TransactionTypeId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
}

public class NewCongragationTransactionDto
{
    public DateTime TransactionDate { get; set; }

    [DecimalRange(ErrorMessage = "The field value is out of range.")]
    public decimal Amount { get; set; }
    public string TransactionNote { get; set; } = string.Empty;
    public Guid CurrencyId { get; set; }
    public Guid AccountId { get; set; }
    public Guid TransactionCategoryId { get; set; }
    public Guid TransactionTypeId { get; set; }
}

public class NewTransactionDto : NewCongragationTransactionDto
{
    public Guid DepartmentId { get; set; }
    public Guid DepartmentTeamId { get; set; }
}

public class CurrencyBalanceDto
{
    public string CurrencySymbol { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    public decimal TotalBalance { get; set; }
    public DateTime ByDateTime { get; set; }
}

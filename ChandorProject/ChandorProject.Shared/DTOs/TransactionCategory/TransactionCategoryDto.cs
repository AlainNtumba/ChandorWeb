namespace ChandorProject.Shared.DTOs.TransactionCategory;

public class TransactionCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TransactionTypeId { get; set; }
}

public class NewTransactionCategoryDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid TransactionTypeId { get; set; }
}

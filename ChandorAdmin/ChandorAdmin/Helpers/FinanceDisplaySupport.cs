using System.Globalization;
using ChandorProject.Shared.DTOs.Currency;

namespace ChandorAdmin.Helpers;

internal static class FinanceDisplaySupport
{
    public const string IncomeCss = "Income";
    public const string ExpenseCss = "Expense";
    public const string UsdSymbol = "$";

    public static CurrencyDto? SelectDefaultCurrency(IEnumerable<CurrencyDto>? currencies)
    {
        var list = currencies?.Where(c => c.Id != Guid.Empty).ToList() ?? [];
        if (list.Count == 0)
            return null;

        return list.FirstOrDefault(c => string.Equals(c.Symbol?.Trim(), UsdSymbol, StringComparison.Ordinal))
            ?? list.FirstOrDefault(c => string.Equals(c.Name?.Trim(), "USD", StringComparison.OrdinalIgnoreCase))
            ?? list[0];
    }

    public static bool IsIncome(string? transactionTypeName)
    {
        if (string.IsNullOrWhiteSpace(transactionTypeName))
            return false;

        var name = transactionTypeName.Trim();
        return name.Equals("Income", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Revenu", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Entrée", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Entree", StringComparison.OrdinalIgnoreCase)
            || name.Contains("income", StringComparison.OrdinalIgnoreCase)
            || name.Contains("revenu", StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsExpense(string? transactionTypeName)
    {
        if (string.IsNullOrWhiteSpace(transactionTypeName))
            return false;

        var name = transactionTypeName.Trim();
        return name.Equals("Expense", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Dépense", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Depense", StringComparison.OrdinalIgnoreCase)
            || name.Equals("Sortie", StringComparison.OrdinalIgnoreCase)
            || name.Contains("expense", StringComparison.OrdinalIgnoreCase)
            || name.Contains("dépense", StringComparison.OrdinalIgnoreCase)
            || name.Contains("depense", StringComparison.OrdinalIgnoreCase);
    }

    public static string CssClass(string? transactionTypeName) =>
        IsExpense(transactionTypeName) ? ExpenseCss : IncomeCss;

    public static string FormatAmount(decimal amount, string? symbol)
    {
        var sign = amount < 0 ? "-" : string.Empty;
        var body = Math.Abs(amount).ToString("N0", CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(symbol)
            ? sign + body
            : $"{sign}{symbol}{body}";
    }
}

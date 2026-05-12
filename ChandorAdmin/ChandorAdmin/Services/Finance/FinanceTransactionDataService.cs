using System.Globalization;
using ChandorAdmin.Models.Finance;

namespace ChandorAdmin.Services.Finance;

/// <summary>
/// In-memory transaction store and filter range (mirrors ExpenseDataService from the Syncfusion showcase).
/// </summary>
public sealed class FinanceTransactionDataService
{
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public List<FinanceCategoryOption> CategoryIncomeData { get; }
    public List<FinanceCategoryOption> CategoryExpenseData { get; }
    public List<FinanceTransaction> Transactions { get; }
    public string CurrentBalance { get; private set; } = "$0";
    public IEnumerable<FinanceTransaction>? CurrentView { get; private set; }
    public event Action? Changed;

    public FinanceTransactionDataService()
    {
        (StartDate, EndDate) = GetCalendarMonthBounds(DateTime.Today);

        CategoryIncomeData =
        [
            new FinanceCategoryOption { Class = "category-icon Salary", Category = "Salary", Id = "Salary" },
            new FinanceCategoryOption { Class = "category-icon Interests", Category = "Interests", Id = "Interests" },
            new FinanceCategoryOption { Class = "category-icon Business", Category = "Business", Id = "Business" },
            new FinanceCategoryOption { Class = "category-icon Extra", Category = "Extra income", Id = "Extra income" }
        ];

        CategoryExpenseData =
        [
            new FinanceCategoryOption { Class = "category-icon Rent", Category = "Rent", Id = "Rent" },
            new FinanceCategoryOption { Class = "category-icon Food", Category = "Food", Id = "Food" },
            new FinanceCategoryOption { Class = "category-icon Bills", Category = "Bills", Id = "Bills" },
            new FinanceCategoryOption { Class = "category-icon Utilities", Category = "Utilities", Id = "Utilities" },
            new FinanceCategoryOption { Class = "category-icon Transportation", Category = "Transportation", Id = "Transportation" },
            new FinanceCategoryOption { Class = "category-icon Insurance", Category = "Insurance", Id = "Insurance" },
            new FinanceCategoryOption { Class = "category-icon Shopping", Category = "Shopping", Id = "Shopping" },
            new FinanceCategoryOption { Class = "category-icon Entertainment", Category = "Entertainment", Id = "Entertainment" },
            new FinanceCategoryOption { Class = "category-icon Health", Category = "Health", Id = "Health" },
            new FinanceCategoryOption { Class = "category-icon Housing", Category = "Housing", Id = "Housing" },
            new FinanceCategoryOption { Class = "category-icon Taxes", Category = "Tax", Id = "Tax" },
            new FinanceCategoryOption { Class = "category-icon Clothing", Category = "Clothing", Id = "Clothing" },
            new FinanceCategoryOption { Class = "category-icon Education", Category = "Education", Id = "Education" },
            new FinanceCategoryOption { Class = "category-icon Miscellaneous", Category = "Miscellaneous", Id = "Miscellaneous" },
            new FinanceCategoryOption { Class = "category-icon Personal", Category = "Personal", Id = "Personal" }
        ];

        Transactions = BuildSeedTransactions();
    }

    /// <summary>First instant of <paramref name="reference"/>'s month through last instant of that month (local calendar).</summary>
    public static (DateTime Start, DateTime End) GetCalendarMonthBounds(DateTime reference)
    {
        var day = reference.Date;
        var start = new DateTime(day.Year, day.Month, 1, 0, 0, 0);
        var end = new DateTime(day.Year, day.Month, DateTime.DaysInMonth(day.Year, day.Month), 23, 59, 59);
        return (start, end);
    }

    public void SetDate(DateTime start, DateTime end)
    {
        StartDate = start;
        EndDate = end;
    }

    public void SetCurrentView(IEnumerable<FinanceTransaction> rows) => CurrentView = rows;

    public void UpdateCurrentBalance(string formatted) => CurrentBalance = formatted;

    public void NotifyChanged() => Changed?.Invoke();

    public static string FormatBalance(int incomeSum, int expenseSum)
    {
        var n = incomeSum - expenseSum;
        var sign = n < 0 ? "-" : "";
        n = Math.Abs(n);
        return sign + "$" + n.ToString("N0", CultureInfo.InvariantCulture);
    }

    private static List<FinanceTransaction> BuildSeedTransactions()
    {
        var rnd = new Random(201906);
        var list = new List<FinanceTransaction>();
        var expenseCategories = new[] { "Food", "Transportation", "Bills", "Utilities", "Shopping", "Entertainment", "Health", "Rent", "Insurance", "Miscellaneous" };
        var incomeCategories = new[] { "Salary", "Interests", "Business", "Extra income" };
        var payments = new[] { "Cash", "Credit Card", "Debit Card" };
        int id = 100001;

        for (var month = 6; month <= 11; month++)
        {
            var daysInMonth = DateTime.DaysInMonth(2019, month);
            for (var d = 1; d <= daysInMonth; d += rnd.Next(1, 3))
            {
                if (list.Count >= 120)
                    break;

                var dt = new DateTime(2019, month, Math.Min(d, daysInMonth), rnd.Next(8, 20), rnd.Next(0, 59), 0);
                var isIncome = rnd.NextDouble() < 0.18;
                var cat = isIncome ? incomeCategories[rnd.Next(incomeCategories.Length)] : expenseCategories[rnd.Next(expenseCategories.Length)];
                var amount = isIncome ? rnd.Next(80, 3200) : rnd.Next(5, 450);
                var pay = payments[rnd.Next(payments.Length)];
                var desc = isIncome ? "Recorded income" : "Recorded expense";

                list.Add(CreateRow("T" + id++, dt, cat, pay, isIncome ? "Income" : "Expense", amount, desc));
            }
        }

        while (list.Count < 55)
        {
            var dt = new DateTime(2019, rnd.Next(6, 12), rnd.Next(1, 28), 12, 0, 0);
            list.Add(CreateRow("T" + id++, dt, "Food", "Cash", "Expense", rnd.Next(5, 40), "Snack run"));
        }

        return list;
    }

    private static FinanceTransaction CreateRow(string uniqueId, DateTime dt, string category, string payment, string type, int amount, string description)
    {
        return new FinanceTransaction
        {
            UniqueId = uniqueId,
            DateTime = dt,
            Category = category,
            PaymentMode = payment,
            TransactionType = type,
            Amount = amount,
            Description = description,
            MonthShort = dt.ToString("MMM", CultureInfo.InvariantCulture),
            MonthFull = dt.ToString("MMMM, yyyy", CultureInfo.InvariantCulture),
            FormattedDate = dt.ToString("MM/dd/yyyy hh:mm tt", CultureInfo.InvariantCulture)
        };
    }
}

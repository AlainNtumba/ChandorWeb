using System.Globalization;

namespace ChandorAdmin.ViewModels;

/// <summary>
/// Finance dashboard presentation model. Replace sample factories with API-backed loading when services are ready.
/// </summary>
public sealed class DashboardViewModel
{
    public required FinanceSummary Summary { get; init; }
    public required ReportingPeriod Period { get; init; }
    /// <summary>Always six monthly points ending in the calendar month of <see cref="ReportingPeriod.End"/>.</summary>
    public required IReadOnlyList<CashflowSeriesPoint> Cashflow { get; init; }
    /// <summary>Human-readable span for the bar chart (six calendar months ending at period end).</summary>
    public required string BarChartPeriodHint { get; init; }
    public required IReadOnlyList<ExpenseCategorySlice> ExpenseByCategory { get; init; }
    public required IReadOnlyList<IncomeCategorySlice> IncomeByCategory { get; init; }
    public required IReadOnlyList<FinanceActivityItem> RecentActivity { get; init; }

    /// <summary>Palette order matches <see cref="ExpenseByCategory"/> (largest slice first).</summary>
    public static readonly string[] ExpenseCompositionPalette =
    [
        "#61EFCD", "#CDDE1F", "#FEC200", "#CA765A", "#2485FA", "#F57D7D", "#C152D2",
        "#8854D9", "#3D4EB8", "#00BCD7",
    ];

    /// <summary>Palette order matches <see cref="IncomeByCategory"/> (largest slice first).</summary>
    public static readonly string[] IncomeCompositionPalette =
    [
        "#4D80F3", "#3B6FD6", "#6B9DF5", "#2E59C7", "#8FB4F8", "#60A5FA", "#2563EB",
        "#93C5FD", "#1D4ED8",
    ];

    /// <summary>First day of the current calendar month through its last day.</summary>
    public static (DateTime Start, DateTime End) GetCurrentMonthRange()
    {
        var t = DateTime.Today;
        var start = new DateTime(t.Year, t.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return (start, end);
    }

    public static DashboardViewModel CreateSample()
    {
        var (start, end) = GetCurrentMonthRange();
        return CreateForDateRange(start, end);
    }

    /// <summary>
    /// Builds a dashboard for the given reporting window using the in-memory sample dataset.
    /// KPIs and the donut use transactions overlapping the selected range; the bar chart always shows six months ending in the period end month.
    /// </summary>
    public static DashboardViewModel CreateForDateRange(DateTime start, DateTime end)
    {
        var requestedStart = start.Date;
        var requestedEnd = end.Date;
        if (requestedStart > requestedEnd)
        {
            (requestedStart, requestedEnd) = (requestedEnd, requestedStart);
        }

        var includedMonths = FullCashflow
            .Where(m => MonthOverlapsReportingRange(m.MonthStart, requestedStart, requestedEnd))
            .ToList();

        var totalIncome = includedMonths.Sum(c => c.Income);
        var totalExpenses = includedMonths.Sum(c => c.Expenses);

        var recent = SampleActivities
            .Where(a => a.OccurredAt.Date >= requestedStart && a.OccurredAt.Date <= requestedEnd)
            .OrderByDescending(a => a.OccurredAt)
            .Take(8)
            .ToList();

        var fullSampleMonths = FullCashflow.Length;
        var monthFactor = fullSampleMonths == 0 ? 0 : (double)includedMonths.Count / fullSampleMonths;
        var scaledTxn = (int)Math.Round(1284 * monthFactor);
        var transactionCount = Math.Max(recent.Count, scaledTxn);

        var summary = new FinanceSummary(
            TotalIncome: totalIncome,
            TotalExpenses: totalExpenses,
            Balance: totalIncome - totalExpenses,
            TransactionCount: transactionCount);

        var period = new ReportingPeriod(
            Label: FormatPeriodLabel(requestedStart, requestedEnd),
            Start: requestedStart,
            End: requestedEnd);

        var barChart = BuildSixMonthBarChartSeries(requestedEnd);
        var barHint = FormatBarChartPeriodHint(requestedEnd);

        var expenseByCategory = BuildExpenseSlicesForTotal(totalExpenses);
        var incomeByCategory = BuildIncomeSlicesForTotal(totalIncome);

        return new DashboardViewModel
        {
            Summary = summary,
            Period = period,
            Cashflow = barChart,
            BarChartPeriodHint = barHint,
            ExpenseByCategory = expenseByCategory,
            IncomeByCategory = incomeByCategory,
            RecentActivity = recent,
        };
    }

    private static string FormatPeriodLabel(DateTime start, DateTime end) =>
        $"{start:MMM d, yyyy} – {end:MMM d, yyyy}";

    private static string FormatBarChartPeriodHint(DateTime periodEndDate)
    {
        var endMonth = new DateTime(periodEndDate.Year, periodEndDate.Month, 1);
        var startMonth = endMonth.AddMonths(-5);
        var startDisplay = new DateTime(startMonth.Year, startMonth.Month, 1);
        var endDisplay = endMonth.AddMonths(1).AddDays(-1);
        return $"{startDisplay:MMM yyyy} – {endDisplay:MMM yyyy}";
    }

    /// <summary>Six months ending in <paramref name="periodEndDate"/>'s calendar month (inclusive).</summary>
    private static IReadOnlyList<CashflowSeriesPoint> BuildSixMonthBarChartSeries(DateTime periodEndDate)
    {
        var endMonth = new DateTime(periodEndDate.Year, periodEndDate.Month, 1);
        var startMonth = endMonth.AddMonths(-5);
        var spanCalendarYears = startMonth.Year != endMonth.Year;

        var list = new List<CashflowSeriesPoint>(6);
        for (var i = 0; i < 6; i++)
        {
            var monthStart = endMonth.AddMonths(-5 + i);
            var label = spanCalendarYears
                ? monthStart.ToString("MMM ''yy", CultureInfo.InvariantCulture)
                : monthStart.ToString("MMM", CultureInfo.InvariantCulture);

            var row = FullCashflow.FirstOrDefault(m =>
                m.MonthStart.Year == monthStart.Year && m.MonthStart.Month == monthStart.Month);

            list.Add(row.MonthStart != default
                ? new CashflowSeriesPoint(label, row.Income, row.Expenses)
                : new CashflowSeriesPoint(label, 0, 0));
        }

        return list;
    }

    private static bool MonthOverlapsReportingRange(DateTime monthStart, DateTime rangeStart, DateTime rangeEnd)
    {
        var monthEndExclusive = monthStart.AddMonths(1);
        var rangeEndExclusive = rangeEnd.AddDays(1);
        return monthStart < rangeEndExclusive && rangeStart < monthEndExclusive;
    }

    private static IReadOnlyList<ExpenseCategorySlice> BuildExpenseSlicesForTotal(decimal expenseTotal)
    {
        if (expenseTotal <= 0)
        {
            return Array.Empty<ExpenseCategorySlice>();
        }

        var templateTotal = ExpenseCategoryTemplate.Sum(t => t.Amount);
        if (templateTotal <= 0)
        {
            return Array.Empty<ExpenseCategorySlice>();
        }

        return ExpenseCategoryTemplate
            .Select(t => new ExpenseCategorySlice(t.Category, Math.Round(expenseTotal * (t.Amount / templateTotal), 2)))
            .OrderByDescending(s => s.Amount)
            .ToList();
    }

    private static IReadOnlyList<IncomeCategorySlice> BuildIncomeSlicesForTotal(decimal incomeTotal)
    {
        if (incomeTotal <= 0)
        {
            return Array.Empty<IncomeCategorySlice>();
        }

        var templateTotal = IncomeCategoryTemplate.Sum(t => t.Amount);
        if (templateTotal <= 0)
        {
            return Array.Empty<IncomeCategorySlice>();
        }

        return IncomeCategoryTemplate
            .Select(t => new IncomeCategorySlice(t.Category, Math.Round(incomeTotal * (t.Amount / templateTotal), 2)))
            .OrderByDescending(s => s.Amount)
            .ToList();
    }

    private static readonly (DateTime MonthStart, decimal Income, decimal Expenses)[] FullCashflow =
    [
        (new DateTime(2025, 10, 1), 11200, 9800),
        (new DateTime(2025, 11, 1), 11550, 10050),
        (new DateTime(2025, 12, 1), 11700, 10120),
        (new DateTime(2026, 1, 1), 11800, 10200),
        (new DateTime(2026, 2, 1), 12450, 10890),
        (new DateTime(2026, 3, 1), 12100, 11540),
        (new DateTime(2026, 4, 1), 13220, 11980),
        (new DateTime(2026, 5, 1), 12880, 12110),
        (new DateTime(2026, 6, 1), 13940, 12650),
        (new DateTime(2026, 7, 1), 13100, 12200),
        (new DateTime(2026, 8, 1), 13350, 12380),
        (new DateTime(2026, 9, 1), 13050, 12040),
        (new DateTime(2026, 10, 1), 13400, 12420),
        (new DateTime(2026, 11, 1), 12950, 12180),
        (new DateTime(2026, 12, 1), 13600, 12750),
    ];

    private static readonly (string Category, decimal Amount)[] ExpenseCategoryTemplate =
    [
        ("Operations", 8420),
        ("Ministries", 5260),
        ("Facilities", 3890),
        ("Outreach", 2410),
        ("Administration", 1670),
    ];

    private static readonly (string Category, decimal Amount)[] IncomeCategoryTemplate =
    [
        ("Tithes", 5200),
        ("Offerings", 3100),
        ("Online giving", 2800),
        ("Transfers & misc.", 1900),
        ("Special gifts", 1500),
    ];

    private static readonly FinanceActivityItem[] SampleActivities =
    [
        new("Sunday offering", "Sanctuary · consolidated", 4280, new DateTime(2026, 5, 4, 10, 15, 0), false),
        new("Facility maintenance", "Quarterly HVAC service", -1850, new DateTime(2026, 5, 3, 14, 40, 0), true),
        new("Youth retreat deposit", "Lakeview venue", -2200, new DateTime(2026, 5, 2, 9, 05, 0), true),
        new("Online tithes", "Stripe settlement", 3165, new DateTime(2026, 5, 1, 18, 22, 0), false),
        new("Utilities — electric", "City grid May cycle", -740, new DateTime(2026, 5, 1, 8, 00, 0), true),
        new("Mission partnership", "Regional alliance wire", 5100, new DateTime(2026, 4, 29, 11, 48, 0), false),
    ];
}

public sealed record FinanceSummary(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal Balance,
    int TransactionCount);

public sealed record ReportingPeriod(string Label, DateTime Start, DateTime End);

public sealed record CashflowSeriesPoint(string MonthLabel, decimal Income, decimal Expenses);

public sealed record ExpenseCategorySlice(string Category, decimal Amount);

public sealed record IncomeCategorySlice(string Category, decimal Amount);

public sealed record FinanceActivityItem(
    string Title,
    string Subtitle,
    decimal Amount,
    DateTime OccurredAt,
    bool IsExpense);

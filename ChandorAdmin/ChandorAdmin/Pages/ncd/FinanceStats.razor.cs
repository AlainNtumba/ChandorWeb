using ChandorAdmin.Components.Dashboard;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Calendars;

namespace ChandorAdmin.Pages.ncd;

public partial class FinanceStats
{
    [Inject] public IFinanceService FinanceService { get; set; } = null!;

    static readonly DateTime PickerMinDate = new(2015, 1, 1);

    static readonly string[] ExpenseCompositionPalette =
    [
        "#61EFCD", "#CDDE1F", "#FEC200", "#CA765A", "#2485FA", "#F57D7D", "#C152D2",
        "#8854D9", "#3D4EB8", "#00BCD7",
    ];

    static readonly string[] IncomeCompositionPalette =
    [
        "#4D80F3", "#3B6FD6", "#6B9DF5", "#2E59C7", "#8FB4F8", "#60A5FA", "#2563EB",
        "#93C5FD", "#1D4ED8",
    ];

    DateTime _pickerStart;
    DateTime _pickerEnd;
    DateTime _periodStart;
    DateTime _periodEnd;
    string _periodLabel = string.Empty;
    string _barChartPeriodHint = string.Empty;
    FinanceSummaryDto _summary = new();
    IReadOnlyList<CashflowSeriesPointDto> _cashflow = Array.Empty<CashflowSeriesPointDto>();
    IReadOnlyList<ExpenseCategoryDto> _expenseByCategory = Array.Empty<ExpenseCategoryDto>();
    IReadOnlyList<IncomeCategoryDto> _incomeByCategory = Array.Empty<IncomeCategoryDto>();
    IReadOnlyList<FinanceActivityItemDto> _recentActivity = Array.Empty<FinanceActivityItemDto>();
    bool _renderRangePicker;

    DateTime PickerMaxDate => new(DateTime.Today.Year + 5, 12, 31);

    string ChartRefreshKey => $"{_periodEnd:yyyyMM}-{_periodStart:yyyyMMdd}-{_periodEnd:yyyyMMdd}";

    List<Presets> DateRangePresets => DashboardDateRangePresets.StandardThreePresets();

    static string IncomeSeriesFill => "#A16EE5";
    static string ExpenseSeriesFill => "#4472C4";

    protected override async Task OnInitializedAsync()
    {
        var (start, end) = GetCurrentMonthRange();
        _pickerStart = start;
        _pickerEnd = end;
        await LoadDashboardForRangeAsync(start, end);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await Task.Delay(1);
            _renderRangePicker = true;
            StateHasChanged();
        }
    }

    async Task OnDateRangeChangeAsync(RangePickerEventArgs<DateTime> args)
    {
        if (args.StartDate == default || args.EndDate == default)
            return;

        var start = args.StartDate.Date;
        var end = args.EndDate.Date;
        if (start > end)
            return;

        await LoadDashboardForRangeAsync(start, end);
    }

    async Task LoadDashboardForRangeAsync(DateTime start, DateTime end)
    {
        var requestedStart = start.Date;
        var requestedEnd = end.Date;
        if (requestedStart > requestedEnd)
            (requestedStart, requestedEnd) = (requestedEnd, requestedStart);
        var cashFlowEndDate = requestedEnd.AddDays(1);

        try
        {
            var summariesTask = FinanceService.GetFinanceSummariesAsync(requestedStart, requestedEnd, departmentId: null);
            var cashflowTask = FinanceService.GetCashflowSeriesAsync(requestedStart, cashFlowEndDate, departmentId: null);
            var activitiesTask = FinanceService.GetFinanceActivitiesAsync(requestedStart, requestedEnd, departmentId: null);
            var incomeTask = FinanceService.GetIncomeByCategoriesAsync(requestedStart, requestedEnd, departmentId: null);
            var expensesTask = FinanceService.GetExpensesByCategoriesAsync(requestedStart, requestedEnd, departmentId: null);

            await Task.WhenAll(summariesTask, cashflowTask, activitiesTask, incomeTask, expensesTask);

            _summary = summariesTask.Result?.Data?.FirstOrDefault() ?? new FinanceSummaryDto();
            _cashflow = cashflowTask.Result?.Data?.ToList() ?? [];
            _recentActivity = activitiesTask.Result?.Data?.ToList() ?? [];
            _incomeByCategory = incomeTask.Result?.Data?.ToList() ?? [];
            _expenseByCategory = expensesTask.Result?.Data?.ToList() ?? [];

            _periodStart = requestedStart;
            _periodEnd = requestedEnd;
            _periodLabel = FormatPeriodLabel(requestedStart, requestedEnd);
            _barChartPeriodHint = FormatBarChartPeriodHint(requestedEnd);
            _pickerStart = requestedStart;
            _pickerEnd = requestedEnd;
        }
        catch
        {
            _summary = new FinanceSummaryDto();
            _cashflow = [];
            _recentActivity = [];
            _incomeByCategory = [];
            _expenseByCategory = [];
            _periodStart = requestedStart;
            _periodEnd = requestedEnd;
            _periodLabel = FormatPeriodLabel(requestedStart, requestedEnd);
            _barChartPeriodHint = FormatBarChartPeriodHint(requestedEnd);
        }

        await InvokeAsync(StateHasChanged);
    }

    static (DateTime Start, DateTime End) GetCurrentMonthRange()
    {
        var t = DateTime.Today;
        var start = new DateTime(t.Year, t.Month, 1);
        var end = start.AddMonths(1).AddDays(-1);
        return (start, end);
    }

    static string FormatPeriodLabel(DateTime start, DateTime end) =>
        $"{start:MMM d, yyyy} – {end:MMM d, yyyy}";

    static string FormatBarChartPeriodHint(DateTime periodEndDate)
    {
        var endMonth = new DateTime(periodEndDate.Year, periodEndDate.Month, 1);
        var startMonth = endMonth.AddMonths(-5);
        var startDisplay = new DateTime(startMonth.Year, startMonth.Month, 1);
        var endDisplay = endMonth.AddMonths(1).AddDays(-1);
        return $"{startDisplay:MMM yyyy} – {endDisplay:MMM yyyy}";
    }

    static string FormatMoney(decimal value) => value.ToString("C0");
}

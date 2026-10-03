using ChandorAdmin.Components.Dashboard;
using ChandorAdmin.Helpers;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Currency;
using ChandorProject.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Calendars;
using Syncfusion.Blazor.DropDowns;

namespace ChandorAdmin.Pages.ncd;

public partial class FinanceStats
{
    [Inject] public ITransactionService TransactionService { get; set; } = null!;
    [Inject] public ICurrencyService CurrencyService { get; set; } = null!;
    [Inject] public IDepartmentService DepartmentService { get; set; } = null!;

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
    IReadOnlyList<CurrencyDto> _currencies = Array.Empty<CurrencyDto>();
    Guid _selectedCurrencyId;
    Guid? _departmentId;
    bool _renderRangePicker;

    DateTime PickerMaxDate => new(DateTime.Today.Year + 5, 12, 31);

    string ChartRefreshKey => $"{_selectedCurrencyId:N}-{_periodEnd:yyyyMM}-{_periodStart:yyyyMMdd}-{_periodEnd:yyyyMMdd}";

    string? SelectedCurrencySymbol =>
        _currencies.FirstOrDefault(c => c.Id == _selectedCurrencyId)?.Symbol;

    List<Presets> DateRangePresets => DashboardDateRangePresets.StandardThreePresets();

    static string IncomeSeriesFill => "#A16EE5";
    static string ExpenseSeriesFill => "#4472C4";

    protected override async Task OnInitializedAsync()
    {
        var (start, end) = GetCurrentMonthRange();
        _pickerStart = start;
        _pickerEnd = end;
        await LoadLookupsAsync();
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

    async Task LoadLookupsAsync()
    {
        try
        {
            var currenciesTask = CurrencyService.GetAllAsync();
            var keysTask = DepartmentService.GetChurchDepartmentKeysAsync();
            await Task.WhenAll(currenciesTask, keysTask);

            _currencies = currenciesTask.Result?.Data?.ToList() ?? [];
            var keys = keysTask.Result;
            if (keys is { Success: true, Data: not null } && keys.Data.DepartmentId != Guid.Empty)
                _departmentId = keys.Data.DepartmentId;
        }
        catch
        {
            _currencies = [];
            _departmentId = null;
        }

        var selected = FinanceDisplaySupport.SelectDefaultCurrency(_currencies);
        _selectedCurrencyId = selected?.Id ?? Guid.Empty;
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

    async Task OnCurrencyChanged(ChangeEventArgs<Guid, CurrencyDto> args)
    {
        if (args.Value == Guid.Empty)
            return;

        _selectedCurrencyId = args.Value;
        await LoadDashboardForRangeAsync(_pickerStart, _pickerEnd);
    }

    async Task LoadDashboardForRangeAsync(DateTime start, DateTime end)
    {
        var requestedStart = start.Date;
        var requestedEnd = end.Date;
        if (requestedStart > requestedEnd)
            (requestedStart, requestedEnd) = (requestedEnd, requestedStart);
        var cashFlowEndDate = requestedEnd.AddDays(1);
        Guid? currencyId = _selectedCurrencyId == Guid.Empty ? null : _selectedCurrencyId;

        try
        {
            var summariesTask = TransactionService.GetFinanceSummariesAsync(requestedStart, requestedEnd, currencyId, _departmentId);
            var cashflowTask = TransactionService.GetCashflowSeriesAsync(requestedStart, cashFlowEndDate, currencyId, _departmentId);
            var activitiesTask = TransactionService.GetFinanceActivitiesAsync(requestedStart, requestedEnd, _departmentId);
            var incomeTask = TransactionService.GetIncomeByCategoriesAsync(requestedStart, requestedEnd, currencyId, _departmentId);
            var expensesTask = TransactionService.GetExpensesByCategoriesAsync(requestedStart, requestedEnd, currencyId, _departmentId);

            await Task.WhenAll(summariesTask, cashflowTask, activitiesTask, incomeTask, expensesTask);

            _summary = summariesTask.Result?.Data?.FirstOrDefault() ?? new FinanceSummaryDto();
            _cashflow = cashflowTask.Result?.Data?.ToList() ?? [];
            _recentActivity = activitiesTask.Result?.Data?.ToList() ?? [];
            _incomeByCategory = incomeTask.Result?.Data?.ToList() ?? [];
            _expenseByCategory = NormalizeExpenseCategories(
                expensesTask.Result?.Data?.Select(item => new ExpenseCategoryDto
                {
                    Category = item.Category,
                    Amount = item.Amount
                }));

            if (currencyId is null)
            {
                _summary.TotalIncome = 0;
                _summary.TotalExpenses = 0;
                _summary.Balance = 0;
                _cashflow = [];
                _incomeByCategory = [];
                _expenseByCategory = [];
            }

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

    string FormatMoney(decimal value) =>
        _selectedCurrencyId == Guid.Empty
            ? "—"
            : FinanceDisplaySupport.FormatAmount(value, SelectedCurrencySymbol);

    static List<ExpenseCategoryDto> NormalizeExpenseCategories(IEnumerable<ExpenseCategoryDto>? items)
    {
        if (items is null)
            return [];

        return items
            .Select(item => new ExpenseCategoryDto
            {
                Category = string.IsNullOrWhiteSpace(item.Category) ? "Non catégorisé" : item.Category.Trim(),
                Amount = item.Amount
            })
            .GroupBy(item => item.Category, StringComparer.OrdinalIgnoreCase)
            .Select(group => new ExpenseCategoryDto
            {
                Category = group.Key,
                Amount = group.Sum(item => item.Amount)
            })
            .ToList();
    }
}

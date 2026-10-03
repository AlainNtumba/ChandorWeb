using System.Globalization;
using ChandorAdmin.Helpers;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Currency;
using ChandorProject.Shared.DTOs.Transaction;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace ChandorAdmin.Components.Finance.Transactions;

public partial class TransactionGridPanel
{
    [Inject] public ITransactionService TransactionService { get; set; } = null!;
    [Inject] public ICurrencyService CurrencyService { get; set; } = null!;
    [Inject] public IDepartmentService DepartmentService { get; set; } = null!;

    public SfGrid<TransactionView>? TransactGridRef { get; set; }
    public TransactionEditorDialog? DialogRef { get; set; }
    public TransactionFilterSidebar? FilterRef { get; set; }

    readonly ValidationRules _rules = new() { Required = true };
    public IEnumerable<TransactionView> GridData { get; private set; } = Array.Empty<TransactionView>();
    public IReadOnlyList<TransactionView> AllTransactions { get; private set; } = Array.Empty<TransactionView>();
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public Guid? CurrencyId { get; private set; }

    IReadOnlyList<CurrencyDto> _currencies = Array.Empty<CurrencyDto>();
    Guid? _departmentId;
    bool _departmentKeysLoaded;

    public List<ItemModel> Toolbaritems { get; } =
    [
        new ItemModel { Text = "Edit", PrefixIcon = "e-edit e-icons", TooltipText = "Edit", Id = "Edit", Disabled = true },
        new ItemModel { Text = "Delete", PrefixIcon = "e-delete e-icons", TooltipText = "Delete", Id = "Delete", Disabled = true },
        new ItemModel { Text = "Excel Export", PrefixIcon = "e-excelexport e-icons", TooltipText = "ExcelExport", Id = "Grid_excelexport" }
    ];

    bool _renderGrid;

    public static (DateTime Start, DateTime End) GetCalendarMonthBounds(DateTime reference)
    {
        var day = reference.Date;
        var start = new DateTime(day.Year, day.Month, 1, 0, 0, 0);
        var end = new DateTime(day.Year, day.Month, DateTime.DaysInMonth(day.Year, day.Month), 23, 59, 59);
        return (start, end);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InvokeAsync(async () =>
            {
                await Task.Delay(1);
                _renderGrid = true;
                StateHasChanged();
            });
        }
    }

    public async Task LoadTransactionsAsync(DateTime start, DateTime end, Guid? currencyId = null)
    {
        StartDate = start;
        EndDate = end;
        CurrencyId = currencyId;

        await EnsureLookupsLoadedAsync();

        try
        {
            var response = await TransactionService.GetTransactionsAsync(start, end, currencyId, _departmentId);
            AllTransactions = response?.Data?.ToList() ?? [];
        }
        catch
        {
            AllTransactions = [];
        }

        GridRefresh(FilterRef?.RefreshData() ?? AllTransactions.OrderByDescending(t => t.TransactionDate).ToList());
    }

    async Task EnsureLookupsLoadedAsync()
    {
        if (_currencies.Count == 0)
        {
            try
            {
                var response = await CurrencyService.GetAllAsync();
                _currencies = response?.Data?.ToList() ?? [];
            }
            catch
            {
                _currencies = [];
            }
        }

        if (_departmentKeysLoaded)
            return;

        try
        {
            var keys = await DepartmentService.GetChurchDepartmentKeysAsync();
            if (keys is { Success: true, Data: not null } && keys.Data.DepartmentId != Guid.Empty)
                _departmentId = keys.Data.DepartmentId;
        }
        catch
        {
            _departmentId = null;
        }
        finally
        {
            _departmentKeysLoaded = true;
        }
    }

    public async Task RefreshCurrenciesAsync()
    {
        _currencies = [];
        await EnsureLookupsLoadedAsync();
        StateHasChanged();
    }

    public string FormatRowAmount(TransactionView row) =>
        FinanceDisplaySupport.FormatAmount(row.Amount, ResolveSymbol(row.CurrencyId));

    public string CashflowCss(TransactionView row) =>
        FinanceDisplaySupport.CssClass(row.TransactionType);

    string? ResolveSymbol(Guid currencyId) =>
        _currencies.FirstOrDefault(c => c.Id == currencyId)?.Symbol;

    Task OnRowSelectChanged(RowSelectEventArgs<TransactionView> _) => RefreshToolbarFromSelectionAsync();

    Task OnRowDeselectChanged(RowDeselectEventArgs<TransactionView> _) => RefreshToolbarFromSelectionAsync();

    public async Task RefreshToolbarFromSelectionAsync()
    {
        if (TransactGridRef is null)
            return;

        var selected = await TransactGridRef.GetSelectedRecordsAsync();
        var count = selected.Count;
        if (count > 1)
        {
            Toolbaritems[0].Disabled = true;
            Toolbaritems[1].Disabled = false;
        }
        else if (count == 0)
        {
            Toolbaritems[0].Disabled = true;
            Toolbaritems[1].Disabled = true;
        }
        else
        {
            Toolbaritems[0].Disabled = false;
            Toolbaritems[1].Disabled = false;
        }
    }

    public async Task ToolbarClickHandler(ClickEventArgs args)
    {
        if (TransactGridRef is null)
            return;

        if (string.Equals(args.Item.Id, "Grid_excelexport", StringComparison.OrdinalIgnoreCase)
            || args.Item.Text?.Contains("Excel", StringComparison.OrdinalIgnoreCase) == true)
        {
            await TransactGridRef.ExportToExcelAsync();
            return;
        }

        var selectedRecords = await TransactGridRef.GetSelectedRecordsAsync();

        if (args.Item.Id == "Edit" && DialogRef is not null)
        {
            if (selectedRecords.Count == 1)
                await DialogRef.ShowEditDialog(selectedRecords[0]);
        }
        else if (args.Item.Id == "Delete" && DialogRef is not null)
        {
            await DialogRef.ShowAlertDialog(selectedRecords.ToList());
        }
    }

    public async Task SearchAsync(string? value) => await (TransactGridRef?.SearchAsync(value ?? string.Empty) ?? Task.CompletedTask);

    public async Task ReloadAsync()
    {
        if (StartDate == default || EndDate == default)
        {
            (StartDate, EndDate) = GetCalendarMonthBounds(DateTime.Today);
        }

        await LoadTransactionsAsync(StartDate, EndDate, CurrencyId);
        FilterRef?.RebuildCategoryList();
        FilterRef?.UpdateGrid();
        UpdateTotalBalance();
    }

    public void GridRefresh(IEnumerable<TransactionView> rows)
    {
        GridData = rows;
        StateHasChanged();
    }

    public void UpdateTotalBalance()
    {
        var commonData = GridData.ToList();
        if (commonData.Count == 0)
            return;

        var currencyIds = commonData.Select(s => s.CurrencyId).Distinct().ToList();
        if (currencyIds.Count != 1)
            return;

        var incomeSum = commonData.Where(s => FinanceDisplaySupport.IsIncome(s.TransactionType)).Sum(s => s.Amount);
        var expenseSum = commonData.Where(s => FinanceDisplaySupport.IsExpense(s.TransactionType)).Sum(s => s.Amount);
        _ = FormatBalance(incomeSum, expenseSum, ResolveSymbol(currencyIds[0]));
    }

    public static string FormatBalance(decimal incomeSum, decimal expenseSum, string? symbol = null)
    {
        var n = incomeSum - expenseSum;
        var sign = n < 0 ? "-" : "";
        n = Math.Abs(n);
        var body = n.ToString("N0", CultureInfo.InvariantCulture);
        return string.IsNullOrWhiteSpace(symbol)
            ? sign + "$" + body
            : sign + symbol + body;
    }

    public void Dispose() => TransactGridRef = null;
}

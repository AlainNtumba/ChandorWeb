using System.Globalization;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Finance;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace ChandorAdmin.Components.Finance.Transactions;

public partial class TransactionGridPanel
{
    [Inject] public IFinanceService FinanceService { get; set; } = null!;

    public SfGrid<TransactionDto>? TransactGridRef { get; set; }
    public TransactionEditorDialog? DialogRef { get; set; }
    public TransactionFilterSidebar? FilterRef { get; set; }

    readonly ValidationRules _rules = new() { Required = true };
    public IEnumerable<TransactionDto> GridData { get; private set; } = Array.Empty<TransactionDto>();
    public IReadOnlyList<TransactionDto> AllTransactions { get; private set; } = Array.Empty<TransactionDto>();
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }

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

    public async Task LoadTransactionsAsync(DateTime start, DateTime end)
    {
        StartDate = start;
        EndDate = end;

        try
        {
            var response = await FinanceService.GetChurchTransactionsAsync(start, end);
            AllTransactions = response?.Data?.ToList() ?? [];
        }
        catch
        {
            AllTransactions = [];
        }

        GridRefresh(FilterRef?.RefreshData() ?? AllTransactions.OrderByDescending(t => t.TransactionDate).ToList());
    }

    Task OnRowSelectChanged(RowSelectEventArgs<TransactionDto> _) => RefreshToolbarFromSelectionAsync();

    Task OnRowDeselectChanged(RowDeselectEventArgs<TransactionDto> _) => RefreshToolbarFromSelectionAsync();

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

        await LoadTransactionsAsync(StartDate, EndDate);
        FilterRef?.RebuildCategoryList();
        FilterRef?.UpdateGrid();
        UpdateTotalBalance();
    }

    public void GridRefresh(IEnumerable<TransactionDto> rows)
    {
        GridData = rows;
        StateHasChanged();
    }

    public void UpdateTotalBalance()
    {
        var commonData = GridData.ToList();
        var incomeSum = commonData.Where(s => s.TransactionType == "Income").Sum(s => s.Amount);
        var expenseSum = commonData.Where(s => s.TransactionType == "Expense").Sum(s => s.Amount);
        _ = FormatBalance(incomeSum, expenseSum);
    }

    public static string FormatBalance(decimal incomeSum, decimal expenseSum)
    {
        var n = incomeSum - expenseSum;
        var sign = n < 0 ? "-" : "";
        n = Math.Abs(n);
        return sign + "$" + n.ToString("N0", CultureInfo.InvariantCulture);
    }

    public void Dispose() => TransactGridRef = null;
}

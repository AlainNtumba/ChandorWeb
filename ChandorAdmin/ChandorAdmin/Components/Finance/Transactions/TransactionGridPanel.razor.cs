using ChandorAdmin.Models.Finance;
using ChandorAdmin.Services.Finance;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace ChandorAdmin.Components.Finance.Transactions;

public partial class TransactionGridPanel
{
    public SfGrid<FinanceTransaction>? TransactGridRef { get; set; }
    public TransactionEditorDialog? DialogRef { get; set; }
    public TransactionFilterSidebar? FilterRef { get; set; }

    readonly ValidationRules _rules = new() { Required = true };
    public IEnumerable<FinanceTransaction> GridData { get; private set; } = Array.Empty<FinanceTransaction>();

    public List<ItemModel> Toolbaritems { get; } =
    [
        new ItemModel { Text = "Edit", PrefixIcon = "e-edit e-icons", TooltipText = "Edit", Id = "Edit", Disabled = true },
    new ItemModel { Text = "Delete", PrefixIcon = "e-delete e-icons", TooltipText = "Delete", Id = "Delete", Disabled = true },
    new ItemModel { Text = "Excel Export", PrefixIcon = "e-excelexport e-icons", TooltipText = "ExcelExport", Id = "Grid_excelexport" }
    ];

    bool _renderGrid;

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        UpdateTotalBalance();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            await InvokeAsync(async () =>
            {
                await Task.Delay(1);
                _renderGrid = true;
                StateHasChanged();
            });
        }
    }

    Task OnRowSelectChanged(RowSelectEventArgs<FinanceTransaction> _) => RefreshToolbarFromSelectionAsync();

    Task OnRowDeselectChanged(RowDeselectEventArgs<FinanceTransaction> _) => RefreshToolbarFromSelectionAsync();

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

    public void AddRecord(FinanceTransaction row)
    {
        Data.Transactions.Add(row);
        GridRefresh(FilterRef?.RefreshData() ?? Enumerable.Empty<FinanceTransaction>());
    }

    public void UpdateRecord(FinanceTransaction row)
    {
        foreach (var data in Data.Transactions.Where(s => s.UniqueId == row.UniqueId))
        {
            data.UniqueId = row.UniqueId;
            data.TransactionType = row.TransactionType;
            data.DateTime = row.DateTime;
            data.Category = row.Category;
            data.PaymentMode = row.PaymentMode;
            data.Description = row.Description;
            data.Amount = row.Amount;
            data.MonthShort = row.MonthShort;
            data.MonthFull = row.MonthFull;
            data.FormattedDate = row.FormattedDate;
        }

        GridRefresh(FilterRef?.RefreshData() ?? Enumerable.Empty<FinanceTransaction>());
    }

    public void RemoveRecord(List<string> uniqueIds)
    {
        foreach (var id in uniqueIds)
        {
            var index = Data.Transactions.FindIndex(s => s.UniqueId == id);
            if (index > -1)
                Data.Transactions.RemoveAt(index);
        }

        GridRefresh(FilterRef?.RefreshData() ?? Enumerable.Empty<FinanceTransaction>());
    }

    public void GridRefresh(IEnumerable<FinanceTransaction> rows)
    {
        GridData = rows;
        StateHasChanged();
    }

    public void UpdateTotalBalance()
    {
        var commonData = Data.CurrentView is null
            ? Data.Transactions.Where(s => s.DateTime >= Data.StartDate && s.DateTime <= Data.EndDate)
            : Data.CurrentView;

        var incomeSum = commonData.Where(s => s.TransactionType == "Income").Sum(s => s.Amount);
        var expenseSum = commonData.Where(s => s.TransactionType == "Expense").Sum(s => s.Amount);
        Data.UpdateCurrentBalance(FinanceTransactionDataService.FormatBalance(incomeSum, expenseSum));
        Data.NotifyChanged();
    }

    public void Dispose() => TransactGridRef = null;
}
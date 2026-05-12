using ChandorAdmin.Configuration;
using ChandorAdmin.Models.Finance;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Components.Finance.Transactions;

public partial class TransactionEditorDialog
{
    public TransactionGridPanel? ContentRef { get; set; }

    SfDialog? _transactionDialog;
    CustomFormValidator? _customFormValidator;

    FinanceTransactionEditorModel _editModel = new();

    List<FinanceCategoryOption> _categoryDataSource = new();
    bool _isAdd;
    DateTime _editorMinDate => new(2019, 6, 1, 0, 0, 0);

    DateTime _editorMaxDate => new(DateTime.Today.Year + 5, 12, 31, 23, 59, 59);
    FinanceTransaction? _selectedRecord;
    string _buttonContent = "Add";
    string _dialogHeader = "";
    bool _createNewDialog;
    List<FinanceTransaction> _gridSelectedRecords = new();

    protected override void OnInitialized()
    {
        ResetCategoryListForExpense();
        _editModel.Description = "";
        _editModel.DateValue = Data.EndDate;
        _editModel.TimeValue = Data.EndDate;
        _editModel.Category = _categoryDataSource.Count > 0 ? _categoryDataSource[0].Category : null;
    }

    void ResetCategoryListForExpense()
    {
        _categoryDataSource = new List<FinanceCategoryOption>(Data.CategoryExpenseData);
    }

    bool _pendingDialogShow;

    public async Task ShowAddDialog()
    {
        _isAdd = true;
        var hadShell = _createNewDialog;
        UpdateAddDialog();
        if (!hadShell)
        {
            _createNewDialog = true;
            _pendingDialogShow = true;
            StateHasChanged();
            return;
        }

        StateHasChanged();
        await Task.Yield();
        if (_transactionDialog is not null)
            await _transactionDialog.ShowAsync();
    }

    void UpdateAddDialog()
    {
        _dialogHeader = "New Transaction";
        _buttonContent = "Add";
        var tick = DateTime.UtcNow.Ticks.ToString();
        _editModel.UniqueId = "T" + (tick.Length >= 6 ? tick[^6..] : tick);
        _editModel.Amount = 0;
        _editModel.DateValue = _editModel.TimeValue = Data.EndDate;
        _editModel.Description = "";
        _editModel.TransactionType = "Expense";
        _editModel.PaymentMode = "Cash";
        ResetCategoryListForExpense();
        _editModel.Category = _categoryDataSource.Count > 0 ? _categoryDataSource[0].Category : null;
    }

    public async Task ShowEditDialog(FinanceTransaction selected)
    {
        _isAdd = false;
        _selectedRecord = selected;
        var hadShell = _createNewDialog;
        UpdateEditDialog();
        if (!hadShell)
        {
            _createNewDialog = true;
            _pendingDialogShow = true;
            StateHasChanged();
            return;
        }

        StateHasChanged();
        await Task.Yield();
        if (_transactionDialog is not null)
            await _transactionDialog.ShowAsync();
    }

    void UpdateEditDialog()
    {
        _dialogHeader = "Edit Transaction";
        _buttonContent = "Save";
        if (_selectedRecord is null)
            return;

        _editModel.UniqueId = _selectedRecord.UniqueId;
        if (_selectedRecord.TransactionType == "Income")
        {
            _editModel.TransactionType = "Income";
            _categoryDataSource = new List<FinanceCategoryOption>(Data.CategoryIncomeData);
        }
        else if (_selectedRecord.TransactionType == "Expense")
        {
            _editModel.TransactionType = "Expense";
            ResetCategoryListForExpense();
        }

        _editModel.DateValue = _editModel.TimeValue = _selectedRecord.DateTime;
        _editModel.PaymentMode = _selectedRecord.PaymentMode;
        _editModel.Description = _selectedRecord.Description;
        _editModel.Amount = _selectedRecord.Amount;
        _editModel.Category = _selectedRecord.Category;
    }

    void OnCreate()
    {
        if (_isAdd)
            UpdateAddDialog();
        else
            UpdateEditDialog();
    }

    public async Task ShowAlertDialog(List<FinanceTransaction> selectedRecords)
    {
        _gridSelectedRecords = selectedRecords;
        var confirm = await DialogService.ConfirmAsync(
            "Are you sure you want to delete the selected transaction(s)?",
            "Delete",
            new DialogOptions
            {
                ShowCloseIcon = true,
                Width = "40%",
                PrimaryButtonOptions = new DialogButtonOptions { Content = "Yes" },
                CancelButtonOptions = new DialogButtonOptions { Content = "No" }
            });

        if (!confirm || ContentRef is null)
            return;

        await OnClickDeleteAsync();
        ContentRef.Toolbaritems[0].Disabled = true;
        ContentRef.Toolbaritems[1].Disabled = true;
        StateHasChanged();
    }

    void OnTransactionTypeChanged(Syncfusion.Blazor.Buttons.ChangeArgs<string> args)
    {
        var value = args.Value ?? "Expense";
        _categoryDataSource = value == "Income"
            ? new List<FinanceCategoryOption>(Data.CategoryIncomeData)
            : new List<FinanceCategoryOption>(Data.CategoryExpenseData);

        _editModel.Category = _categoryDataSource.Count > 0 ? _categoryDataSource[0].Category : null;
    }

    async Task OnValidSubmitAsync()
    {
        _customFormValidator?.ClearFormErrors();

        if (ContentRef is null || _editModel.Amount is null)
            return;

        var dtBase = _editModel.DateValue ?? Data.EndDate;
        var tm = _editModel.TimeValue ?? Data.EndDate;
        var combined = dtBase.Date.Add(tm.TimeOfDay);

        var row = new FinanceTransaction
        {
            UniqueId = _editModel.UniqueId,
            TransactionType = _editModel.TransactionType,
            DateTime = combined,
            Category = _editModel.Category ?? "",
            PaymentMode = _editModel.PaymentMode,
            Description = _editModel.Description ?? "",
            Amount = (int)_editModel.Amount.Value,
            MonthShort = combined.ToString("MMM"),
            MonthFull = combined.ToString("MMMM, yyyy"),
            FormattedDate = combined.ToString("MM/dd/yyyy hh:mm tt")
        };

        if (_isAdd)
            ContentRef.AddRecord(row);
        else
            ContentRef.UpdateRecord(row);

        if (_transactionDialog is not null)
            await _transactionDialog.HideAsync();

        ContentRef.UpdateTotalBalance();
    }

    async Task OnClickDeleteAsync()
    {
        if (ContentRef is null)
            return;

        var ids = _gridSelectedRecords.Select(s => s.UniqueId).ToList();
        ContentRef.RemoveRecord(ids);
        ContentRef.UpdateTotalBalance();
        await ContentRef.RefreshToolbarFromSelectionAsync();
    }

    async Task OnClickCancel()
    {
        _customFormValidator?.ClearFormErrors();
        if (_createNewDialog && _transactionDialog is not null)
            await _transactionDialog.HideAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_pendingDialogShow && _transactionDialog is not null)
        {
            _pendingDialogShow = false;
            await _transactionDialog.ShowAsync();
        }
    }

    public void Dispose()
    {
        _transactionDialog = null;
        _customFormValidator = null;
    }
}
using ChandorAdmin.Components.Finance.Management;
using ChandorAdmin.Components.GlobalNotification;
using ChandorAdmin.Configuration;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.Finance;
using ChandorProject.Shared.DTOs.Account;
using ChandorProject.Shared.DTOs.Expenses;
using ChandorProject.Shared.DTOs.ExpensesType;
using ChandorProject.Shared.DTOs.Finance;
using ChandorProject.Shared.DTOs.Income;
using ChandorProject.Shared.DTOs.IncomeType;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Components.Finance.Transactions;

public partial class TransactionEditorDialog
{
    [Inject] public IIncomeTypeService IncomeTypeService { get; set; } = null!;
    [Inject] public IExpensesTypeService ExpensesTypeService { get; set; } = null!;
    [Inject] public IAccountService AccountService { get; set; } = null!;
    [Inject] public IIncomeService IncomeService { get; set; } = null!;
    [Inject] public IExpensesService ExpensesService { get; set; } = null!;
    [Inject] public IDepartmentService DepartmentService { get; set; } = null!;

    public TransactionGridPanel? ContentRef { get; set; }

    SfDialog? _transactionDialog;
    NotificationDialog? _notificationRef;
    CustomFormValidator? _customFormValidator;

    FinanceTransactionEditorModel _editModel = new();

    List<TransactionCategoryOption> _categoryDataSource = new();
    List<AccountDto> _accounts = new();
    List<IncomeTypeDto> _incomeTypes = new();
    List<ExpensesTypeDto> _expenseTypes = new();
    List<TransType> _transactionTypes = new()
    {
        new TransType { Value = "Income", Name = "Revenu" },
        new TransType { Value = "Expense", Name = "Dépense" }
    };

    bool _isAdd;
    bool _saving;
    bool _lookupsLoaded;
    DateTime _editorMinDate => new(2015, 1, 1, 0, 0, 0);
    DateTime _editorMaxDate => new(DateTime.Today.Year + 5, 12, 31, 23, 59, 59);
    TransactionDto? _selectedRecord;
    string _buttonContent = "Ajouter";
    string _dialogHeader = "";
    string _formId = "transaction";
    bool _createNewDialog;
    bool _editIncome, _editExpense = true;
    List<TransactionDto> _gridSelectedRecords = new();

    protected override async Task OnInitializedAsync()
    {
        await EnsureLookupsLoadedAsync();
        ResetCategoryListForExpense();
        _editModel.TransactionNote = "";
        var (_, end) = TransactionGridPanel.GetCalendarMonthBounds(DateTime.Today);
        _editModel.DateValue = end;
        _editModel.TimeValue = end;
        _editModel.CategoryId = _categoryDataSource.FirstOrDefault()?.Id;
        _editModel.AccountId = _accounts.FirstOrDefault()?.Id;
    }

    async Task EnsureLookupsLoadedAsync()
    {
        if (_lookupsLoaded)
            return;

        try
        {
            var incomeTypesTask = IncomeTypeService.GetAllIncomeTypesAsync();
            var expenseTypesTask = ExpensesTypeService.GetAllExpensesTypesAsync();
            var accountsTask = AccountService.GetAllAccountsAsync();
            await Task.WhenAll(incomeTypesTask, expenseTypesTask, accountsTask);

            _incomeTypes = incomeTypesTask.Result?.Data?.ToList() ?? [];
            _expenseTypes = expenseTypesTask.Result?.Data?.ToList() ?? [];
            _accounts = accountsTask.Result?.Data?.ToList() ?? [];
            _lookupsLoaded = true;
        }
        catch
        {
            _incomeTypes = [];
            _expenseTypes = [];
            _accounts = [];
        }
    }

    void ResetCategoryListForExpense()
    {
        _categoryDataSource = _expenseTypes
            .Select(t => new TransactionCategoryOption { Id = t.Id, Name = t.Name })
            .ToList();
    }

    bool _pendingDialogShow;

    public async Task ShowAddDialog()
    {
        await EnsureLookupsLoadedAsync();
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
        _editExpense = true;
        _editIncome = true;
        _dialogHeader = "Nouvelle opération";
        _buttonContent = "Ajouter";
        _editModel.Id = Guid.Empty;
        _editModel.Amount = 0;
        var (_, end) = TransactionGridPanel.GetCalendarMonthBounds(DateTime.Today);
        _editModel.DateValue = _editModel.TimeValue = end;
        _editModel.TransactionNote = "";
        _editModel.TransactionType = "Expense";
        ResetCategoryListForExpense();
        _editModel.CategoryId = _categoryDataSource.FirstOrDefault()?.Id;
        _editModel.AccountId = _accounts.FirstOrDefault()?.Id;
        _editModel.DepartmentId = Guid.Empty;
        _editModel.DepartmentTeamId = Guid.Empty;
    }

    public async Task ShowEditDialog(TransactionDto selected)
    {
        await EnsureLookupsLoadedAsync();
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
        _dialogHeader = "Modifier l'opération";
        _buttonContent = "Sauvegarder";
        if (_selectedRecord is null)
            return;

        _editModel.Id = _selectedRecord.Id;
        _editModel.TransactionType = _selectedRecord.TransactionType;
        if (_selectedRecord.TransactionType == "Income")
        {
            _editIncome = true;
            _editExpense = false;
            _categoryDataSource = _incomeTypes
                .Select(t => new TransactionCategoryOption { Id = t.Id, Name = t.Name })
                .ToList();
        }
        else
        {
            _editIncome = false;
            _editExpense = true;
            ResetCategoryListForExpense();
        }

        _editModel.DateValue = _editModel.TimeValue = _selectedRecord.TransactionDate;
        _editModel.TransactionNote = _selectedRecord.TransactionNote;
        _editModel.Amount = _selectedRecord.Amount;
        _editModel.DepartmentId = _selectedRecord.DepartmentId;

        _editModel.CategoryId = _categoryDataSource
            .FirstOrDefault(c => string.Equals(c.Name, _selectedRecord.TransactionCategory, StringComparison.OrdinalIgnoreCase))?.Id
            ?? _categoryDataSource.FirstOrDefault()?.Id;

        _editModel.AccountId = _accounts
            .FirstOrDefault(a => string.Equals(a.AccountName, _selectedRecord.AccountName, StringComparison.OrdinalIgnoreCase))?.Id
            ?? _accounts.FirstOrDefault()?.Id;
    }

    void OnCreate()
    {
        if (_isAdd)
            UpdateAddDialog();
        else
            UpdateEditDialog();
    }

    public async Task ShowAlertDialog(List<TransactionDto> selectedRecords)
    {
        _gridSelectedRecords = selectedRecords;

        if (_notificationRef is null)
            return;

        _notificationRef.NotificationHeader = "Warning";
        _notificationRef.NotificationType = "Warning";
        _notificationRef.NotificationMessage = selectedRecords.Count > 1
            ? "Êtes-vous sûr de vouloir supprimer la transaction sélectionnée?"
            : "Êtes-vous sûr de vouloir supprimer la transaction sélectionnée?";
        var confirm = await _notificationRef.ShowAlertDialog();

        if (!confirm || ContentRef is null)
            return;

        await OnClickDeleteAsync();
        ContentRef.Toolbaritems[0].Disabled = true;
        ContentRef.Toolbaritems[1].Disabled = true;
        StateHasChanged();
    }

    void OnTransactionTypeChanged()
    {
        var value = _editModel.TransactionType ?? "Expense";
        _categoryDataSource = value == "Income"
            ? _incomeTypes.Select(t => new TransactionCategoryOption { Id = t.Id, Name = t.Name }).ToList()
            : _expenseTypes.Select(t => new TransactionCategoryOption { Id = t.Id, Name = t.Name }).ToList();

        _editModel.CategoryId = _categoryDataSource.FirstOrDefault()?.Id;
    }

    async Task OnValidSubmitAsync()
    {
        _customFormValidator?.ClearFormErrors();

        if (ContentRef is null || _editModel.Amount is null || _editModel.CategoryId is null || _editModel.AccountId is null)
            return;

        var dtBase = _editModel.DateValue ?? DateTime.Today;
        var tm = _editModel.TimeValue ?? DateTime.Today;
        var combined = dtBase.Date.Add(tm.TimeOfDay);

        _saving = true;
        try
        {
            if (_isAdd)
            {
                var request = new NewChurchTransactionDto
                {
                    TransactionDate = combined,
                    TransactionType = _editModel.TransactionType,
                    TransactionNote = _editModel.TransactionNote ?? string.Empty,
                    Amount = _editModel.Amount.Value,
                    CategoryId = _editModel.CategoryId.Value,
                    AccountId = _editModel.AccountId.Value
                };

                var response = await FinanceService.InsertChurchTransactionAsync(request);
                if (response is not { Success: true })
                {
                    await NotifyTransactionResultAsync(false, _isAdd, response?.Message);
                    return;
                }

                await NotifyTransactionResultAsync(true, _isAdd);
            }
            else
            {
                var saved = await SaveEditAsync(combined);
                if (!saved)
                    return;

                await NotifyTransactionResultAsync(true, _isAdd);
            }

            if (_transactionDialog is not null)
                await _transactionDialog.HideAsync();

            await ContentRef.ReloadAsync();
            ContentRef.UpdateTotalBalance();
        }
        catch
        {
            await NotifyTransactionResultAsync(false, _isAdd, "Impossible d'enregistrer l'opération. Veuillez réessayer.");
        }
        finally
        {
            _saving = false;
        }
    }

    async Task<bool> SaveEditAsync(DateTime transactionDate)
    {
        if (_selectedRecord is null || _editModel.CategoryId is null || _editModel.AccountId is null || _editModel.Amount is null)
            return false;

        if (_selectedRecord.TransactionType == "Income")
        {
            var existing = await IncomeService.GetIncomeByIdAsync(_selectedRecord.Id);
            if (existing is not { Success: true, Data: not null })
            {
                await NotifyTransactionResultAsync(
                    false,
                    false,
                    FinanceManagementGridSupport.FormatApiErrorMessage(existing, "La transaction n'a pas pu être chargée pour modification."));
                return false;
            }

            var (departmentId, departmentTeamId) = await ResolveDepartmentKeysForUpdateAsync(
                existing.Data.DepartmentId,
                existing.Data.DepartmentTeamId,
                _selectedRecord.DepartmentId);
            if (departmentId == Guid.Empty || departmentTeamId == Guid.Empty)
            {
                await NotifyTransactionResultAsync(false, false, "Impossible de résoudre le département pour cette transaction.");
                return false;
            }

            var income = new IncomeDto
            {
                Id = _selectedRecord.Id,
                IncomeDate = transactionDate,
                IncomeTypeId = _editModel.CategoryId.Value,
                AccountId = _editModel.AccountId.Value,
                Amount = _editModel.Amount.Value,
                Note = _editModel.TransactionNote ?? string.Empty,
                DepartmentId = departmentId,
                DepartmentTeamId = departmentTeamId
            };

            var response = await IncomeService.UpdateIncomeAsync(income);
            if (response is not { Success: true })
            {
                await NotifyTransactionResultAsync(
                    false,
                    false,
                    FinanceManagementGridSupport.FormatApiErrorMessage(response, "Impossible de mettre à jour la transaction."));
                return false;
            }
        }
        else
        {
            var existing = await ExpensesService.GetExpensesByIdAsync(_selectedRecord.Id);
            if (existing is not { Success: true, Data: not null })
            {
                await NotifyTransactionResultAsync(
                    false,
                    false,
                    FinanceManagementGridSupport.FormatApiErrorMessage(existing, "La transaction n'a pas pu être chargée pour modification."));
                return false;
            }

            var (departmentId, departmentTeamId) = await ResolveDepartmentKeysForUpdateAsync(
                existing.Data.DepartmentId,
                existing.Data.DepartmentTeamId,
                _selectedRecord.DepartmentId);
            if (departmentId == Guid.Empty || departmentTeamId == Guid.Empty)
            {
                await NotifyTransactionResultAsync(false, false, "Impossible de résoudre le département pour cette transaction.");
                return false;
            }

            var expense = new ExpensesDto
            {
                Id = _selectedRecord.Id,
                ExpenseDate = transactionDate,
                ExpensesTypeId = _editModel.CategoryId.Value,
                AccountId = _editModel.AccountId.Value,
                Amount = _editModel.Amount.Value,
                Note = _editModel.TransactionNote ?? string.Empty,
                DepartmentId = departmentId,
                DepartmentTeamId = departmentTeamId
            };

            var response = await ExpensesService.UpdateExpensesAsync(expense);
            if (response is not { Success: true })
            {
                await NotifyTransactionResultAsync(
                    false,
                    false,
                    FinanceManagementGridSupport.FormatApiErrorMessage(response, "Impossible de mettre à jour la transaction."));
                return false;
            }
        }

        return true;
    }

    async Task<(Guid DepartmentId, Guid DepartmentTeamId)> ResolveDepartmentKeysForUpdateAsync(
        Guid existingDepartmentId,
        Guid existingDepartmentTeamId,
        Guid selectedDepartmentId)
    {
        var departmentId = existingDepartmentId != Guid.Empty
            ? existingDepartmentId
            : selectedDepartmentId;
        var departmentTeamId = existingDepartmentTeamId;

        if (departmentId != Guid.Empty && departmentTeamId != Guid.Empty)
            return (departmentId, departmentTeamId);

        var keys = await DepartmentService.GetChurchDepartmentKeysAsync();
        if (keys is not { Success: true, Data: not null })
            return (departmentId, departmentTeamId);

        if (departmentId == Guid.Empty)
            departmentId = keys.Data.DepartmentId;
        if (departmentTeamId == Guid.Empty)
            departmentTeamId = keys.Data.DepartmentTeamId;

        return (departmentId, departmentTeamId);
    }

    async Task NotifyTransactionResultAsync(bool success, bool isAdd, string? errorMessage = null)
    {
        if (_notificationRef is null)
            return;

        var status = success ? "Success" : "Error";
        var action = isAdd ? "l'ajout" : "la mise à jour";
        var responseMessage = success
            ? isAdd
                ? "La transaction a été ajoutée avec succès."
                : "La transaction a été mise à jour avec succès."
            : string.IsNullOrWhiteSpace(errorMessage)
                ? $"Une erreur s'est produite pendant {action} de la transaction."
                : $"Une erreur s'est produite pendant {action} de la transaction.\nError: {errorMessage}";

        await _notificationRef.Notify(responseMessage, status, status);
    }

    async Task OnClickDeleteAsync()
    {
        if (ContentRef is null || _notificationRef is null)
            return;

        var allSucceeded = true;
        string? lastError = null;

        foreach (var record in _gridSelectedRecords)
        {
            var response = record.TransactionType == "Income"
                ? await IncomeService.DeleteIncomeAsync(record.Id)
                : await ExpensesService.DeleteExpensesAsync(record.Id);

            if (response is not { Success: true })
            {
                allSucceeded = false;
                lastError = response?.Message;
            }
        }

        await ContentRef.ReloadAsync();
        ContentRef.UpdateTotalBalance();
        await ContentRef.RefreshToolbarFromSelectionAsync();

        var count = _gridSelectedRecords.Count;
        var responseMessage = allSucceeded
            ? count > 1
                ? $"{count} transactions ont été supprimées avec succès"
                : "La transaction a été supprimée avec succès."
            : string.IsNullOrWhiteSpace(lastError)
                ? "Une erreur s'est produite lors de la suppression de la transaction."
                : $"Une erreur s'est produite lors de la suppression de la transaction.\nError: {lastError}";

        var status = allSucceeded ? "Success" : "Error";
        await _notificationRef.Notify(responseMessage, status, status);
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

    sealed class TransactionCategoryOption
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    sealed class TransType
    {
        public string Value { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

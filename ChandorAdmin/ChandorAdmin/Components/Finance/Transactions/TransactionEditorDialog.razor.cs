using ChandorAdmin.Components.Finance.Management;
using ChandorAdmin.Components.GlobalNotification;
using ChandorAdmin.Configuration;
using ChandorAdmin.Helpers;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.Finance;
using ChandorProject.Shared.DTOs.Account;
using ChandorProject.Shared.DTOs.Currency;
using ChandorProject.Shared.DTOs.Transaction;
using ChandorProject.Shared.DTOs.TransactionCategory;
using ChandorProject.Shared.DTOs.TransactionType;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.DropDowns;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Components.Finance.Transactions;

public partial class TransactionEditorDialog
{
    [Inject] public ITransactionService TransactionService { get; set; } = null!;
    [Inject] public ITransactionTypeService TransactionTypeService { get; set; } = null!;
    [Inject] public ITransactionCategoryService TransactionCategoryService { get; set; } = null!;
    [Inject] public ICurrencyService CurrencyService { get; set; } = null!;
    [Inject] public IAccountService AccountService { get; set; } = null!;
    [Inject] public IDepartmentService DepartmentService { get; set; } = null!;

    public TransactionGridPanel? ContentRef { get; set; }

    SfDialog? _transactionDialog;
    NotificationDialog? _notificationRef;
    CustomFormValidator? _customFormValidator;

    FinanceTransactionEditorModel _editModel = new();

    List<TransactionCategoryDto> _allCategories = new();
    List<TransactionCategoryDto> _categoryDataSource = new();
    List<TransactionTypeDto> _transactionTypes = new();
    List<CurrencyDto> _currencies = new();
    List<AccountDto> _allAccounts = new();
    List<AccountDto> _accountDataSource = new();

    bool _isAdd;
    bool _saving;
    bool _lookupsLoaded;
    DateTime _editorMinDate => new(2015, 1, 1, 0, 0, 0);
    DateTime _editorMaxDate => new(DateTime.Today.Year + 5, 12, 31, 23, 59, 59);
    TransactionView? _selectedRecord;
    string _buttonContent = "Ajouter";
    string _dialogHeader = "";
    string _formId = "transaction";
    bool _createNewDialog;
    List<TransactionView> _gridSelectedRecords = new();
    bool _pendingDialogShow;

    protected override async Task OnInitializedAsync()
    {
        await EnsureLookupsLoadedAsync();
        ResetDependentLists();
        _editModel.TransactionNote = "";
        var (_, end) = TransactionGridPanel.GetCalendarMonthBounds(DateTime.Today);
        _editModel.DateValue = end;
        _editModel.TimeValue = end;
        ApplyDefaultSelections();
    }

    public async Task RefreshLookupsAsync()
    {
        _lookupsLoaded = false;
        await EnsureLookupsLoadedAsync();
        ResetDependentLists();
        StateHasChanged();
    }

    async Task EnsureLookupsLoadedAsync()
    {
        if (_lookupsLoaded)
            return;

        try
        {
            var typesTask = TransactionTypeService.GetAllAsync();
            var categoriesTask = TransactionCategoryService.GetAllAsync();
            var currenciesTask = CurrencyService.GetAllAsync();
            var accountsTask = AccountService.GetAllAccountsAsync();
            await Task.WhenAll(typesTask, categoriesTask, currenciesTask, accountsTask);

            _transactionTypes = typesTask.Result?.Data?.ToList() ?? [];
            _allCategories = categoriesTask.Result?.Data?.ToList() ?? [];
            _currencies = currenciesTask.Result?.Data?.ToList() ?? [];
            _allAccounts = accountsTask.Result?.Data?.ToList() ?? [];
            _lookupsLoaded = true;
        }
        catch
        {
            _transactionTypes = [];
            _allCategories = [];
            _currencies = [];
            _allAccounts = [];
        }
    }

    void ApplyDefaultSelections()
    {
        _editModel.TransactionTypeId ??= _transactionTypes.FirstOrDefault()?.Id;
        _editModel.CurrencyId ??= ContentRef?.CurrencyId
            ?? FinanceDisplaySupport.SelectDefaultCurrency(_currencies)?.Id;
        ResetDependentLists();
        _editModel.CategoryId ??= _categoryDataSource.FirstOrDefault()?.Id;
        _editModel.AccountId ??= _accountDataSource.FirstOrDefault()?.Id;
    }

    void ResetDependentLists()
    {
        _categoryDataSource = _editModel.TransactionTypeId is { } typeId
            ? _allCategories.Where(c => c.TransactionTypeId == typeId).ToList()
            : [];

        _accountDataSource = _editModel.CurrencyId is { } currencyId
            ? _allAccounts.Where(a => a.CurrencyId == currencyId).ToList()
            : [];
    }

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
        _dialogHeader = "Nouvelle opération";
        _buttonContent = "Ajouter";
        _editModel.Id = Guid.Empty;
        _editModel.Amount = 0;
        var (_, end) = TransactionGridPanel.GetCalendarMonthBounds(DateTime.Today);
        _editModel.DateValue = _editModel.TimeValue = end;
        _editModel.TransactionNote = "";
        _editModel.TransactionTypeId = _transactionTypes.FirstOrDefault()?.Id;
        _editModel.CurrencyId = ContentRef?.CurrencyId
            ?? FinanceDisplaySupport.SelectDefaultCurrency(_currencies)?.Id;
        ResetDependentLists();
        _editModel.CategoryId = _categoryDataSource.FirstOrDefault()?.Id;
        _editModel.AccountId = _accountDataSource.FirstOrDefault()?.Id;
        _editModel.DepartmentId = Guid.Empty;
        _editModel.DepartmentTeamId = Guid.Empty;
    }

    public async Task ShowEditDialog(TransactionView selected)
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
        _editModel.TransactionTypeId = _selectedRecord.TransactionTypeId;
        _editModel.CurrencyId = _selectedRecord.CurrencyId != Guid.Empty
            ? _selectedRecord.CurrencyId
            : FinanceDisplaySupport.SelectDefaultCurrency(_currencies)?.Id;
        ResetDependentLists();
        _editModel.DateValue = _editModel.TimeValue = _selectedRecord.TransactionDate;
        _editModel.TransactionNote = _selectedRecord.TransactionNote;
        _editModel.Amount = _selectedRecord.Amount;
        _editModel.DepartmentId = _selectedRecord.DepartmentId;
        _editModel.DepartmentTeamId = _selectedRecord.DepartmentTeamId;
        _editModel.CategoryId = _categoryDataSource.Any(c => c.Id == _selectedRecord.TransactionCategoryId)
            ? _selectedRecord.TransactionCategoryId
            : _categoryDataSource.FirstOrDefault()?.Id;
        _editModel.AccountId = _accountDataSource.Any(a => a.Id == _selectedRecord.AccountId)
            ? _selectedRecord.AccountId
            : _accountDataSource.FirstOrDefault()?.Id;
    }

    void OnCreate()
    {
        if (_isAdd)
            UpdateAddDialog();
        else
            UpdateEditDialog();
    }

    public async Task ShowAlertDialog(List<TransactionView> selectedRecords)
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

    void OnTransactionTypeChanged(ChangeEventArgs<Guid?, TransactionTypeDto> args)
    {
        _editModel.TransactionTypeId = args.Value;
        ResetDependentLists();
        _editModel.CategoryId = _categoryDataSource.FirstOrDefault()?.Id;
    }

    void OnCurrencyChanged(ChangeEventArgs<Guid?, CurrencyDto> args)
    {
        _editModel.CurrencyId = args.Value;
        ResetDependentLists();
        _editModel.AccountId = _accountDataSource.FirstOrDefault()?.Id;
    }

    async Task OnValidSubmitAsync()
    {
        _customFormValidator?.ClearFormErrors();

        if (ContentRef is null
            || _editModel.Amount is null
            || _editModel.CategoryId is null
            || _editModel.AccountId is null
            || _editModel.CurrencyId is null
            || _editModel.TransactionTypeId is null)
            return;

        var dtBase = _editModel.DateValue ?? DateTime.Today;
        var tm = _editModel.TimeValue ?? DateTime.Today;
        var combined = dtBase.Date.Add(tm.TimeOfDay);

        _saving = true;
        try
        {
            var (departmentId, departmentTeamId) = await ResolveDepartmentKeysAsync(
                _editModel.DepartmentId,
                _editModel.DepartmentTeamId);

            if (departmentId == Guid.Empty || departmentTeamId == Guid.Empty)
            {
                await NotifyTransactionResultAsync(false, _isAdd, "Impossible de résoudre le département pour cette transaction.");
                return;
            }

            if (_isAdd)
            {
                var request = new NewTransactionDto
                {
                    TransactionDate = combined,
                    Amount = _editModel.Amount.Value,
                    TransactionNote = _editModel.TransactionNote ?? string.Empty,
                    CurrencyId = _editModel.CurrencyId.Value,
                    AccountId = _editModel.AccountId.Value,
                    TransactionCategoryId = _editModel.CategoryId.Value,
                    TransactionTypeId = _editModel.TransactionTypeId.Value,
                    DepartmentId = departmentId,
                    DepartmentTeamId = departmentTeamId
                };

                var response = await TransactionService.CreateAsync(request);
                if (response is not { Success: true })
                {
                    await NotifyTransactionResultAsync(false, _isAdd, response?.Message);
                    return;
                }

                await NotifyTransactionResultAsync(true, _isAdd);
            }
            else
            {
                var transaction = new TransactionDto
                {
                    Id = _editModel.Id,
                    TransactionDate = combined,
                    Amount = _editModel.Amount.Value,
                    TransactionNote = _editModel.TransactionNote ?? string.Empty,
                    CurrencyId = _editModel.CurrencyId.Value,
                    AccountId = _editModel.AccountId.Value,
                    TransactionCategoryId = _editModel.CategoryId.Value,
                    TransactionTypeId = _editModel.TransactionTypeId.Value,
                    DepartmentId = departmentId,
                    DepartmentTeamId = departmentTeamId
                };

                var response = await TransactionService.UpdateAsync(transaction);
                if (response is not { Success: true })
                {
                    await NotifyTransactionResultAsync(
                        false,
                        false,
                        FinanceManagementGridSupport.FormatApiErrorMessage(response, "Impossible de mettre à jour la transaction."));
                    return;
                }

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

    async Task<(Guid DepartmentId, Guid DepartmentTeamId)> ResolveDepartmentKeysAsync(
        Guid existingDepartmentId,
        Guid existingDepartmentTeamId)
    {
        var departmentId = existingDepartmentId;
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
            var response = await TransactionService.DeleteAsync(record.Id);

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
}

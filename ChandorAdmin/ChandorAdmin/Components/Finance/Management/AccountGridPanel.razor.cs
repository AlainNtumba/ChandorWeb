using ChandorAdmin.Components.GlobalNotification;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Account;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Components.Finance.Management;

public partial class AccountGridPanel : IDisposable
{
    [Inject] IAccountService AccountService { get; set; } = null!;

    SfDialog? _dialog;
    SfGrid<AccountDto>? _gridRef;
    NotificationDialog? _notificationRef;

    bool _dialogShell;
    bool _renderGrid;
    bool _saving;

    List<AccountDto> _gridData = [];

    readonly ValidationRules _requiredRules = new() { Required = true };

    readonly List<object> _toolbarItems = FinanceManagementGridSupport.CrudToolbarItems;

    public async Task ShowAsync()
    {
        if (!_dialogShell)
        {
            _dialogShell = true;
            await InvokeAsync(StateHasChanged);
            await Task.Yield();
        }

        await LoadDataAsync();

        if (_dialog is not null)
            await _dialog.ShowAsync();
    }

    async Task OnDialogOpenedAsync(Syncfusion.Blazor.Popups.OpenEventArgs _)
    {
        if (_renderGrid)
            return;

        _renderGrid = true;
        await InvokeAsync(StateHasChanged);
    }

    Task OnDialogClosedAsync(Syncfusion.Blazor.Popups.CloseEventArgs _) => Task.CompletedTask;

    async Task LoadDataAsync()
    {
        try
        {
            var response = await AccountService.GetAllAccountsAsync();
            if (response is { Success: true, Data: not null })
            {
                _gridData = response.Data.ToList();
            }
            else
            {
                _gridData = [];
                await NotifyErrorAsync(
                    FinanceManagementGridSupport.FormatApiErrorMessage(response, "Failed to load records."));
            }
        }
        catch
        {
            _gridData = [];
            await NotifyErrorAsync("Failed to load records.");
        }

        if (_renderGrid)
            await InvokeAsync(StateHasChanged);
    }

    async Task OnActionBeginAsync(ActionEventArgs<AccountDto> args)
    {
        if (args.RequestType != Syncfusion.Blazor.Grids.Action.Save || _saving)
            return;

        args.Cancel = true;

        var row = args.Data;
        if (row is null)
            return;

        if (string.IsNullOrWhiteSpace(row.AccountName) || string.IsNullOrWhiteSpace(row.AccountNumber))
        {
            await NotifyWarningAsync("Please fill all required fields.");
            return;
        }

        _saving = true;
        try
        {
            if (FinanceManagementGridSupport.IsSaveAddAction(args.Action))
            {
                var request = new NewAccountDto
                {
                    AccountName = row.AccountName.Trim(),
                    AccountNumber = row.AccountNumber.Trim(),
                    Description = row.Description?.Trim() ?? string.Empty
                };

                var response = await AccountService.CreateAccountAsync(request);
                if (response is { Success: true })
                {
                    await NotifySuccessAsync("Compte créé avec succès.");
                    await LoadDataAsync();
                    if (_gridRef is not null)
                        await _gridRef.CloseEditAsync();
                }
                else
                {
                    await NotifyErrorAsync(
                        FinanceManagementGridSupport.FormatApiErrorMessage(response, "Failed to save changes."));
                }
            }
            else if (FinanceManagementGridSupport.IsSaveEditAction(args.Action))
            {
                var response = await AccountService.UpdateAccountAsync(row);
                if (response is { Success: true })
                {
                    await NotifySuccessAsync("Compte mis à jour avec succès.");
                    await LoadDataAsync();
                    if (_gridRef is not null)
                        await _gridRef.CloseEditAsync();
                }
                else
                {
                    await NotifyErrorAsync(
                        FinanceManagementGridSupport.FormatApiErrorMessage(response, "Failed to save changes."));
                }
            }
        }
        catch
        {
            await NotifyErrorAsync("Failed to save changes.");
        }
        finally
        {
            _saving = false;
        }
    }

    async Task OnToolbarClickAsync(ClickEventArgs args)
    {
        if (_gridRef is null)
            return;

        if (FinanceManagementGridSupport.IsExcelExport(args))
        {
            await _gridRef.ExportToExcelAsync();
            return;
        }

        if (string.Equals(args.Item.Id, "Edit", StringComparison.OrdinalIgnoreCase))
        {
            await HandleEditToolbarAsync();
            return;
        }

        if (!string.Equals(args.Item.Id, "Delete", StringComparison.OrdinalIgnoreCase))
            return;

        var selected = await _gridRef.GetSelectedRecordsAsync();
        if (selected.Count == 0)
        {
            await NotifyWarningAsync("Please select a record.");
            return;
        }

        if (_notificationRef is null)
            return;

        _notificationRef.NotificationHeader = "Warning";
        _notificationRef.NotificationType = "Warning";
        _notificationRef.NotificationMessage = selected.Count > 1
            ? "Are you sure you want to delete the selected records?"
            : "Are you sure you want to delete the selected record?";

        if (!await _notificationRef.ShowAlertDialog())
            return;

        var allSucceeded = true;
        string? lastError = null;

        foreach (var record in selected)
        {
            try
            {
                var response = await AccountService.DeleteAccountAsync(record.Id);
                if (response is not { Success: true })
                {
                    allSucceeded = false;
                    lastError = FinanceManagementGridSupport.FormatApiErrorMessage(response, "Failed to delete record.");
                }
            }
            catch
            {
                allSucceeded = false;
                lastError = "Failed to delete record.";
            }
        }

        await LoadDataAsync();

        if (allSucceeded)
            await NotifySuccessAsync("Compte supprimé avec succès.");
        else
            await NotifyErrorAsync(lastError ?? "Failed to delete record.");
    }

    async Task HandleEditToolbarAsync()
    {
        if (_gridRef is null)
            return;

        var selected = await _gridRef.GetSelectedRecordsAsync();
        if (selected.Count == 0)
        {
            await NotifyWarningAsync("Please select a record.");
            return;
        }

        if (selected.Count > 1)
        {
            await NotifyWarningAsync("Please select only one record to edit.");
            return;
        }

        await _gridRef.StartEditAsync();
    }

    Task NotifySuccessAsync(string message) =>
        _notificationRef?.Notify(message, "Success", "Success") ?? Task.CompletedTask;

    Task NotifyErrorAsync(string message) =>
        _notificationRef?.Notify(message, "Error", "Error") ?? Task.CompletedTask;

    Task NotifyWarningAsync(string message) =>
        _notificationRef?.Notify(message, "Warning", "Warning") ?? Task.CompletedTask;

    public void Dispose()
    {
        _dialog = null;
        _gridRef = null;
    }
}

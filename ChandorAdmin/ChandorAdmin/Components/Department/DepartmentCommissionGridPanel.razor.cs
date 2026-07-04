using ChandorAdmin.Components.GlobalNotification;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.DepartmentTeamDto;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Components.Department;

public partial class DepartmentCommissionGridPanel : IDisposable
{
    [Inject] IDepartmentTeamService DepartmentTeamService { get; set; } = null!;

    SfDialog? _dialog;
    SfGrid<DepartmentTeamDto>? _gridRef;
    NotificationDialog? _notificationRef;

    bool _dialogShell;
    bool _renderGrid;
    bool _loading;
    bool _saving;
    Guid _departmentId;
    string _dialogHeader = "Commissions du département";

    List<DepartmentTeamDto> _gridData = [];

    readonly ValidationRules _requiredRules = new() { Required = true };

    readonly List<object> _toolbarItems = DepartmentManagementGridSupport.CrudToolbarItems;

    public async Task ShowAsync(Guid departmentId, string departmentName)
    {
        _departmentId = departmentId;
        _dialogHeader = string.IsNullOrWhiteSpace(departmentName)
            ? "Commissions du département"
            : $"Commissions — {departmentName.Trim()}";

        if (!_dialogShell)
        {
            _dialogShell = true;
            await InvokeAsync(StateHasChanged);
            await Task.Yield();
        }

        _renderGrid = false;
        await LoadDataAsync(showLoading: true);

        if (_dialog is not null)
            await _dialog.ShowAsync();
    }

    async Task OnDialogOpenedAsync(OpenEventArgs _)
    {
        if (_renderGrid)
            return;

        _renderGrid = true;
        await InvokeAsync(StateHasChanged);
    }

    Task OnDialogClosedAsync(CloseEventArgs _) => Task.CompletedTask;

    async Task LoadDataAsync(bool showLoading = false)
    {
        if (showLoading)
        {
            _loading = true;
            await InvokeAsync(StateHasChanged);
        }

        try
        {
            var response = await DepartmentTeamService.GetDepartmentTeamsByDepartmentIdAsync(_departmentId);
            if (response is { Success: true, Data: not null })
            {
                _gridData = response.Data.ToList();
            }
            else
            {
                _gridData = [];
                await NotifyErrorAsync(
                    DepartmentManagementGridSupport.FormatApiErrorMessage(response, DepartmentManagementGridSupport.LoadErrorMessage));
            }
        }
        catch
        {
            _gridData = [];
            await NotifyErrorAsync(DepartmentManagementGridSupport.LoadErrorMessage);
        }
        finally
        {
            if (showLoading)
                _loading = false;

            await InvokeAsync(StateHasChanged);
        }
    }

    async Task OnActionBeginAsync(ActionEventArgs<DepartmentTeamDto> args)
    {
        if (args.RequestType != Syncfusion.Blazor.Grids.Action.Save || _saving)
            return;

        args.Cancel = true;

        var row = args.Data;
        if (row is null || string.IsNullOrWhiteSpace(row.Name))
        {
            await NotifyWarningAsync(DepartmentManagementGridSupport.ValidationErrorMessage);
            return;
        }

        _saving = true;
        try
        {
            if (DepartmentManagementGridSupport.IsSaveAddAction(args.Action))
            {
                var request = new NewDepartmentTeamDto
                {
                    Name = row.Name.Trim(),
                    Address = row.Address?.Trim() ?? string.Empty,
                    Description = row.Description?.Trim() ?? string.Empty,
                    DepartmentId = _departmentId
                };

                var response = await DepartmentTeamService.AddDepartmentTeamAsync(request);
                if (response is { Success: true })
                {
                    await NotifySuccessAsync(DepartmentManagementGridSupport.CreateSuccessMessage);
                    await LoadDataAsync();
                    if (_gridRef is not null)
                        await _gridRef.CloseEditAsync();
                }
                else
                {
                    await NotifyErrorAsync(
                        DepartmentManagementGridSupport.FormatApiErrorMessage(response, DepartmentManagementGridSupport.SaveErrorMessage));
                }
            }
            else if (DepartmentManagementGridSupport.IsSaveEditAction(args.Action))
            {
                row.Name = row.Name.Trim();
                row.Address = row.Address?.Trim() ?? string.Empty;
                row.Description = row.Description?.Trim() ?? string.Empty;
                row.DepartmentId = _departmentId;

                var response = await DepartmentTeamService.UpdateDepartmentTeamAsync(row);
                if (response is { Success: true })
                {
                    await NotifySuccessAsync(DepartmentManagementGridSupport.UpdateSuccessMessage);
                    await LoadDataAsync();
                    if (_gridRef is not null)
                        await _gridRef.CloseEditAsync();
                }
                else
                {
                    await NotifyErrorAsync(
                        DepartmentManagementGridSupport.FormatApiErrorMessage(response, DepartmentManagementGridSupport.SaveErrorMessage));
                }
            }
        }
        catch
        {
            await NotifyErrorAsync(DepartmentManagementGridSupport.SaveErrorMessage);
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

        if (DepartmentManagementGridSupport.IsExcelExport(args))
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
            await NotifyWarningAsync(DepartmentManagementGridSupport.ValidationErrorMessage);
            return;
        }

        if (_notificationRef is null)
            return;

        _notificationRef.NotificationHeader = "Avertissement";
        _notificationRef.NotificationType = "Warning";
        _notificationRef.NotificationMessage = "Êtes-vous sûr de vouloir supprimer la commission sélectionnée ?";

        if (!await _notificationRef.ShowAlertDialog())
            return;

        try
        {
            var response = await DepartmentTeamService.DeleteDepartmentTeamAsync(selected[0].Id);
            if (response is { Success: true })
            {
                await LoadDataAsync();
                await NotifySuccessAsync(DepartmentManagementGridSupport.DeleteSuccessMessage);
            }
            else
            {
                await NotifyErrorAsync(
                    DepartmentManagementGridSupport.FormatApiErrorMessage(response, DepartmentManagementGridSupport.DeleteErrorMessage));
            }
        }
        catch
        {
            await NotifyErrorAsync(DepartmentManagementGridSupport.DeleteErrorMessage);
        }
    }

    async Task HandleEditToolbarAsync()
    {
        if (_gridRef is null)
            return;

        var selected = await _gridRef.GetSelectedRecordsAsync();
        if (selected.Count == 0)
        {
            await NotifyWarningAsync(DepartmentManagementGridSupport.ValidationErrorMessage);
            return;
        }

        await _gridRef.StartEditAsync();
    }

    Task NotifySuccessAsync(string message) =>
        _notificationRef?.Notify(message, "Success", "Succès") ?? Task.CompletedTask;

    Task NotifyErrorAsync(string message) =>
        _notificationRef?.Notify(message, "Error", "Erreur") ?? Task.CompletedTask;

    Task NotifyWarningAsync(string message) =>
        _notificationRef?.Notify(message, "Warning", "Avertissement") ?? Task.CompletedTask;

    public void Dispose()
    {
        _dialog = null;
        _gridRef = null;
    }
}

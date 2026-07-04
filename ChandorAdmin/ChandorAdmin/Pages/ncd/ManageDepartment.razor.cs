using ChandorAdmin.Components.Department;
using ChandorAdmin.Components.GlobalNotification;
using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace ChandorAdmin.Pages.ncd;

public partial class ManageDepartment
{
    SfGrid<DepartmentDto>? _gridRef;
    NotificationDialog? _notificationRef;
    DepartmentMembersGridPanel? _membersPanel;
    DepartmentCommissionGridPanel? _commissionPanel;

    bool _loading = true;
    bool _saving;

    List<DepartmentDto> _gridData = [];

    readonly ValidationRules _requiredRules = new() { Required = true };

    // Add/Update/Cancel/Search must stay built-in string tokens so Syncfusion wires inline edit and search.
    readonly List<object> _toolbarItems =
    [
        "Add",
        new ItemModel { Text = "Edit", PrefixIcon = "e-edit e-icons", TooltipText = "Edit", Id = "Edit" },
        new ItemModel { Text = "Delete", PrefixIcon = "e-delete e-icons", TooltipText = "Delete", Id = "Delete" },
        "Update",
        "Cancel",
        "ExcelExport",
        "Search"
    ];

    protected override async Task OnInitializedAsync()
    {
        await LoadDataAsync(showLoading: true);
    }

    async Task LoadDataAsync(bool showLoading = false)
    {
        if (showLoading)
        {
            _loading = true;
            await InvokeAsync(StateHasChanged);
        }

        try
        {
            var response = await dpServ.GetDepartmentsAsync();
            if (response is { Success: true, Data: not null })
            {
                _gridData = response.Data.ToList();
            }
            else
            {
                _gridData = [];
                await NotifyErrorAsync(FormatApiErrorMessage(response, "Échec du chargement des départements."));
            }
        }
        catch
        {
            _gridData = [];
            await NotifyErrorAsync("Échec du chargement des départements.");
        }
        finally
        {
            if (showLoading)
                _loading = false;

            await InvokeAsync(StateHasChanged);
        }
    }

    async Task OnActionBeginAsync(ActionEventArgs<DepartmentDto> args)
    {
        if (args.RequestType != Syncfusion.Blazor.Grids.Action.Save || _saving)
            return;

        args.Cancel = true;

        var row = args.Data;
        if (row is null || string.IsNullOrWhiteSpace(row.Name))
        {
            await NotifyWarningAsync("Veuillez remplir tous les champs obligatoires.");
            return;
        }

        _saving = true;
        try
        {
            if (IsSaveAddAction(args.Action))
            {
                var request = new NewDepartmentDto
                {
                    Name = row.Name.Trim(),
                    Description = row.Description?.Trim() ?? string.Empty
                };

                var response = await dpServ.AddDepartmentAsync(request);
                if (response is { Success: true })
                {
                    await NotifySuccessAsync("Département créé avec succès.");
                    await LoadDataAsync();
                    if (_gridRef is not null)
                        await _gridRef.CloseEditAsync();
                }
                else
                {
                    await NotifyErrorAsync(
                        FormatApiErrorMessage(response, "Échec de la mise à jour du département."));
                }
            }
            else if (IsSaveEditAction(args.Action))
            {
                var request = new UpdateDepartmentDto
                {
                    Id = row.Id,
                    Name = row.Name.Trim(),
                    Description = row.Description?.Trim() ?? string.Empty
                };

                var response = await dpServ.UpdateDepartmentAsync(request);
                if (response is { Success: true })
                {
                    await NotifySuccessAsync("Département mis à jour avec succès.");
                    await LoadDataAsync();
                    if (_gridRef is not null)
                        await _gridRef.CloseEditAsync();
                }
                else
                {
                    await NotifyErrorAsync(
                        FormatApiErrorMessage(response, "Échec de la mise à jour du département."));
                }
            }
        }
        catch
        {
            await NotifyErrorAsync("Échec de la mise à jour du département.");
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

        if (IsExcelExport(args))
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
            await NotifyWarningAsync("Veuillez sélectionner un département.");
            return;
        }

        if (_notificationRef is null)
            return;

        _notificationRef.NotificationHeader = "Avertissement";
        _notificationRef.NotificationType = "Warning";
        _notificationRef.NotificationMessage = "Êtes-vous sûr de vouloir supprimer le département sélectionné ?";

        if (!await _notificationRef.ShowAlertDialog())
            return;

        try
        {
            var response = await dpServ.DeleteDepartmentAsync(selected[0].Id);
            if (response is { Success: true })
            {
                await LoadDataAsync();
                await NotifySuccessAsync("Département supprimé avec succès.");
            }
            else
            {
                await NotifyErrorAsync(
                    FormatApiErrorMessage(response, "Échec de la suppression du département."));
            }
        }
        catch
        {
            await NotifyErrorAsync("Échec de la suppression du département.");
        }
    }

    async Task HandleEditToolbarAsync()
    {
        if (_gridRef is null)
            return;

        var selected = await _gridRef.GetSelectedRecordsAsync();
        if (selected.Count == 0)
        {
            await NotifyWarningAsync("Veuillez sélectionner un département.");
            return;
        }

        await _gridRef.StartEditAsync();
    }

    async Task OnCommandClickedAsync(CommandClickEventArgs<DepartmentDto> args)
    {
        var department = args.RowData;
        if (department is null)
            return;

        if (string.Equals(args.CommandColumn?.ID, "ViewMembres", StringComparison.OrdinalIgnoreCase))
        {
            if (_membersPanel is not null)
                await _membersPanel.ShowAsync(department.Id, department.Name);
            return;
        }

        if (string.Equals(args.CommandColumn?.ID, "ViewTeams", StringComparison.OrdinalIgnoreCase)
            && _commissionPanel is not null)
        {
            await _commissionPanel.ShowAsync(department.Id, department.Name);
        }
    }

    Task NotifySuccessAsync(string message) =>
        _notificationRef?.Notify(message, "Success", "Succès") ?? Task.CompletedTask;

    Task NotifyErrorAsync(string message) =>
        _notificationRef?.Notify(message, "Error", "Erreur") ?? Task.CompletedTask;

    Task NotifyWarningAsync(string message) =>
        _notificationRef?.Notify(message, "Warning", "Avertissement") ?? Task.CompletedTask;

    static bool IsSaveAddAction(string? action) =>
        string.Equals(action, "add", StringComparison.OrdinalIgnoreCase);

    static bool IsSaveEditAction(string? action) =>
        string.Equals(action, "edit", StringComparison.OrdinalIgnoreCase);

    static bool IsExcelExport(ClickEventArgs args) =>
        string.Equals(args.Item.Id, "Grid_excelexport", StringComparison.OrdinalIgnoreCase)
        || string.Equals(args.Item.Id, "ExcelExport", StringComparison.OrdinalIgnoreCase)
        || args.Item.Text?.Contains("Excel", StringComparison.OrdinalIgnoreCase) == true;

    static string FormatApiErrorMessage<T>(DataResponse<T>? response, string fallback)
    {
        if (response?.Message is { Length: > 0 } msg)
        {
            var errors = response.Error?.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
            return errors is { Length: > 0 } ? $"{msg} {string.Join(" ", errors)}" : msg;
        }

        if (response?.Error is { Length: > 0 } errs)
        {
            var parts = errs.Where(e => !string.IsNullOrWhiteSpace(e)).ToArray();
            if (parts.Length > 0)
                return string.Join(" ", parts);
        }

        return fallback;
    }
}

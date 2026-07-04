using ChandorAdmin.Components.GlobalNotification;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Member;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Components.Department;

public partial class DepartmentMembersGridPanel : IDisposable
{
    [Inject] IDepartmentService DepartmentService { get; set; } = null!;

    SfDialog? _dialog;
    SfGrid<MemberDto>? _gridRef;
    NotificationDialog? _notificationRef;

    bool _dialogShell;
    bool _renderGrid;
    bool _loading;
    Guid _departmentId;
    string _dialogHeader = "Membres du département";

    List<MemberDto> _gridData = [];

    readonly List<object> _toolbarItems = DepartmentManagementGridSupport.ReadOnlyToolbarItems;

    public async Task ShowAsync(Guid departmentId, string departmentName)
    {
        _departmentId = departmentId;
        _dialogHeader = string.IsNullOrWhiteSpace(departmentName)
            ? "Membres du département"
            : $"Membres — {departmentName.Trim()}";

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
            var response = await DepartmentService.GetDepartmentMembersAsync(_departmentId);
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

    async Task OnToolbarClickAsync(ClickEventArgs args)
    {
        if (_gridRef is null)
            return;

        if (DepartmentManagementGridSupport.IsExcelExport(args))
            await _gridRef.ExportToExcelAsync();
    }

    Task NotifyErrorAsync(string message) =>
        _notificationRef?.Notify(message, "Error", "Erreur") ?? Task.CompletedTask;

    public void Dispose()
    {
        _dialog = null;
        _gridRef = null;
    }
}

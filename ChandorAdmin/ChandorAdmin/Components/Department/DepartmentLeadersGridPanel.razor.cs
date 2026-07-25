using ChandorAdmin.Components.GlobalNotification;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.DepartmentTeamDto;
using ChandorProject.Shared.DTOs.MemberActivity;
using ChandorProject.Shared.DTOs.MemberRole;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;
using Syncfusion.Blazor.Popups;
using InputChangedEventArgs = Syncfusion.Blazor.Inputs.ChangedEventArgs;

namespace ChandorAdmin.Components.Department;

public partial class DepartmentLeadersGridPanel : IDisposable
{
    [Inject] IMemberActivityService MemberActivityService { get; set; } = null!;
    [Inject] IMemberService MemberService { get; set; } = null!;
    [Inject] IDepartmentTeamService DepartmentTeamService { get; set; } = null!;
    [Inject] IMemberRoleService MemberRoleService { get; set; } = null!;

    SfDialog? _dialog;
    SfGrid<MemberResponsibilityDto>? _gridRef;
    NotificationDialog? _notificationRef;

    bool _dialogShell;
    bool _renderGrid;
    bool _loading;
    bool _saving;
    Guid _departmentId;
    string _dialogHeader = "Responsables de département";

    List<MemberResponsibilityDto> _gridData = [];
    List<DepartmentTeamDto> _teams = [];
    List<MemberRoleDto> _roles = [];

    readonly List<object> _toolbarItems = DepartmentManagementGridSupport.CrudToolbarItems;

    readonly DialogSettings _editDialogSettings = new() { Width = "320px", ZIndex = 100150 };

    const string CreateSuccessMessage = "Responsable ajouté avec succès.";
    const string UpdateSuccessMessage = "Responsable modifié avec succès.";
    const string DeleteSuccessMessage = "Responsable supprimé avec succès.";
    const string SelectWarningMessage = "Veuillez sélectionner un responsable.";

    public async Task ShowAsync(Guid departmentId, string departmentName)
    {
        _departmentId = departmentId;
        _dialogHeader = string.IsNullOrWhiteSpace(departmentName)
            ? "Responsables de département"
            : $"Responsables — {departmentName.Trim()}";

        if (!_dialogShell)
        {
            _dialogShell = true;
            await InvokeAsync(StateHasChanged);
            await Task.Yield();
        }

        _renderGrid = false;
        await LoadLookupsAsync();
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

    async Task LoadLookupsAsync()
    {
        try
        {
            var teamsTask = DepartmentTeamService.GetDepartmentTeamsByDepartmentIdAsync(_departmentId);
            var rolesTask = MemberRoleService.GetAllMemberRolesAsync();
            await Task.WhenAll(teamsTask, rolesTask);

            var teamsResponse = await teamsTask;
            _teams = teamsResponse is { Success: true, Data: not null }
                ? teamsResponse.Data.ToList()
                : [];

            var rolesResponse = await rolesTask;
            _roles = rolesResponse is { Success: true, Data: not null }
                ? rolesResponse.Data.ToList()
                : [];
        }
        catch
        {
            _teams = [];
            _roles = [];
        }
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
            var response = await MemberActivityService.GetMemberResponsibilitiesByDepartmentIdAsync(_departmentId);
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

    async Task OnUsernameSearchAsync(InputChangedEventArgs args, MemberResponsibilityDto row)
    {
        var username = args.Value?.Trim() ?? string.Empty;
        row.Username = username;

        if (string.IsNullOrWhiteSpace(username))
        {
            row.MemberId = Guid.Empty;
            return;
        }

        try
        {
            var response = await MemberService.GetMemberByUsernameAsync(username);
            if (response is { Success: true, Data: not null })
            {
                row.MemberId = response.Data.Id;
                row.Username = response.Data.Username;
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                row.MemberId = Guid.Empty;
                await NotifyWarningAsync(
                    DepartmentManagementGridSupport.FormatApiErrorMessage(response, "Aucun membre trouvé pour ce nom d'utilisateur."));
            }
        }
        catch
        {
            row.MemberId = Guid.Empty;
            await NotifyErrorAsync(DepartmentManagementGridSupport.LoadErrorMessage);
        }
    }

    async Task OnActionBeginAsync(ActionEventArgs<MemberResponsibilityDto> args)
    {
        if (args.RequestType != Syncfusion.Blazor.Grids.Action.Save || _saving)
            return;

        args.Cancel = true;

        var row = args.Data;
        if (row is null
            || row.MemberId == Guid.Empty
            || row.DepartmentTeamId == Guid.Empty
            || row.MemberRoleId == Guid.Empty)
        {
            await NotifyWarningAsync(DepartmentManagementGridSupport.ValidationErrorMessage);
            return;
        }

        _saving = true;
        try
        {
            if (DepartmentManagementGridSupport.IsSaveAddAction(args.Action))
            {
                var request = new NewMemberActivityDto
                {
                    MemberId = row.MemberId,
                    DepartmentTeamId = row.DepartmentTeamId,
                    MemberRoleId = row.MemberRoleId
                };

                var response = await MemberActivityService.CreateMemberActivityAsync(request);
                if (response is { Success: true })
                {
                    await NotifySuccessAsync(CreateSuccessMessage);
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
                var request = new MemberActivityDto
                {
                    Id = row.RoleId,
                    MemberId = row.MemberId,
                    DepartmentTeamId = row.DepartmentTeamId,
                    MemberRoleId = row.MemberRoleId
                };

                var response = await MemberActivityService.UpdateMemberActivityAsync(request);
                if (response is { Success: true })
                {
                    await NotifySuccessAsync(UpdateSuccessMessage);
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
            await NotifyWarningAsync(SelectWarningMessage);
            return;
        }

        if (_notificationRef is null)
            return;

        _notificationRef.NotificationHeader = "Avertissement";
        _notificationRef.NotificationType = "Warning";
        _notificationRef.NotificationMessage = "Êtes-vous sûr de vouloir supprimer le responsable sélectionné ?";

        if (!await _notificationRef.ShowAlertDialog())
            return;

        try
        {
            var response = await MemberActivityService.DeleteMemberActivityAsync(selected[0].RoleId);
            if (response is { Success: true })
            {
                await LoadDataAsync();
                await NotifySuccessAsync(DeleteSuccessMessage);
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
            await NotifyWarningAsync(SelectWarningMessage);
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

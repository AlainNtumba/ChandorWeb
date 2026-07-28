using System.ComponentModel.DataAnnotations;
using ChandorAdmin.Components.GlobalNotification;
using ChandorAdmin.Configuration;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Popups;
using ChandorProject.Shared.DTOs.MemberRole;

namespace ChandorAdmin.Components.DepartmentManagement;

public partial class DepartmentMemberAddDialog
{
    public DepartmentMemberGridPanel? ContentRef { get; set; }
    public NotificationDialog? NotificationRef { get; set; }

    SfDialog? _dialog;
    CustomFormValidator? _customFormValidator;
    AddDepartmentMemberModel _editModel = new();
    List<MemberRoleDto> _roles = [];

    bool _createNewDialog;
    bool _pendingDialogShow;
    bool _saving;
    Guid _departmentId;
    string _dialogHeader = "Ajouter Membre";
    string _formId = "DepartmentMemberAdd";

    public async Task ShowAddDialog(Guid departmentId)
    {
        _departmentId = departmentId;
        _dialogHeader = "Ajouter Membre";
        ResetModel();

        var hadShell = _createNewDialog;
        await LoadRolesAsync();

        if (!hadShell)
        {
            _createNewDialog = true;
            _pendingDialogShow = true;
            StateHasChanged();
            return;
        }

        StateHasChanged();
        await Task.Yield();
        if (_dialog is not null)
            await _dialog.ShowAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_pendingDialogShow && _dialog is not null)
        {
            _pendingDialogShow = false;
            await _dialog.ShowAsync();
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    async Task LoadRolesAsync()
    {
        try
        {
            var response = await MemberRoleService.GetAllMemberRolesAsync();
            _roles = response is { Success: true, Data: not null }
                ? response.Data.OrderBy(r => r.Name, StringComparer.OrdinalIgnoreCase).ToList()
                : [];
        }
        catch
        {
            _roles = [];
        }
    }

    void ResetModel()
    {
        _editModel = new AddDepartmentMemberModel();
        _customFormValidator?.ClearFormErrors();
    }

    async Task OnUsernameChangedAsync(ChangedEventArgs args)
    {
        var username = args.Value?.Trim() ?? string.Empty;
        _editModel.Username = username;
        _editModel.MemberId = Guid.Empty;
        _customFormValidator?.ClearFormErrors();

        if (string.IsNullOrWhiteSpace(username))
            return;

        try
        {
            var response = await MemberService.GetMemberByUsernameAsync(username);
            if (response is { Success: true, Data: not null })
            {
                _editModel.MemberId = response.Data.Id;
                _editModel.Username = response.Data.Username;
                await InvokeAsync(StateHasChanged);
            }
            else
            {
                _editModel.MemberId = Guid.Empty;
                _customFormValidator?.DisplayFormErrors(new Dictionary<string, List<string>>
                {
                    [nameof(AddDepartmentMemberModel.Username)] =
                    [
                        string.IsNullOrWhiteSpace(response?.Message)
                            ? "Aucun membre trouvé pour ce nom d'utilisateur."
                            : response.Message
                    ]
                });
            }
        }
        catch
        {
            _editModel.MemberId = Guid.Empty;
            _customFormValidator?.DisplayFormErrors(new Dictionary<string, List<string>>
            {
                [nameof(AddDepartmentMemberModel.Username)] =
                    ["Une erreur est survenue lors de la validation du nom d'utilisateur."]
            });
        }
    }

    async Task OnValidSubmitAsync()
    {
        _customFormValidator?.ClearFormErrors();

        var errors = new Dictionary<string, List<string>>();

        if (string.IsNullOrWhiteSpace(_editModel.Username) || _editModel.MemberId == Guid.Empty)
        {
            errors[nameof(AddDepartmentMemberModel.Username)] =
                ["Un nom d'utilisateur valide est requis."];
        }

        if (_editModel.RoleId == Guid.Empty)
        {
            errors[nameof(AddDepartmentMemberModel.RoleId)] =
                ["Veuillez sélectionner un rôle."];
        }

        if (errors.Count > 0)
        {
            _customFormValidator?.DisplayFormErrors(errors);
            return;
        }

        if (_departmentId == Guid.Empty)
            return;

        _saving = true;
        try
        {
            var response = await DepartmentService.AddDepartmentMemberAsync(
                _departmentId,
                _editModel.MemberId,
                _editModel.RoleId);

            if (response is { Success: true })
            {
                if (NotificationRef is not null)
                {
                    var message = string.IsNullOrWhiteSpace(response.Message)
                        ? "Membre ajouté au département avec succès."
                        : response.Message;
                    await NotificationRef.Notify(message, "Success", "Success");
                }

                if (ContentRef is not null)
                    await ContentRef.LoadData();

                if (_dialog is not null)
                    await _dialog.HideAsync();
            }
            else
            {
                var errorMessage = response?.Message;
                if (string.IsNullOrWhiteSpace(errorMessage))
                    errorMessage = "Une erreur est survenue lors de l'ajout du membre.";

                if (NotificationRef is not null)
                    await NotificationRef.Notify(errorMessage, "Error", "Error");
            }
        }
        catch
        {
            if (NotificationRef is not null)
                await NotificationRef.Notify("Une erreur est survenue lors de l'ajout du membre.", "Error", "Error");
        }
        finally
        {
            _saving = false;
        }
    }

    async Task OnClickCancelAsync()
    {
        _customFormValidator?.ClearFormErrors();
        if (_createNewDialog && _dialog is not null)
            await _dialog.HideAsync();
    }

    public sealed class AddDepartmentMemberModel
    {
        [Required(ErrorMessage = "This field is required.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "This field is required.")]
        public Guid RoleId { get; set; }

        public Guid MemberId { get; set; }
    }
}

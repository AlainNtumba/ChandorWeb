using ChandorAdmin.Configuration;
using ChandorProject.Shared.DTOs.AgeGroup;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.DTOs.MemberType;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Components.Member;

public partial class MemberEditorDialog
{
    public MemberGridPanel? ContentRef { get; set; }
    SfDialog? _memberDialog;
    CustomFormValidator? _customFormValidator;
    MemberDto? _selectedRecord;
    MemberDto _editModel = new();
    List<MemberDto> _gridSelectedRecords = new();
    IEnumerable<AgeGroupDto> _ageGroups = Array.Empty<AgeGroupDto>();
    IEnumerable<MemberTypeDto> _memberTypes = Array.Empty<MemberTypeDto>();

    bool _isAdd;
    bool _createNewDialog;
    bool _pendingDialogShow;
    string _buttonContent = "Add";
    string _dialogHeader = "";
    string _formId = "Member";

    List<Gender> _genders = [];

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();

        _genders = [new Gender { Name = "Femme", Sex = false }, new Gender { Name = "Homme", Sex = true }];

        var types = await _typeServ.GetAllMemberTypesAsync();

        var groups = await _ageServ.GetAllAgeGroupsAsync();

        if (types != null && types.Success && types.Data != null) _memberTypes = types.Data.OrderBy(t => t.Name);

        if (groups != null && groups.Success && groups.Data != null) _ageGroups = groups.Data.OrderBy(a => a.FromAge);
    }

    public async Task ShowEditDialog(MemberDto selected)
    {
        _isAdd = false;
        _selectedRecord = selected;
        var hadShell = _createNewDialog;

        OnCreate();

        if (!hadShell)
        {
            _createNewDialog = true;
            _pendingDialogShow = true;
            StateHasChanged();
            return;
        }

        StateHasChanged();
        await Task.Yield();
        if (_memberDialog is not null)
            await _memberDialog.ShowAsync();
    }

    public async Task ShowAddDialog()
    {
        _isAdd = true;
        var hadShell = _createNewDialog;

        OnCreate();

        if (!hadShell)
        {
            _createNewDialog = true;
            _pendingDialogShow = true;
            StateHasChanged();
            return;
        }

        StateHasChanged();
        await Task.Yield();
        if (_memberDialog is not null)
            await _memberDialog.ShowAsync();
    }

    async Task OnClickCancel()
    {
        _customFormValidator?.ClearFormErrors();
        if (_createNewDialog && _memberDialog is not null)
            await _memberDialog.HideAsync();
    }

    void UpdateAddDialog()
    {
        _dialogHeader = "New Member";
        _buttonContent = "Add";
        _editModel = new();
    }

    void UpdateEditDialog()
    {
        _dialogHeader = "Edit Member";
        _buttonContent = "Save";
        if (_selectedRecord is null)
            return;

        _editModel.Id = _selectedRecord.Id;
        _editModel.Username = _selectedRecord.Username;
        _editModel.Name = _selectedRecord.Name;
        _editModel.Surname = _selectedRecord.Surname;
        _editModel.Postname = _selectedRecord.Postname;
        _editModel.Birthday = _selectedRecord.Birthday;
        _editModel.Gender = _selectedRecord.Gender;
        _editModel.Country = _selectedRecord.Country;
        _editModel.Town = _selectedRecord.Town;
        _editModel.Suburb = _selectedRecord.Suburb;
        _editModel.Address = _selectedRecord.Address;
        _editModel.Note = _selectedRecord.Note;
        _editModel.AgeGroupId = _selectedRecord.AgeGroupId;
        _editModel.MemberTypeId = _selectedRecord.MemberTypeId;
    }

    void OnCreate()
    {
        if (_isAdd)
            UpdateAddDialog();
        else
            UpdateEditDialog();
    }

    async Task OnValidSubmitAsync()
    {
        _customFormValidator?.ClearFormErrors();

        if (ContentRef is null || _editModel is null)
            return;

        if (_isAdd)
            await ContentRef.AddMember(SetNewMember());
        else
            await ContentRef.EditMember(SetEditMember());

        if (_memberDialog is not null)
            await _memberDialog.HideAsync();
    }

    NewMemberDto SetNewMember()
    {
        var newMember = new NewMemberDto
        {
            Username = _editModel.Username,
            Name = _editModel.Name,
            Surname = _editModel.Surname,
            Postname = _editModel.Postname,
            Birthday = _editModel.Birthday,
            Gender = _editModel.Gender,
            Country = _editModel.Country,
            Town = _editModel.Town,
            Suburb = _editModel.Suburb,
            Address = _editModel.Address,
            Note = _editModel.Note,
            AgeGroupId = _editModel.AgeGroupId,
            MemberTypeId = _editModel.MemberTypeId,
            UserId = Guid.Parse("49c2dd77-23a8-491d-bf72-3a5c4a26e74a"),
            Email1 = "web@register.com", Email2 = "web@register.com",
            NumPhone1 = "024300000", NumPhone2 = "024300000",
        };

        return newMember;
    }

    UpdateMemberDto SetEditMember()
    {
        var member = new UpdateMemberDto
        {
            Id = _editModel.Id,
            Username = _editModel.Username,
            Name = _editModel.Name,
            Surname = _editModel.Surname,
            Postname = _editModel.Postname,
            Birthday = _editModel.Birthday,
            Gender = _editModel.Gender,
            Country = _editModel.Country,
            Town = _editModel.Town,
            Suburb = _editModel.Suburb,
            Address = _editModel.Address,
            Note = _editModel.Note,
            AgeGroupId = _editModel.AgeGroupId,
            MemberTypeId = _editModel.MemberTypeId,
        };

        return member;
    }

    public async Task ShowAlertDialog(List<MemberDto> selectedRecords)
    {
        _gridSelectedRecords = selectedRecords;
        var confirm = await DialogService.ConfirmAsync(
            "Are you sure you want to delete the selected member(s)?",
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

    async Task OnClickDeleteAsync()
    {
        if (ContentRef is null)
            return;

        var ids = _gridSelectedRecords.Select(s => s.Id).ToList();
        //ContentRef.RemoveRecord(ids);
        //ContentRef.UpdateTotalBalance();
        await ContentRef.RefreshToolbarFromSelectionAsync();
    }

    private class Gender
    {
        public bool Sex { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
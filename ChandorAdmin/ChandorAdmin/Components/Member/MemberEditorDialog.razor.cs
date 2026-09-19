using ChandorAdmin.Configuration;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.Member;
using ChandorProject.Shared.DTOs.AgeGroup;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.DTOs.MemberType;
using Syncfusion.Blazor.Popups;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;

namespace ChandorAdmin.Components.Member;

public partial class MemberEditorDialog
{
    private const long MaximumProfileImageSize = 5 * 1024 * 1024;

    [Inject] private IMemberService MemberService { get; set; } = default!;
    [Inject] private IOptions<ChandorApiOptions> ApiOptions { get; set; } = default!;

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
    bool _isSaving;
    bool _deleteProfileImage;
    int _profileInputKey;
    string _buttonContent = "Add";
    string _dialogHeader = "";
    string _formId = "Member";
    string? _email1;
    string? _email2;
    string? _phone1;
    string? _phone2;
    string? _existingProfileImageUrl;
    string? _profileError;
    MemberProfileImageUpload? _selectedProfileImage;

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
        var fullMember = await MemberService.GetMemberAsync(selected.Id);
        _selectedRecord = fullMember is { Success: true, Data: not null }
            ? fullMember.Data
            : selected;
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
        ResetPendingProfileImage();
        if (_createNewDialog && _memberDialog is not null)
            await _memberDialog.HideAsync();
    }

    void UpdateAddDialog()
    {
        _dialogHeader = "New Member";
        _buttonContent = "Add";
        _editModel = new();
        _email1 = null;
        _email2 = null;
        _phone1 = null;
        _phone2 = null;
        _existingProfileImageUrl = null;
        ResetPendingProfileImage();
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
        _email1 = _selectedRecord.Emails.ElementAtOrDefault(0);
        _email2 = _selectedRecord.Emails.ElementAtOrDefault(1);
        _phone1 = _selectedRecord.PhoneNumbers.ElementAtOrDefault(0);
        _phone2 = _selectedRecord.PhoneNumbers.ElementAtOrDefault(1);
        _existingProfileImageUrl = _selectedRecord.ProfileImageUrl;
        ResetPendingProfileImage(keepExistingUrl: true);
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
        if (_isSaving)
            return;

        _customFormValidator?.ClearFormErrors();
        _profileError = null;

        if (ContentRef is null || _editModel is null)
            return;

        _isSaving = true;
        try
        {
            MemberDto? savedMember;
            if (_isAdd)
            {
                savedMember = await ContentRef.AddMember(SetNewMember());
                if (savedMember is null)
                    return;

                _editModel.Id = savedMember.Id;
                _isAdd = false;
                _buttonContent = "Save";
                _dialogHeader = "Edit Member";
            }
            else
            {
                savedMember = await ContentRef.EditMember(SetEditMember());
                if (savedMember is null)
                    return;
            }

            if (!await ApplyPendingProfileImageAsync(savedMember.Id))
                return;

            await ContentRef.LoadData();
            if (_memberDialog is not null)
                await _memberDialog.HideAsync();
        }
        finally
        {
            _isSaving = false;
        }
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
            UserId = null,
            Email1 = NullIfWhiteSpace(_email1),
            Email2 = NullIfWhiteSpace(_email2),
            NumPhone1 = NullIfWhiteSpace(_phone1),
            NumPhone2 = NullIfWhiteSpace(_phone2),
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
            Email1 = NullIfWhiteSpace(_email1),
            Email2 = NullIfWhiteSpace(_email2),
            NumPhone1 = NullIfWhiteSpace(_phone1),
            NumPhone2 = NullIfWhiteSpace(_phone2),
        };

        return member;
    }

    private string? ProfilePreviewUrl
    {
        get
        {
            if (_selectedProfileImage is { } selected)
                return $"data:{selected.ContentType};base64,{Convert.ToBase64String(selected.Content)}";
            return !_deleteProfileImage && !string.IsNullOrWhiteSpace(_existingProfileImageUrl)
                ? GetProfileImageUrl(_existingProfileImageUrl)
                : null;
        }
    }

    private bool HasExistingProfileImage => !string.IsNullOrWhiteSpace(_existingProfileImageUrl);
    private bool HasVisibleProfileImage => _selectedProfileImage is not null || (HasExistingProfileImage && !_deleteProfileImage);

    private string MemberInitials
    {
        get
        {
            var first = string.IsNullOrWhiteSpace(_editModel.Name) ? null : _editModel.Name.Trim()[0].ToString();
            var second = string.IsNullOrWhiteSpace(_editModel.Surname) ? null : _editModel.Surname.Trim()[0].ToString();
            return string.Concat(first, second).ToUpperInvariant() is { Length: > 0 } initials ? initials : "?";
        }
    }

    private async Task OnProfileImageSelectedAsync(InputFileChangeEventArgs args)
    {
        _profileError = null;
        var file = args.File;
        if (file.Size <= 0)
        {
            _profileError = "Le fichier sélectionné est vide.";
            return;
        }
        if (file.Size > MaximumProfileImageSize)
        {
            _profileError = "La photo ne peut pas dépasser 5 Mo.";
            return;
        }

        try
        {
            await using var source = file.OpenReadStream(MaximumProfileImageSize);
            using var destination = new MemoryStream((int)file.Size);
            await source.CopyToAsync(destination);
            var content = destination.ToArray();
            var contentType = DetectImageContentType(content);
            if (contentType is null)
            {
                _profileError = "Sélectionnez une image JPEG, PNG ou WebP valide.";
                return;
            }

            _selectedProfileImage = new MemberProfileImageUpload(file.Name, contentType, content);
            _deleteProfileImage = false;
        }
        catch (IOException)
        {
            _profileError = "Impossible de lire la photo sélectionnée.";
        }
    }

    private void RemoveSelectedProfileImage()
    {
        _selectedProfileImage = null;
        _profileError = null;
        _profileInputKey++;
    }

    private void MarkProfileImageForDeletion()
    {
        _selectedProfileImage = null;
        _deleteProfileImage = true;
        _profileError = null;
    }

    private void CancelProfileImageDeletion() => _deleteProfileImage = false;

    private async Task<bool> ApplyPendingProfileImageAsync(Guid memberId)
    {
        if (_selectedProfileImage is { } selected)
        {
            var response = await MemberService.UploadProfileImageAsync(memberId, selected);
            if (response is null || !response.Success)
            {
                _profileError = FormatApiError(response, "Impossible d’enregistrer la photo de profil.");
                return false;
            }
            _existingProfileImageUrl = response.Data?.ProfileImageUrl;
        }
        else if (_deleteProfileImage)
        {
            var response = await MemberService.DeleteProfileImageAsync(memberId);
            if (response is null || !response.Success)
            {
                _profileError = FormatApiError(response, "Impossible de supprimer la photo de profil.");
                return false;
            }
            _existingProfileImageUrl = null;
        }

        ResetPendingProfileImage(keepExistingUrl: true);
        return true;
    }

    private void ResetPendingProfileImage(bool keepExistingUrl = false)
    {
        _selectedProfileImage = null;
        _deleteProfileImage = false;
        _profileError = null;
        _profileInputKey++;
        if (!keepExistingUrl)
            _existingProfileImageUrl = null;
    }

    private static string? DetectImageContentType(ReadOnlySpan<byte> content)
    {
        if (content.Length >= 3 && content[0] == 0xFF && content[1] == 0xD8 && content[2] == 0xFF)
            return "image/jpeg";
        ReadOnlySpan<byte> png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
        if (content.Length >= png.Length && content[..png.Length].SequenceEqual(png))
            return "image/png";
        if (content.Length >= 12 && content[..4].SequenceEqual("RIFF"u8) && content.Slice(8, 4).SequenceEqual("WEBP"u8))
            return "image/webp";
        return null;
    }

    private static string? NullIfWhiteSpace(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private string GetProfileImageUrl(string value)
    {
        if (Uri.TryCreate(value, UriKind.Absolute, out var absolute))
            return absolute.ToString();
        var configuredBase = new Uri(ApiOptions.Value.BaseUrl.TrimEnd('/') + "/");
        var origin = new Uri(configuredBase.GetLeftPart(UriPartial.Authority) + "/");
        return new Uri(origin, value.TrimStart('/')).ToString();
    }

    private static string FormatApiError<T>(ChandorProject.Shared.Models.DataResponse<T>? response, string fallback)
    {
        if (response is null)
            return fallback;
        var detail = string.Join(" ", (response.Error ?? []).Where(value => !string.IsNullOrWhiteSpace(value))!);
        return string.IsNullOrWhiteSpace(detail)
            ? response.Message ?? fallback
            : $"{response.Message ?? fallback} {detail}";
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

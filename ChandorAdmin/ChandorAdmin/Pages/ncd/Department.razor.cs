using ChandorAdmin.Components.DepartmentManagement;
using ChandorAdmin.Components.GlobalNotification;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Inputs;

namespace ChandorAdmin.Pages.ncd;

public partial class Department
{
    [Inject] private IDepartmentService DepartmentService { get; set; } = null!;

    [Parameter] public Guid DepartmentId { get; set; }

    DepartmentMemberGridPanel? _gridRef;
    DepartmentMemberFilterSidebar? _filterRef;
    DepartmentMemberAddDialog? _dialogRef;
    NotificationDialog? _notificationRef;
    DepartmentDto? _department;

    string? _searchValue;
    string _searchVisibility = "none";
    string _searchHeadBg = "#FFFFFF";
    string _selectedGenderValue = string.Empty;
    bool _didInitialWire;
    bool _isDataLoaded;
    bool _pageLoading;
    bool _syncingGenderFilter;
    string? _pageError;
    bool _invalidDepartmentId;

    readonly List<GenderFilterOption> _genderOptions =
    [
        new() { Value = string.Empty, Name = "Tous" },
        new() { Value = "false", Name = "Femme" },
        new() { Value = "true", Name = "Homme" }
    ];

    string PageTitleText => _department?.Name is { Length: > 0 } n
        ? $"{n} | Chandelier d'Or"
        : "Department | Chandelier d'Or";

    protected override async Task OnParametersSetAsync()
    {
        if (DepartmentId == Guid.Empty)
        {
            _invalidDepartmentId = true;
            _department = null;
            _pageError = null;
            _pageLoading = false;
            _didInitialWire = false;
            return;
        }

        _invalidDepartmentId = false;
        await LoadDepartmentDetailsAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        await base.OnAfterRenderAsync(firstRender);
        if (_invalidDepartmentId || _pageLoading || _pageError is not null)
            return;

        if (!_isDataLoaded || _didInitialWire || _gridRef is null || _filterRef is null || _dialogRef is null)
            return;

        _didInitialWire = true;
        _gridRef.DialogRef = _dialogRef;
        _gridRef.NotificationRef = _notificationRef;
        _filterRef.ContentRef = _gridRef;
        _dialogRef.ContentRef = _gridRef;
        _dialogRef.NotificationRef = _notificationRef;
        _gridRef.FilterRef = _filterRef;
        _filterRef.ToolbarSyncRequested = SyncToolbarFromSidebar;

        await _gridRef.LoadData();
        _filterRef.UpdateGrid();

        await InvokeAsync(StateHasChanged);
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender) _ = LoadAsync();
    }

    Task RunSearchAsync() => _gridRef?.SearchAsync(_searchValue) ?? Task.CompletedTask;

    Task OpenAddDialogAsync() => _dialogRef?.ShowAddDialog(DepartmentId) ?? Task.CompletedTask;

    void OpenFilterMenu() => _filterRef?.ShowFilterMenu();

    async Task OnSearchChanged(ChangedEventArgs args)
    {
        if (_gridRef is not null)
            await _gridRef.SearchAsync(_searchValue);
    }

    void OnSearchCreated() => (_searchVisibility, _searchHeadBg) = ("", "");

    void OnToolbarGenderChanged(string value)
    {
        if (_syncingGenderFilter || _filterRef is null)
            return;

        _syncingGenderFilter = true;
        _selectedGenderValue = value ?? string.Empty;

        if (string.IsNullOrWhiteSpace(_selectedGenderValue))
            _filterRef.ClearGenderFilter();
        else
            _filterRef.SetGenderFilter(_selectedGenderValue);

        _syncingGenderFilter = false;
    }

    void SyncToolbarFromSidebar()
    {
        if (_syncingGenderFilter || _filterRef is null)
            return;

        _syncingGenderFilter = true;
        _selectedGenderValue = _filterRef.GetSelectedGenderFilterValue();
        _syncingGenderFilter = false;
        InvokeAsync(StateHasChanged);
    }

    async Task LoadDepartmentDetailsAsync()
    {
        _pageLoading = true;
        _pageError = null;
        _department = null;
        _didInitialWire = false;
        await InvokeAsync(StateHasChanged);

        try
        {
            var response = await DepartmentService.GetDepartmentAsync(DepartmentId);
            if (response is { Success: true, Data: not null } && response.Data.Id != Guid.Empty)
            {
                _department = response.Data;
            }
            else
            {
                _pageError = FormatDepartmentPageError(response);
            }
        }
        catch (Exception)
        {
            _pageError = "Unable to load this department. Please try again later.";
        }
        finally
        {
            _pageLoading = false;
            await InvokeAsync(StateHasChanged);
        }
    }

    async Task LoadAsync()
    {
        await Task.Delay(500);
        _isDataLoaded = true;
        await InvokeAsync(StateHasChanged);
    }

    static string FormatDepartmentPageError(DataResponse<DepartmentDto>? response)
    {
        if (response?.Message is { Length: > 0 } msg)
            return msg;

        if (response?.Error is { Length: > 0 } errs)
        {
            var parts = errs.Where(static e => !string.IsNullOrWhiteSpace(e)).ToArray();
            if (parts.Length > 0)
                return string.Join(" ", parts);
        }

        return "Department was not found or could not be loaded.";
    }

    public sealed class GenderFilterOption
    {
        public string Value { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }
}

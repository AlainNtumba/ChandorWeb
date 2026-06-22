using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace ChandorAdmin.Pages.ncd;

public partial class Department
{
    [Inject] private IDepartmentService DepartmentService { get; set; } = null!;
    [Inject] private IChurchProgramService ProgramService { get; set; } = null!;

    [Parameter] public Guid DepartmentId { get; set; }
    public SfGrid<MemberDto>? MemberGridRef { get; set; }
    public SfGrid<ChurchProgramDto>? ProgramGridRef { get; set; }
    public IEnumerable<ChurchProgramDto> _depPrograms { get; set; } = Array.Empty<ChurchProgramDto>();

    public IEnumerable<MemberDto> _depMembers { get; set; } = Array.Empty<MemberDto>();

    public List<object> MemberToolbaritems { get; set; } = new List<object>
    {
        new ItemModel { Text = "Add", PrefixIcon = "e-add e-icons", TooltipText = "Add", Id = "AddMember" },
        new ItemModel { Text = "Edit", PrefixIcon = "e-edit e-icons", TooltipText = "Edit", Id = "EditMember" },
        new ItemModel { Text = "Delete", PrefixIcon = "e-delete e-icons", TooltipText = "Delete", Id = "DeleteMember" },
        new ItemModel { Text = "Excel Export", PrefixIcon = "e-excelexport e-icons", TooltipText = "ExcelExport", Id = "Member_excelexport" },
        "Search"
    };

    public List<object> ProgramToolbaritems { get; } =
    [
        new ItemModel { Text = "Add", PrefixIcon = "e-add e-icons", TooltipText = "Add", Id = "AddProgram" },
        new ItemModel { Text = "Edit", PrefixIcon = "e-edit e-icons", TooltipText = "Edit", Id = "EditProgram" },
        new ItemModel { Text = "Delete", PrefixIcon = "e-delete e-icons", TooltipText = "Delete", Id = "DeleteProgram" },
        new ItemModel { Text = "Excel Export", PrefixIcon = "e-excelexport e-icons", TooltipText = "ExcelExport", Id = "Program_excelexport" },
        "Search"
    ];

    DepartmentDto? _department;
    bool _pageLoading;
    string? _pageError;
    bool _invalidDepartmentId;

    protected override async Task OnInitializedAsync()
    {
        if (!_invalidDepartmentId)
        {
            var _members = await DepartmentService.GetDepartmentMembersAsync(DepartmentId);
            var _programs = await ProgramService.GetDepartmentProgramAsync(DepartmentId);

            _depPrograms = _programs?.Data ?? Array.Empty<ChurchProgramDto>();
            _depMembers = _members?.Data ?? Array.Empty<MemberDto>();
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (DepartmentId == Guid.Empty)
        {
            _invalidDepartmentId = true;
            _department = null;
            _pageError = null;
            _pageLoading = false;
            return;
        }

        _invalidDepartmentId = false;
        await LoadDepartmentDetailsAsync();
    }

    async Task LoadDepartmentDetailsAsync()
    {
        _pageLoading = true;
        _pageError = null;
        _department = null;
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
}

using ChandorAdmin.Components.GlobalNotification;
using ChandorProject.Shared.DTOs.Member;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace ChandorAdmin.Components.DepartmentManagement;

public partial class DepartmentMemberGridPanel
{
    [Parameter] public Guid DepartmentId { get; set; }

    public IEnumerable<MemberDto> GridData { get; set; } = Array.Empty<MemberDto>();
    public IReadOnlyList<MemberDto> AllMembers { get; private set; } = Array.Empty<MemberDto>();
    public SfGrid<MemberDto>? MemberGridRef { get; set; }
    public DepartmentMemberFilterSidebar? FilterRef { get; set; }
    public DepartmentMemberAddDialog? DialogRef { get; set; }
    public NotificationDialog? NotificationRef { get; set; }

    public List<ItemModel> Toolbaritems { get; } =
    [
        new ItemModel { Text = "Delete", PrefixIcon = "e-delete e-icons", TooltipText = "Delete", Id = "Delete", Disabled = true },
        new ItemModel { Text = "Excel Export", PrefixIcon = "e-excelexport e-icons", TooltipText = "ExcelExport", Id = "Grid_excelexport" }
    ];

    bool _renderGrid;
    Guid _loadedDepartmentId;
    string? _lastSearch;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await base.OnAfterRenderAsync(firstRender);
            await InvokeAsync(async () =>
            {
                await Task.Delay(1);
                _renderGrid = true;
                StateHasChanged();
            });
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (DepartmentId == Guid.Empty || DepartmentId == _loadedDepartmentId)
            return;

        if (_renderGrid)
            await LoadData();
    }

    public async Task LoadData()
    {
        if (DepartmentId == Guid.Empty)
        {
            AllMembers = [];
            GridRefresh(AllMembers);
            return;
        }

        try
        {
            var data = await srv.GetDepartmentMembersAsync(DepartmentId);
            if (data is { Success: true, Data: not null })
                AllMembers = data.Data.ToList();
            else
                AllMembers = [];
        }
        catch
        {
            AllMembers = [];
        }

        _loadedDepartmentId = DepartmentId;
        GridRefresh(FilterRef?.RefreshData() ?? AllMembers);

        if (FilterRef is not null)
            await FilterRef.RebuildFilterListsAsync();

        if (!string.IsNullOrWhiteSpace(_lastSearch))
            await SearchAsync(_lastSearch);

        StateHasChanged();
    }

    public async Task RemoveMember(MemberDto member)
    {
        var delete = await srv.RemoveDepartmentMemberAsync(DepartmentId, member.Id);

        if (delete is not null && NotificationRef is not null)
        {
            var status = delete.Success ? "Success" : "Error";
            var message = delete.Success
                ? delete.Message
                : $"An error occured while removing the member. \nError: {delete.Message}";
            var checkedMsg = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            var responseMessage = delete.Success
                ? $"Member {member.Name} was successfully removed from the department"
                : checkedMsg;

            await NotificationRef.Notify(responseMessage, status, status);

            if (delete.Success)
                await LoadData();
        }
    }

    public async Task SearchAsync(string? value)
    {
        _lastSearch = value;
        await (MemberGridRef?.SearchAsync(value ?? string.Empty) ?? Task.CompletedTask);
    }

    public void GridRefresh(IEnumerable<MemberDto> rows)
    {
        GridData = rows;
        StateHasChanged();
    }

    public async Task ToolbarClickHandler(ClickEventArgs args)
    {
        if (MemberGridRef is null)
            return;

        if (string.Equals(args.Item.Id, "Grid_excelexport", StringComparison.OrdinalIgnoreCase)
            || args.Item.Text?.Contains("Excel", StringComparison.OrdinalIgnoreCase) == true)
        {
            await MemberGridRef.ExportToExcelAsync();
            return;
        }

        if (args.Item.Id != "Delete" || NotificationRef is null)
            return;

        var selectedRecords = await MemberGridRef.GetSelectedRecordsAsync();
        NotificationRef.NotificationHeader = "Warning";
        NotificationRef.NotificationType = "Warning";
        NotificationRef.NotificationMessage = "Are you sure you want to remove the selected member from this department?";
        var confirm = await NotificationRef.ShowAlertDialog();

        var member = selectedRecords.FirstOrDefault();
        if (member != null && confirm)
            await RemoveMember(member);
    }

    Task OnRowSelectChanged(RowSelectEventArgs<MemberDto> _) => RefreshToolbarFromSelectionAsync();

    Task OnRowDeselectChanged(RowDeselectEventArgs<MemberDto> _) => RefreshToolbarFromSelectionAsync();

    public async Task RefreshToolbarFromSelectionAsync()
    {
        if (MemberGridRef is null)
            return;

        var selected = await MemberGridRef.GetSelectedRecordsAsync();
        Toolbaritems[0].Disabled = selected.Count == 0;
    }

    public void Dispose() => MemberGridRef = null;
}

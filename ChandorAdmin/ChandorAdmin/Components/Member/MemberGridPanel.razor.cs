using ChandorAdmin.Components.GlobalNotification;
using ChandorProject.Shared.DTOs.Member;
using Mapster;
using Syncfusion.Blazor.Grids;
using Syncfusion.Blazor.Navigations;

namespace ChandorAdmin.Components.Member;

public partial class MemberGridPanel
{
    public IEnumerable<MemberDetailsDto> GridData { get; set; } = Array.Empty<MemberDetailsDto>();
    public SfGrid<MemberDetailsDto>? MemberGridRef { get; set; }
    public MemberEditorDialog? DialogRef { get; set; }
    public NotificationDialog? NotificationRef { get; set; }

    public List<ItemModel> Toolbaritems { get; } =
    [
        new ItemModel { Text = "Edit", PrefixIcon = "e-edit e-icons", TooltipText = "Edit", Id = "Edit", Disabled = true },
        new ItemModel { Text = "Delete", PrefixIcon = "e-delete e-icons", TooltipText = "Delete", Id = "Delete", Disabled = true },
        new ItemModel { Text = "Excel Export", PrefixIcon = "e-excelexport e-icons", TooltipText = "ExcelExport", Id = "Grid_excelexport" }
    ];

    bool _renderGrid;

    readonly ValidationRules _rules = new() { Required = true };

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadData();
    }

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

    public async Task LoadData()
    {
        var data = await srv.GetMembersAsync();
        if (data != null && data.Success && data.Data != null)
        {
            GridData = data.Data;
            StateHasChanged();
        }
    }

    public async Task AddMember(NewMemberDto member)
    {
        var insert = await srv.AddSimpleMemberAsync(member);

        string responseMessage = string.Empty;
        string responseType = string.Empty;
        string responseHeader = string.Empty;

        if (insert is not null && NotificationRef is not null)
        {
            var status = insert.Success ? "Success" : "Error";
            var message = insert.Success ? insert.Message : $"An error occured while adding the member. \nError: {insert.Message}";
            var checkedMsg = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            responseHeader = status;
            responseType = status;
            responseMessage = insert.Success ? $"Member {member.Name} was successfully added" : checkedMsg;

            await NotificationRef.Notify(responseMessage, responseType, responseHeader);

            await LoadData();

            StateHasChanged();
        }
    }

    public async Task EditMember(UpdateMemberDto member)
    {
        var edit = await srv.UpdateMemberAsync(member);

        string responseMessage = string.Empty;
        string responseType = string.Empty;
        string responseHeader = string.Empty;

        if (edit is not null && NotificationRef is not null)
        {
            var editMem = edit.Data;
            var status = edit.Success ? "Success" : "Error";
            var message = edit.Success ? edit.Message : $"An error occured while updating the member. \nError: {edit.Message}";
            var checkedMsg = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            responseHeader = status;
            responseType = status;
            responseMessage = edit.Success ? $"Member {member.Name} was successfully updated" : checkedMsg;

            await NotificationRef.Notify(responseMessage, responseType, responseHeader);
            await LoadData();
        }
    }

    public async Task DeleteMember(MemberDetailsDto member)
    {
        var delete = await srv.DeleteMemberAsync(member.Id);

        string responseMessage = string.Empty;
        string responseType = string.Empty;
        string responseHeader = string.Empty;

        if (delete is not null && NotificationRef is not null)
        {
            var status = delete.Success ? "Success" : "Error";
            var message = delete.Success ? delete.Message : $"An error occured while deleting the member. \nError: {delete.Message}";
            var checkedMsg = string.IsNullOrWhiteSpace(message) ? string.Empty : message;
            responseHeader = status;
            responseType = status;
            responseMessage = delete.Success ? $"Member {member.Name} was successfully deleted" : checkedMsg;

            await NotificationRef.Notify(responseMessage, responseType, responseHeader);

            await LoadData();
        }
    }

    public async Task SearchAsync(string? value) => await (MemberGridRef?.SearchAsync(value ?? string.Empty) ?? Task.CompletedTask);

    public void GridRefresh(IEnumerable<MemberDetailsDto> rows)
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

        var selectedRecords = await MemberGridRef.GetSelectedRecordsAsync();

        if (args.Item.Id == "Edit" && DialogRef is not null)
        {
            if (selectedRecords.Count == 1) 
                await DialogRef.ShowEditDialog(GetMemberDto(selectedRecords[0]));
        }
        else if (args.Item.Id == "Delete" && NotificationRef is not null)
        {
            //await DialogRef.ShowAlertDialog(selectedRecords.ToList());
            NotificationRef.NotificationHeader = "Warning";
            NotificationRef.NotificationType = "Warning";
            NotificationRef.NotificationMessage = "Are you sure you want to delete the selected member?";
            var confirm = await NotificationRef.ShowAlertDialog();

            var member = selectedRecords.FirstOrDefault();

            if (member != null && confirm)
                await DeleteMember(member);
        }
    }

    Task OnRowSelectChanged(RowSelectEventArgs<MemberDetailsDto> _) => RefreshToolbarFromSelectionAsync();

    Task OnRowDeselectChanged(RowDeselectEventArgs<MemberDetailsDto> _) => RefreshToolbarFromSelectionAsync();

    public async Task RefreshToolbarFromSelectionAsync()
    {
        if (MemberGridRef is null)
            return;

        var selected = await MemberGridRef.GetSelectedRecordsAsync();
        var count = selected.Count;
        if (count > 1)
        {
            Toolbaritems[0].Disabled = true;
            Toolbaritems[1].Disabled = false;
        }
        else if (count == 0)
        {
            Toolbaritems[0].Disabled = true;
            Toolbaritems[1].Disabled = true;
        }
        else
        {
            Toolbaritems[0].Disabled = false;
            Toolbaritems[1].Disabled = false;
        }
    }

    public void Dispose() => MemberGridRef = null;

    private MemberDto GetMemberDto(MemberDetailsDto member)
    {
        return new MemberDto
        {
            Id = member.Id,
            Username = member.Username,
            Name = member.Name,
            Surname = member.Surname,
            Postname = member.Postname,
            Birthday = member.Birthday,
            Gender = member.Gender,
            Country = member.Country,
            Town = member.Town,
            Suburb = member.Suburb,
            Address = member.Address,
            Note = member.Note,
            AgeGroupId = member.AgeGroupId,
            MemberTypeId = member.MemberTypeId,
        };
    }
}
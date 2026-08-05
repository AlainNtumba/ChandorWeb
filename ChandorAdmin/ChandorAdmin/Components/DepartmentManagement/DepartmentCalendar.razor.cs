using ChandorAdmin.Data;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorProject.Shared.DTOs.DepartmentTeamDto;
using ChandorProject.Shared.DTOs.ProgramType;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;
using Syncfusion.Blazor.Schedule;

namespace ChandorAdmin.Components.DepartmentManagement;

public partial class DepartmentCalendar : IDisposable
{
    [Inject] private ILogger<DepartmentCalendar> Logger { get; set; } = default!;
    [Inject] private DepartmentCalendarDataAdaptor DataAdaptor { get; set; } = default!;
    [Inject] public IDepartmentTeamService DepartmentTeamService { get; set; } = default!;
    [Inject] public IProgramTypeService ProgramTypeService { get; set; } = default!;

    [Parameter] public Guid DepartmentId { get; set; }
    public List<ProgramTypeDto> ProgramTypes { get; set; } = new List<ProgramTypeDto>();
    public List<DepartmentTeamDto> DepartmentTeams { get; set; } = new List<DepartmentTeamDto>();

    SfDialog? _dialog;
    bool _dialogShell;
    bool _renderSchedule;
    string _dialogHeader = "Programme du département";
    string? _userMessage;
    bool _isError;

    public View CurrentView { get; set; } = View.Month;
    public DateTime SelectedDate { get; set; } = DateTime.Today;

    static readonly Dictionary<string, object> ValidationMessages = new()
    {
        { "regex", "Caractères spéciaux non autorisés dans ce champ" }
    };

    ValidationRules ValidationRules { get; set; } = new() { Required = true };
    readonly ValidationRules LocationValidationRules = new()
    {
        Required = true,
        RegexPattern = "^[A-Za-z-0-9-,()-/&' ]{5,80}$",
        Messages = ValidationMessages
    };
    readonly ValidationRules DescriptionValidationRules = new()
    {
        Required = true,
        MinLength = 5,
        MaxLength = 500
    };

    public async Task ShowAsync(Guid departmentId, string? departmentName = null)
    {
        DepartmentId = departmentId;
        DataAdaptor.DepartmentId = departmentId;

        _dialogHeader = string.IsNullOrWhiteSpace(departmentName)
            ? "Programme du département"
            : $"Programme — {departmentName.Trim()}";

        _userMessage = null;
        _isError = false;
        CurrentView = View.Month;
        SelectedDate = DateTime.Today;

        var _programType = await ProgramTypeService.GetAllProgramTypesAsync();
        ProgramTypes = _programType?.Data?.ToList() ?? new List<ProgramTypeDto>();
        var _departmentTeams = await DepartmentTeamService.GetDepartmentTeamsByDepartmentIdAsync(DepartmentId);
        DepartmentTeams = _departmentTeams?.Data?.ToList() ?? new List<DepartmentTeamDto>();

        if (!_dialogShell)
        {
            _dialogShell = true;
            await InvokeAsync(StateHasChanged);
            await Task.Yield();
        }

        // Remount schedule so a different department never shows stale events.
        _renderSchedule = false;
        await InvokeAsync(StateHasChanged);
        await Task.Yield();

        if (_dialog is not null)
            await _dialog.ShowAsync();
    }

    async Task OnDialogOpenedAsync(OpenEventArgs _)
    {
        DataAdaptor.DepartmentId = DepartmentId;

        if (_renderSchedule)
            return;

        _renderSchedule = true;
        await InvokeAsync(StateHasChanged);
    }

    Task OnDialogClosedAsync(CloseEventArgs _)
    {
        _renderSchedule = false;
        return Task.CompletedTask;
    }

    void OnScheduleActionFailure(ActionEventArgs<ChurchProgramDto> args)
    {
        var ex = args.Error;
        if (ex is not null)
            Logger.LogError(ex, "Department calendar OnActionFailure. Action: {Action}", args.ActionType);

        _isError = true;
        _userMessage = ex?.Message ?? "Une erreur s'est produite sur le calendrier.";
        StateHasChanged();
    }

    void OnScheduleActionCompleted(ActionEventArgs<ChurchProgramDto> args)
    {
        if (args.ActionType is ActionType.EventCreate or ActionType.EventChange or ActionType.EventRemove)
        {
            _isError = false;
            _userMessage = null;
            StateHasChanged();
        }
    }

    public void Dispose() => _dialog = null;
}

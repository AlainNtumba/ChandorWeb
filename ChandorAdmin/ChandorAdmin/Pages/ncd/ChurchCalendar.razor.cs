using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Schedule;
using ChandorProject.Shared.DTOs.ChurchProgram;
using ChandorAdmin.Services;
using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.DTOs.DepartmentTeamDto;
using ChandorProject.Shared.DTOs.ProgramType;
using Microsoft.JSInterop;

namespace ChandorAdmin.Pages.ncd;

public partial class ChurchCalendar
{
    [Inject] private ILogger<ChurchCalendar> Logger { get; set; } = default!;
    [Inject] private ChurchProgramPosterEditorState PosterEditor { get; set; } = default!;
    [Inject] private IProgramTypeService ProgramTypeService { get; set; } = default!;
    [Inject] private IDepartmentService DepartmentService { get; set; } = default!;
    [Inject] private IDepartmentTeamService DepartmentTeamService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private string? _userMessage;
    private bool _isError;
    private bool _lookupsLoading;
    private bool _teamsLoading;
    private string? _lookupError;
    private IReadOnlyList<ProgramTypeDto> _programTypes = [];
    private IReadOnlyList<DepartmentDto> _departments = [];
    private IReadOnlyList<DepartmentTeamDto> _departmentTeams = [];

    public View CurrentView { get; set; } = View.Month;
    public DateTime SelectedDate { get; set; } = DateTime.Today;

    // Form validation
    static readonly Dictionary<string, object> ValidationMessages = new() { { "regex", "Caractères spéciaux non autorisés dans ce champ" } };
    ValidationRules ValidationRules { get; set; } = new ValidationRules { Required = true };
    readonly ValidationRules LocationValidationRules = new ValidationRules { Required = true, RegexPattern = "^[A-Za-z-0-9-,()-/&' ]{5,80}$", Messages = ValidationMessages };
    readonly ValidationRules DescriptionValidationRules = new ValidationRules { Required = true, MinLength = 5, MaxLength = 500 };

    private async Task OnSchedulePopupOpen(PopupOpenEventArgs<ChurchProgramDto> args)
    {
        if (args.Type != PopupType.Editor || args.Data is null)
            return;

        PosterEditor.Begin(args.Data);
        await LoadProgramLookupsAsync();
        await LoadDepartmentTeamsAsync(args.Data.DepartmentId, preserveTeamId: args.Data.DepartmentTeamId);
        if (args.Data.DepartmentTeamId != Guid.Empty
            && _departmentTeams.All(item => item.Id != args.Data.DepartmentTeamId))
        {
            args.Data.DepartmentTeamId = Guid.Empty;
        }
    }

    private async Task LoadProgramLookupsAsync()
    {
        if (_programTypes.Count > 0 && _departments.Count > 0)
            return;

        _lookupsLoading = true;
        _lookupError = null;
        try
        {
            var programTypesTask = ProgramTypeService.GetAllProgramTypesAsync();
            var departmentsTask = DepartmentService.GetDepartmentsAsync();
            await Task.WhenAll(programTypesTask, departmentsTask);

            var programTypes = await programTypesTask;
            var departments = await departmentsTask;
            if (programTypes is null || !programTypes.Success)
                throw new InvalidOperationException(programTypes?.Message ?? "Impossible de charger les types de programme.");
            if (departments is null || !departments.Success)
                throw new InvalidOperationException(departments?.Message ?? "Impossible de charger les départements.");

            _programTypes = programTypes.Data?.OrderBy(item => item.Name).ToList() ?? [];
            _departments = departments.Data?.OrderBy(item => item.Name).ToList() ?? [];
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unable to load ChurchProgram editor lookups.");
            _lookupError = ex.Message;
        }
        finally
        {
            _lookupsLoading = false;
        }
    }

    private async Task OnDepartmentChangedAsync(ChurchProgramDto program, ChangeEventArgs args)
    {
        program.DepartmentId = Guid.TryParse(args.Value?.ToString(), out var departmentId)
            ? departmentId
            : Guid.Empty;
        program.DepartmentTeamId = Guid.Empty;
        await LoadDepartmentTeamsAsync(program.DepartmentId);
    }

    private async Task LoadDepartmentTeamsAsync(Guid departmentId, Guid preserveTeamId = default)
    {
        _departmentTeams = [];
        if (departmentId == Guid.Empty)
            return;

        _teamsLoading = true;
        _lookupError = null;
        try
        {
            var response = await DepartmentTeamService.GetDepartmentTeamsByDepartmentIdAsync(departmentId);
            if (response is null || !response.Success)
                throw new InvalidOperationException(response?.Message ?? "Impossible de charger les équipes du département.");

            _departmentTeams = response.Data?.OrderBy(item => item.Name).ToList() ?? [];
            if (preserveTeamId != Guid.Empty && _departmentTeams.All(item => item.Id != preserveTeamId))
                _lookupError = "L’équipe enregistrée n’appartient plus au département sélectionné.";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unable to load teams for department {DepartmentId}.", departmentId);
            _lookupError = ex.Message;
        }
        finally
        {
            _teamsLoading = false;
        }
    }

    private async Task OnScheduleActionBegin(ActionEventArgs<ChurchProgramDto> args)
    {
        if (args.ActionType is ActionType.EventCreate or ActionType.EventChange)
            await SetSaveLoadingAsync(true);
    }

    private async Task OnScheduleActionFailure(ActionEventArgs<ChurchProgramDto> args)
    {
        var ex = args.Error;
        if (ex is not null)
            Logger.LogError(ex, "Calendar OnActionFailure. Action: {Action}", args.ActionType);
        _isError = true;
        _userMessage = ex?.Message
            ?? "Une erreur s'est produite sur le calendrier.";
        await SetSaveLoadingAsync(false);
        StateHasChanged();
    }

    private async Task OnScheduleActionCompleted(ActionEventArgs<ChurchProgramDto> args)
    {
        if (args.ActionType is ActionType.EventCreate or ActionType.EventChange or ActionType.EventRemove)
        {
            _isError = false;
            _userMessage = null;
            await SetSaveLoadingAsync(false);
            StateHasChanged();
        }
    }

    private async Task SetSaveLoadingAsync(bool isLoading)
    {
        try
        {
            await JS.InvokeVoidAsync("chandorCalendar.setSaveLoading", isLoading);
        }
        catch (JSException ex)
        {
            Logger.LogWarning(ex, "Unable to update the Scheduler save button loading state.");
        }
    }
}

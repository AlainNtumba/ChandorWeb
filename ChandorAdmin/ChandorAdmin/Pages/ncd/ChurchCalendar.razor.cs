using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Schedule;
using ChandorProject.Shared.DTOs.ChurchProgram;

namespace ChandorAdmin.Pages.ncd;

public partial class ChurchCalendar
{
    [Inject] private ILogger<ChurchCalendar> Logger { get; set; } = default!;

    private string? _userMessage;
    private bool _isError;
    private string? _editorMessage;
    private string _editorTab = "INFORMATION";

    public View CurrentView { get; set; } = View.Month;
    public DateTime SelectedDate { get; set; } = DateTime.Today;

    // Form validation
    static readonly Dictionary<string, object> ValidationMessages = new() { { "regex", "Caractères spéciaux non autorisés dans ce champ" } };
    ValidationRules ValidationRules { get; set; } = new ValidationRules { Required = true };
    readonly ValidationRules LocationValidationRules = new ValidationRules { Required = true, RegexPattern = "^[A-Za-z-0-9-,()-/&' ]{5,80}$", Messages = ValidationMessages };
    readonly ValidationRules DescriptionValidationRules = new ValidationRules { Required = true, MinLength = 5, MaxLength = 500 };

    private string EditorTabClass(string tab) =>
        _editorTab == tab ? "church-program-editor__tab active" : "church-program-editor__tab";

    private void SelectEditorTab(string tab)
    {
        _editorTab = tab;
        _editorMessage = null;
    }

    private void OnSchedulePopupOpen(PopupOpenEventArgs<ChurchProgramDto> args)
    {
        if (args.Type != PopupType.Editor || args.Data is null)
            return;

        _editorTab = "INFORMATION";
        _editorMessage = null;
        PosterEditor.Begin(args.Data);
    }

    private void OnSchedulePopupClose(PopupCloseEventArgs<ChurchProgramDto> args)
    {
        if (args.Type == PopupType.Editor && args.CurrentAction == CurrentAction.Cancel)
        {
            PosterEditor.Reset();
            _editorMessage = null;
            _editorTab = "INFORMATION";
        }
    }

    private void OnScheduleActionBegin(ActionEventArgs<ChurchProgramDto> args)
    {
        if (args.ActionType == ActionType.EventCreate && PosterEditor.SelectedPoster is null)
        {
            args.Cancel = true;
            _editorTab = "POSTER";
            _editorMessage = "Le poster est obligatoire pour créer un programme.";
        }
    }

    private void OnScheduleActionFailure(ActionEventArgs<ChurchProgramDto> args)
    {
        var ex = args.Error;
        if (ex is not null)
            Logger.LogError(ex, "Calendar OnActionFailure. Action: {Action}", args.ActionType);
        _isError = true;
        _editorMessage = ex?.Message;
        _userMessage = ex?.Message
            ?? "Une erreur s'est produite sur le calendrier.";
        StateHasChanged();
    }

    private void OnScheduleActionCompleted(ActionEventArgs<ChurchProgramDto> args)
    {
        if (args.ActionType is ActionType.EventCreate or ActionType.EventChange or ActionType.EventRemove)
        {
            _isError = false;
            _userMessage = null;
            _editorMessage = null;
            PosterEditor.Reset();
            StateHasChanged();
        }
    }
}

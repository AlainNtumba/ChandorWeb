using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using Syncfusion.Blazor.Inputs;
using Syncfusion.Blazor.Schedule;
using ChandorProject.Shared.DTOs.ChurchProgram;

namespace ChandorAdmin.Pages.ncd;

public partial class ChurchCalendar
{
    [Inject] private ILogger<ChurchCalendar> Logger { get; set; } = default!;

    private string? _userMessage;
    private bool _isError;

    public View CurrentView { get; set; } = View.Month;
    public DateTime SelectedDate { get; set; } = DateTime.Today;

    // Form validation
    static readonly Dictionary<string, object> ValidationMessages = new() { { "regex", "Caractères spéciaux non autorisés dans ce champ" } };
    ValidationRules ValidationRules { get; set; } = new ValidationRules { Required = true };
    readonly ValidationRules LocationValidationRules = new ValidationRules { Required = true, RegexPattern = "^[A-Za-z-0-9-,()-/& ]{5,80}$", Messages = ValidationMessages };
    readonly ValidationRules DescriptionValidationRules = new ValidationRules { Required = true, MinLength = 5, MaxLength = 500 };

    private void OnScheduleActionFailure(ActionEventArgs<ChurchProgramDto> args)
    {
        var ex = args.Error;
        if (ex is not null)
            Logger.LogError(ex, "Calendar OnActionFailure. Action: {Action}", args.ActionType);
        _isError = true;
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
            StateHasChanged();
        }
    }
}

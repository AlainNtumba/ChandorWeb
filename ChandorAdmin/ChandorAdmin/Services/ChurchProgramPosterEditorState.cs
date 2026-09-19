using ChandorAdmin.Models.ChurchProgram;
using ChandorProject.Shared.DTOs.ChurchProgram;

namespace ChandorAdmin.Services;

/// <summary>
/// Holds poster changes while the Scheduler editor is open. The API is called
/// only when the Scheduler saves the complete event.
/// </summary>
public sealed class ChurchProgramPosterEditorState
{
    public const long MaximumFileSize = 5 * 1024 * 1024;

    public Guid ProgramId { get; private set; }
    public string ExistingPosterLink { get; private set; } = string.Empty;
    public ChurchProgramPosterUpload? SelectedPoster { get; private set; }
    public bool DeleteExistingPoster { get; private set; }

    public void Begin(ChurchProgramDto program)
    {
        ProgramId = program.Id;
        ExistingPosterLink = program.PosterLink ?? string.Empty;
        SelectedPoster = null;
        DeleteExistingPoster = false;
    }

    public void Select(ChurchProgramPosterUpload poster)
    {
        SelectedPoster = poster;
        DeleteExistingPoster = false;
    }

    public void RemoveSelected() => SelectedPoster = null;

    public void MarkExistingForDeletion()
    {
        SelectedPoster = null;
        DeleteExistingPoster = !string.IsNullOrWhiteSpace(ExistingPosterLink);
    }

    public void CancelDeletion() => DeleteExistingPoster = false;

    public void Reset()
    {
        ProgramId = Guid.Empty;
        ExistingPosterLink = string.Empty;
        SelectedPoster = null;
        DeleteExistingPoster = false;
    }
}

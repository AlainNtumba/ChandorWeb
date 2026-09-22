using ChandorAdmin.Interfaces.Api;
using ChandorProject.Shared.DTOs.Bible;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Pages.ncd;

public partial class BibleManagement : IDisposable
{
    [Inject] private IBibleVerseApiService BibleService { get; set; } = default!;
    [Inject] private SfDialogService DialogService { get; set; } = default!;

    private BibleVersePageDto _page = new();
    private int _requestedPage = 1;
    private int _pageSize = 20;
    private bool _loading;
    private bool _saving;
    private bool _dialogOpen;
    private string _dialogKind = string.Empty;
    private string _dialogTitle = string.Empty;
    private string? _dialogError;
    private string? _notice;
    private bool _noticeIsError;
    private Guid? _editingId;
    private BibleVerseDto? _selectedVerse;
    private CreateBibleVerseDto _formModel = NewModel();
    private CancellationTokenSource? _loadCts;

    protected override Task OnInitializedAsync() => LoadPageAsync();

    private async Task LoadPageAsync()
    {
        _loadCts?.Cancel(); _loadCts?.Dispose(); _loadCts = new CancellationTokenSource();
        _loading = true;
        try
        {
            var response = await BibleService.GetAllAsync(_requestedPage, _pageSize, _loadCts.Token);
            if (response is { Success: true, Data: not null }) _page = response.Data;
            else ShowError(response, "Impossible de charger les versets.");
        }
        catch (OperationCanceledException) { }
        catch (Exception) { Error("Impossible de joindre l’API. Vérifiez la connexion puis réessayez."); }
        finally { _loading = false; }
    }

    private async Task ChangePageAsync(int page)
    {
        if (page < 1 || page == _requestedPage) return;
        _requestedPage = page;
        await LoadPageAsync();
    }

    private async Task ChangePageSizeAsync()
    {
        _requestedPage = 1;
        await LoadPageAsync();
    }

    private void OpenCreate()
    {
        _editingId = null; _selectedVerse = null; _formModel = NewModel(); _dialogError = null;
        _dialogKind = "CREATE"; _dialogTitle = "Nouveau verset"; _dialogOpen = true;
    }

    private async Task<BibleVerseDto?> LoadDetailAsync(Guid id)
    {
        try
        {
            var response = await BibleService.GetByIdAsync(id);
            if (response is { Success: true, Data: not null }) return response.Data;
            ShowError(response, "Impossible de charger ce verset.");
        }
        catch (Exception) { Error("Impossible de joindre l’API."); }
        return null;
    }

    private async Task OpenDetailAsync(BibleVerseDto item)
    {
        var detail = await LoadDetailAsync(item.Id); if (detail is null) return;
        _selectedVerse = detail; _dialogKind = "DETAIL"; _dialogTitle = Reference(detail); _dialogOpen = true;
    }

    private async Task OpenEditAsync(BibleVerseDto item)
    {
        var detail = await LoadDetailAsync(item.Id); if (detail is null) return;
        BeginEdit(detail);
    }

    private void BeginEdit(BibleVerseDto detail)
    {
        _selectedVerse = detail; _editingId = detail.Id; _formModel = MapToForm(detail); _dialogError = null;
        _dialogKind = "EDIT"; _dialogTitle = $"Modifier {Reference(detail)}"; _dialogOpen = true;
    }

    private async Task SaveAsync()
    {
        if (_saving) return;
        _saving = true; _dialogError = null;
        try
        {
            DataResponse<BibleVerseDto>? response = _editingId.HasValue
                ? await BibleService.UpdateAsync(_editingId.Value, new UpdateBibleVerseDto
                {
                    BookName = _formModel.BookName.Trim(), Chapter = _formModel.Chapter, Verse = _formModel.Verse.Trim(),
                    Content = _formModel.Content.Trim(), DailyThought = _formModel.DailyThought.Trim(), PublicationDate = _formModel.PublicationDate
                })
                : await BibleService.CreateAsync(new CreateBibleVerseDto
                {
                    BookName = _formModel.BookName.Trim(), Chapter = _formModel.Chapter, Verse = _formModel.Verse.Trim(),
                    Content = _formModel.Content.Trim(), DailyThought = _formModel.DailyThought.Trim(), PublicationDate = _formModel.PublicationDate
                });

            if (response is not { Success: true, Data: not null })
            {
                _dialogError = ResponseMessage(response, "Impossible d’enregistrer le verset.");
                return;
            }

            Success(_editingId.HasValue ? "Verset modifié." : "Verset créé.");
            _saving = false;
            CloseDialog();
            await LoadPageAsync();
        }
        catch (Exception) { _dialogError = "Impossible de joindre l’API. Vérifiez votre connexion puis réessayez."; }
        finally { _saving = false; }
    }

    private async Task DeleteAsync(BibleVerseDto item)
    {
        if (_saving) return;
        var confirmed = await DialogService.ConfirmAsync($"Supprimer « {Reference(item)} » programmé le {item.PublicationDate:dd/MM/yyyy} ? Cette action est irréversible.", "Confirmation");
        if (!confirmed) return;
        _saving = true;
        try
        {
            var response = await BibleService.DeleteAsync(item.Id);
            if (response is not { Success: true }) { ShowError(response, "Impossible de supprimer le verset."); return; }
            if (_page.Items.Count == 1 && _requestedPage > 1) _requestedPage--;
            Success("Verset supprimé.");
            await LoadPageAsync();
        }
        catch (Exception) { Error("Impossible de joindre l’API."); }
        finally { _saving = false; }
    }

    private void CloseDialog()
    {
        if (_saving) return;
        _dialogOpen = false; _dialogKind = string.Empty; _dialogError = null; _editingId = null; _selectedVerse = null;
    }

    private static CreateBibleVerseDto NewModel() => new() { Chapter = 1, PublicationDate = DateOnly.FromDateTime(DateTime.Today) };
    private static CreateBibleVerseDto MapToForm(BibleVerseDto item) => new() { BookName = item.BookName, Chapter = item.Chapter, Verse = item.Verse, Content = item.Content, DailyThought = item.DailyThought, PublicationDate = item.PublicationDate };
    private static string Reference(BibleVerseDto item) => string.IsNullOrWhiteSpace(item.Reference) ? $"{item.BookName} {item.Chapter}:{item.Verse}" : item.Reference;
    private static string LocalDate(DateTime value) => value == default ? "—" : value.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
    private void ShowError<T>(DataResponse<T>? response, string fallback) => Error(ResponseMessage(response, fallback));
    private static string ResponseMessage<T>(DataResponse<T>? response, string fallback) { if (!string.IsNullOrWhiteSpace(response?.Message)) return response.Message; var errors = string.Join(" ", response?.Error?.Where(x => !string.IsNullOrWhiteSpace(x)) ?? []); return string.IsNullOrWhiteSpace(errors) ? fallback : errors; }
    private void Error(string message) { _notice = message; _noticeIsError = true; }
    private void Success(string message) { _notice = message; _noticeIsError = false; }
    public void Dispose() { _loadCts?.Cancel(); _loadCts?.Dispose(); }
}

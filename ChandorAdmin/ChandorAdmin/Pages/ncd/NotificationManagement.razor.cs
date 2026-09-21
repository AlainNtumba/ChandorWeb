using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.Notification;
using ChandorProject.Shared.DTOs.AgeGroup;
using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.DTOs.DepartmentTeamDto;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.DTOs.MemberType;
using ChandorProject.Shared.DTOs.Notification;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Pages.ncd;

public partial class NotificationManagement : IDisposable
{
    [Inject] private INotificationService NotificationService { get; set; } = default!;
    [Inject] private IMemberService MemberService { get; set; } = default!;
    [Inject] private IMemberTypeService MemberTypeService { get; set; } = default!;
    [Inject] private IAgeGroupService AgeGroupService { get; set; } = default!;
    [Inject] private IDepartmentService DepartmentService { get; set; } = default!;
    [Inject] private IDepartmentTeamService DepartmentTeamService { get; set; } = default!;
    [Inject] private SfDialogService DialogService { get; set; } = default!;

    private static readonly string[] Statuses = ["DRAFT", "PUBLISHED", "ARCHIVED"];
    private static readonly string[] Audiences = ["GLOBAL", "SPECIFIC", "MEMBER_TYPE", "AGE_GROUP", "DEPARTMENT_TEAM"];
    private static readonly string[] RecipientStates = ["READ", "UNREAD", "DISMISSED"];
    private readonly NotificationAdminFilterState _filter = new();
    private readonly NotificationRecipientFilterState _recipientFilter = new();
    private PagedResult<NotificationAdminDto> _notificationPage = new();
    private PagedResult<NotificationRecipientAdminDto> _recipientPage = new();
    private NotificationSummaryDto _summary = new();
    private IReadOnlyList<MemberDto> _members = [];
    private IReadOnlyList<MemberTypeDto> _memberTypes = [];
    private IReadOnlyList<AgeGroupDto> _ageGroups = [];
    private IReadOnlyList<DepartmentDto> _departments = [];
    private IReadOnlyList<DepartmentTeamDto> _teams = [];
    private DateTime? _summaryFrom;
    private DateTime? _summaryTo;
    private int _selectedTab;
    private bool _loading;
    private bool _saving;
    private bool _dialogOpen;
    private string _dialogKind = string.Empty;
    private string _dialogTitle = string.Empty;
    private string _editorTab = "CONTENT";
    private string? _notice;
    private bool _noticeIsError;
    private CancellationTokenSource? _searchCts;
    private Guid? _editingId;
    private NotificationAdminDto? _selectedNotification;
    private NotificationInputDto _model = NewModel();
    private Guid? _teamDepartmentId;
    private string _memberSearch = string.Empty;

    private IEnumerable<MemberDto> FilteredMembers => _members.Where(x => string.IsNullOrWhiteSpace(_memberSearch) || MemberName(x).Contains(_memberSearch, StringComparison.OrdinalIgnoreCase) || x.Username.Contains(_memberSearch, StringComparison.OrdinalIgnoreCase));
    private IEnumerable<DepartmentTeamDto> FilteredTeams => _teams.Where(x => !_teamDepartmentId.HasValue || x.DepartmentId == _teamDepartmentId.Value);

    protected override async Task OnInitializedAsync() => await LoadInitialAsync();

    private async Task LoadInitialAsync()
    {
        _loading = true;
        try
        {
            await Task.WhenAll(LoadSummaryCoreAsync(), LoadNotificationsCoreAsync(), LoadMembersAsync(), LoadMemberTypesAsync(), LoadAgeGroupsAsync());
            await LoadDepartmentsAndTeamsAsync();
        }
        finally { _loading = false; }
    }

    private async Task LoadSummaryCoreAsync(CancellationToken token = default)
    {
        var response = await NotificationService.GetAdminSummaryAsync(_summaryFrom, _summaryTo, token);
        if (response is { Success: true, Data: not null }) _summary = response.Data;
        else ShowError(response, "Impossible de charger les statistiques des notifications.");
    }

    private async Task LoadNotificationsCoreAsync(CancellationToken token = default)
    {
        var response = await NotificationService.GetAdminAsync(_filter, token);
        if (response is { Success: true, Data: not null }) _notificationPage = response.Data;
        else ShowError(response, "Impossible de charger les notifications.");
    }

    private async Task LoadMembersAsync()
    {
        var response = await MemberService.GetAllMembersAsync();
        if (response is { Success: true, Data: not null }) _members = response.Data.OrderBy(x => x.Name).ThenBy(x => x.Surname).ToList();
    }

    private async Task LoadMemberTypesAsync()
    {
        var response = await MemberTypeService.GetAllMemberTypesAsync();
        if (response is { Success: true, Data: not null }) _memberTypes = response.Data.OrderBy(x => x.Name).ToList();
    }

    private async Task LoadAgeGroupsAsync()
    {
        var response = await AgeGroupService.GetAllAgeGroupsAsync();
        if (response is { Success: true, Data: not null }) _ageGroups = response.Data.OrderBy(x => x.FromAge).ToList();
    }

    private async Task LoadDepartmentsAndTeamsAsync()
    {
        var response = await DepartmentService.GetDepartmentsAsync();
        if (response is not { Success: true, Data: not null }) return;
        _departments = response.Data.OrderBy(x => x.Name).ToList();
        var results = await Task.WhenAll(_departments.Select(x => DepartmentTeamService.GetDepartmentTeamsByDepartmentIdAsync(x.Id)));
        _teams = results.Where(x => x is { Success: true, Data: not null }).SelectMany(x => x!.Data!).OrderBy(x => x.Name).ToList();
    }

    private async Task RefreshSelectedAsync()
    {
        _loading = true;
        try { if (_selectedTab == 0) await LoadSummaryCoreAsync(); else await LoadNotificationsCoreAsync(); }
        finally { _loading = false; }
    }

    private Task OnKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _filter.Keyword = value, args.Value?.ToString(), () => { _filter.Page = 1; return LoadNotificationsCoreAsync(_searchCts!.Token); });
    private Task OnRecipientKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _recipientFilter.Keyword = value, args.Value?.ToString(), () => { _recipientFilter.Page = 1; return LoadRecipientsCoreAsync(_searchCts!.Token); });
    private async Task DebounceAsync(Action<string> setter, string? value, Func<Task> loader)
    {
        setter(value ?? string.Empty); _searchCts?.Cancel(); _searchCts?.Dispose(); _searchCts = new CancellationTokenSource();
        try { await Task.Delay(400, _searchCts.Token); await loader(); await InvokeAsync(StateHasChanged); } catch (OperationCanceledException) { }
    }

    private async Task ApplyFiltersAsync() { _filter.Page = 1; await LoadNotificationsCoreAsync(); }
    private async Task ApplySummaryPeriodAsync() => await LoadSummaryCoreAsync();
    private async Task ResetFiltersAsync()
    {
        _filter.Status = null; _filter.AudienceType = null; _filter.Keyword = string.Empty; _filter.FromDate = null; _filter.ToDate = null; _filter.IsActive = null; _filter.Page = 1;
        await LoadNotificationsCoreAsync();
    }

    private void OpenCreate()
    {
        _editingId = null; _model = NewModel(); _editorTab = "CONTENT"; _memberSearch = string.Empty; _teamDepartmentId = null;
        _dialogKind = "EDITOR"; _dialogTitle = "Nouvelle notification"; _dialogOpen = true;
    }

    private async Task<NotificationAdminDto?> LoadDetailAsync(Guid id)
    {
        var response = await NotificationService.GetAdminByIdAsync(id);
        if (response is { Success: true, Data: not null }) return response.Data;
        ShowError(response, "Impossible de charger cette notification."); return null;
    }

    private async Task OpenViewAsync(NotificationAdminDto row)
    {
        var detail = await LoadDetailAsync(row.Id); if (detail is null) return;
        _selectedNotification = detail; _dialogKind = "DETAIL"; _dialogTitle = detail.Title; _dialogOpen = true;
    }

    private async Task OpenEditAsync(NotificationAdminDto row)
    {
        var detail = await LoadDetailAsync(row.Id); if (detail is null) return;
        _editingId = detail.Id; _selectedNotification = detail; _editorTab = "CONTENT"; _memberSearch = string.Empty; _teamDepartmentId = null;
        _model = new NotificationInputDto
        {
            Title = detail.Title, Message = detail.Message, AudienceType = Normalize(detail.AudienceType), Status = Normalize(detail.Status),
            PublishedAt = ToLocalEditorTime(detail.PublishedAt), ExpiresAt = ToLocalEditorTime(detail.ExpiresAt), IsActive = detail.IsActive,
            MemberIds = [.. detail.MemberIds], MemberTypeIds = [.. detail.MemberTypeIds], AgeGroupIds = [.. detail.AgeGroupIds], DepartmentTeamIds = [.. detail.DepartmentTeamIds]
        };
        _dialogKind = "EDITOR"; _dialogTitle = "Modifier la notification"; _dialogOpen = true;
    }

    private async Task OpenRecipientsAsync(NotificationAdminDto row)
    {
        _selectedNotification = row; _recipientFilter.State = null; _recipientFilter.Keyword = string.Empty; _recipientFilter.Page = 1;
        await LoadRecipientsCoreAsync(); _dialogKind = "RECIPIENTS"; _dialogTitle = $"Destinataires — {row.Title}"; _dialogOpen = true;
    }

    private async Task LoadRecipientsCoreAsync(CancellationToken token = default)
    {
        if (_selectedNotification is null) return;
        var response = await NotificationService.GetRecipientsAsync(_selectedNotification.Id, _recipientFilter, token);
        if (response is { Success: true, Data: not null }) _recipientPage = response.Data;
        else ShowError(response, "Impossible de charger les destinataires.");
    }

    private async Task OnRecipientStateChangedAsync()
    {
        _recipientFilter.Page = 1;
        await LoadRecipientsCoreAsync();
    }

    private void OnAudienceChanged(ChangeEventArgs args)
    {
        _model.AudienceType = Normalize(args.Value?.ToString() ?? "GLOBAL");
        _model.MemberIds.Clear(); _model.MemberTypeIds.Clear(); _model.AgeGroupIds.Clear(); _model.DepartmentTeamIds.Clear();
    }

    private static void ToggleSelection(List<Guid> values, Guid id, bool selected)
    {
        if (selected && !values.Contains(id)) values.Add(id); else if (!selected) values.Remove(id);
    }

    private async Task SaveAsync()
    {
        if (_saving) return; _saving = true;
        try
        {
            if (!ValidateModel()) return;
            ClearUnrelatedTargets();
            var apiModel = BuildApiModel();
            var response = _editingId.HasValue ? await NotificationService.UpdateAdminAsync(_editingId.Value, apiModel) : await NotificationService.CreateAdminAsync(apiModel);
            if (response is not { Success: true }) { ShowError(response, "Impossible d’enregistrer la notification."); return; }
            Success("Notification enregistrée."); CloseDialogCore(); await ReloadNotificationsAsync();
        }
        finally { _saving = false; }
    }

    private bool ValidateModel()
    {
        if (string.IsNullOrWhiteSpace(_model.Title) || string.IsNullOrWhiteSpace(_model.Message)) { Error("Le titre et le message sont obligatoires."); return false; }
        if (_model.ExpiresAt.HasValue && _model.PublishedAt.HasValue && _model.ExpiresAt <= _model.PublishedAt) { Error("La date d’expiration doit être postérieure à la publication."); return false; }
        var hasTarget = _model.AudienceType switch { "GLOBAL" => true, "SPECIFIC" => _model.MemberIds.Count > 0, "MEMBER_TYPE" => _model.MemberTypeIds.Count > 0, "AGE_GROUP" => _model.AgeGroupIds.Count > 0, "DEPARTMENT_TEAM" => _model.DepartmentTeamIds.Count > 0, _ => false };
        if (!hasTarget) { Error("Sélectionnez au moins un destinataire pour cette audience."); return false; }
        return true;
    }

    private void ClearUnrelatedTargets()
    {
        if (_model.AudienceType != "SPECIFIC") _model.MemberIds.Clear();
        if (_model.AudienceType != "MEMBER_TYPE") _model.MemberTypeIds.Clear();
        if (_model.AudienceType != "AGE_GROUP") _model.AgeGroupIds.Clear();
        if (_model.AudienceType != "DEPARTMENT_TEAM") _model.DepartmentTeamIds.Clear();
    }

    private NotificationInputDto BuildApiModel() => new()
    {
        Title = _model.Title.Trim(),
        Message = _model.Message.Trim(),
        AudienceType = Normalize(_model.AudienceType),
        Status = Normalize(_model.Status),
        PublishedAt = ToUtcApiTime(_model.PublishedAt),
        ExpiresAt = ToUtcApiTime(_model.ExpiresAt),
        IsActive = _model.IsActive,
        MemberIds = [.. _model.MemberIds],
        MemberTypeIds = [.. _model.MemberTypeIds],
        AgeGroupIds = [.. _model.AgeGroupIds],
        DepartmentTeamIds = [.. _model.DepartmentTeamIds]
    };

    private static DateTime? ToLocalEditorTime(DateTime? value)
    {
        if (!value.HasValue) return null;
        var utc = value.Value.Kind == DateTimeKind.Utc
            ? value.Value
            : DateTime.SpecifyKind(value.Value, DateTimeKind.Utc);
        return utc.ToLocalTime();
    }

    private static DateTime? ToUtcApiTime(DateTime? value)
    {
        if (!value.HasValue) return null;
        var local = value.Value.Kind == DateTimeKind.Local
            ? value.Value
            : DateTime.SpecifyKind(value.Value, DateTimeKind.Local);
        return local.ToUniversalTime();
    }

    private Task PublishAsync(NotificationAdminDto item) => ChangeLifecycleAsync(item, "PUBLISH");
    private Task MoveToDraftAsync(NotificationAdminDto item) => ChangeLifecycleAsync(item, "DRAFT");
    private Task ArchiveAsync(NotificationAdminDto item) => ChangeLifecycleAsync(item, "ARCHIVE");
    private async Task ChangeLifecycleAsync(NotificationAdminDto item, string action)
    {
        var label = action == "PUBLISH" ? "publier" : action == "DRAFT" ? "remettre en brouillon" : "archiver";
        if (!await DialogService.ConfirmAsync($"Voulez-vous {label} cette notification ?", "Confirmation")) return;
        var response = action switch { "PUBLISH" => await NotificationService.PublishAsync(item.Id), "DRAFT" => await NotificationService.MoveToDraftAsync(item.Id), _ => await NotificationService.ArchiveAsync(item.Id) };
        if (response is { Success: true }) { Success("Statut de la notification mis à jour."); await ReloadNotificationsAsync(); }
        else ShowError(response, "Impossible de modifier le statut de la notification.");
    }

    private async Task DeleteAsync(NotificationAdminDto item)
    {
        if (!await DialogService.ConfirmAsync($"Supprimer définitivement la notification « {item.Title} » ?", "Confirmation")) return;
        var response = await NotificationService.DeleteAdminAsync(item.Id);
        if (response is { Success: true }) { Success("Notification supprimée."); await ReloadNotificationsAsync(); }
        else ShowError(response, "Impossible de supprimer la notification.");
    }

    private async Task ReloadNotificationsAsync() => await Task.WhenAll(LoadNotificationsCoreAsync(), LoadSummaryCoreAsync());
    private async Task ChangePageAsync(int page) { if (page < 1) return; _filter.Page = page; await LoadNotificationsCoreAsync(); }
    private async Task ChangeRecipientPageAsync(int page) { if (page < 1) return; _recipientFilter.Page = page; await LoadRecipientsCoreAsync(); }
    private void CloseDialog() { if (!_saving) CloseDialogCore(); }
    private void CloseDialogCore() { _dialogOpen = false; _dialogKind = string.Empty; _editingId = null; _selectedNotification = null; }
    private static NotificationInputDto NewModel() => new() { AudienceType = "GLOBAL", Status = "DRAFT", IsActive = true };
    private static string Normalize(string value) => value.Trim().Replace("-", "_").Replace(" ", "_").ToUpperInvariant();
    private static string StatusLabel(string value) => Normalize(value) switch { "DRAFT" => "Brouillon", "PUBLISHED" => "Publiée", "ARCHIVED" => "Archivée", _ => value };
    private static string AudienceLabel(string value) => Normalize(value) switch { "GLOBAL" => "Tous les membres", "SPECIFIC" => "Membres précis", "MEMBER_TYPE" => "Types de membres", "AGE_GROUP" => "Tranches d’âge", "DEPARTMENT_TEAM" => "Équipes", _ => value };
    private static string MemberName(MemberDto item) => string.Join(" ", new[] { item.Name, item.Postname, item.Surname }.Where(x => !string.IsNullOrWhiteSpace(x)));
    private string TeamName(DepartmentTeamDto team) => $"{_departments.FirstOrDefault(x => x.Id == team.DepartmentId)?.Name ?? "Département"} · {team.Name}";
    private static string RecipientRate(NotificationSummaryDto summary) => summary.TotalRecipients == 0 ? "0 %" : $"{Math.Round(summary.TotalRead * 100d / summary.TotalRecipients):0.#} %";
    private void ShowError<T>(DataResponse<T>? response, string fallback) => Error(ResponseMessage(response, fallback));
    private static string ResponseMessage<T>(DataResponse<T>? response, string fallback) { if (!string.IsNullOrWhiteSpace(response?.Message)) return response.Message; var errors = string.Join(" ", response?.Error?.Where(x => !string.IsNullOrWhiteSpace(x)) ?? []); return string.IsNullOrWhiteSpace(errors) ? fallback : errors; }
    private void Error(string value) { _notice = value; _noticeIsError = true; }
    private void Success(string value) { _notice = value; _noticeIsError = false; }
    public void Dispose() { _searchCts?.Cancel(); _searchCts?.Dispose(); }
}

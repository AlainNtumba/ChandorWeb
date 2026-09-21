using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.MemberRequest;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.DTOs.MemberRequest;
using ChandorProject.Shared.DTOs.RequestType;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Pages.ncd;

public partial class MemberRequestManagement : IDisposable
{
    [Inject] private IMemberRequestService RequestService { get; set; } = default!;
    [Inject] private IRequestTypeService RequestTypeService { get; set; } = default!;
    [Inject] private IMemberService MemberService { get; set; } = default!;
    [Inject] private SfDialogService DialogService { get; set; } = default!;

    private readonly string[] _statuses = ["PENDING", "IN_PROGRESS", "PROCESSED", "REJECTED", "CANCELLED"];
    private readonly MemberRequestFilterState _requestFilter = new();
    private readonly RequestTypeFilterState _typeFilter = new();
    private PagedResult<MemberRequestDto> _requestPage = new();
    private PagedResult<RequestTypeDto> _typePage = new();
    private MemberRequestSummaryDto _summary = new();
    private IReadOnlyList<RequestTypeDto> _allTypes = [];
    private IReadOnlyList<MemberDto> _members = [];
    private DateTime? _summaryFrom;
    private DateTime? _summaryTo;
    private int _selectedTab;
    private bool _loading;
    private bool _saving;
    private bool _dialogOpen;
    private string _dialogKind = string.Empty;
    private string _dialogTitle = string.Empty;
    private string? _notice;
    private bool _noticeIsError;
    private CancellationTokenSource? _searchCts;

    private MemberRequestDto? _selectedRequest;
    private UpdateMemberRequestDto _requestModel = new();
    private string _statusModel = "PENDING";
    private Guid? _editingTypeId;
    private UpdateRequestTypeDto _typeModel = new();

    protected override async Task OnInitializedAsync() => await LoadInitialAsync();

    private async Task LoadInitialAsync()
    {
        _loading = true;
        try { await Task.WhenAll(LoadSummaryCoreAsync(), LoadRequestsCoreAsync(), LoadTypesCoreAsync(), LoadTypeLookupAsync(), LoadMembersAsync()); }
        finally { _loading = false; }
    }

    private async Task LoadSummaryCoreAsync(CancellationToken token = default)
    {
        var response = await RequestService.GetSummaryAsync(_summaryFrom, _summaryTo, token);
        if (response is { Success: true, Data: not null }) _summary = response.Data;
        else ShowError(response, "Impossible de charger les statistiques.");
    }

    private async Task LoadRequestsCoreAsync(CancellationToken token = default)
    {
        var response = await RequestService.GetPagedAsync(_requestFilter, token);
        if (response is { Success: true, Data: not null }) _requestPage = response.Data;
        else ShowError(response, "Impossible de charger les demandes.");
    }

    private async Task LoadTypesCoreAsync(CancellationToken token = default)
    {
        var response = await RequestTypeService.GetAdminAsync(_typeFilter, token);
        if (response is { Success: true, Data: not null }) _typePage = response.Data;
        else ShowError(response, "Impossible de charger les types de demandes.");
    }

    private async Task LoadTypeLookupAsync(CancellationToken token = default)
    {
        var response = await RequestTypeService.GetAdminAsync(new RequestTypeFilterState { PageSize = 100 }, token);
        if (response is { Success: true, Data: not null }) _allTypes = response.Data.Items.OrderBy(x => x.OptionsName).ToList();
    }

    private async Task LoadMembersAsync()
    {
        var response = await MemberService.GetAllMembersAsync();
        if (response is { Success: true, Data: not null }) _members = response.Data.OrderBy(x => x.Name).ThenBy(x => x.Surname).ToList();
    }

    private async Task RefreshSelectedAsync()
    {
        _loading = true;
        try
        {
            if (_selectedTab == 0) await LoadSummaryCoreAsync();
            else if (_selectedTab == 1) await LoadRequestsCoreAsync();
            else await Task.WhenAll(LoadTypesCoreAsync(), LoadTypeLookupAsync());
        }
        finally { _loading = false; }
    }

    private async Task ApplySummaryPeriodAsync() => await LoadSummaryCoreAsync();
    private async Task ApplyRequestFiltersAsync() { _requestFilter.Page = 1; await LoadRequestsCoreAsync(); }
    private async Task ApplyTypeFiltersAsync() { _typeFilter.Page = 1; await LoadTypesCoreAsync(); }

    private Task OnRequestKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _requestFilter.Keyword = value, args.Value?.ToString(), () => { _requestFilter.Page = 1; return LoadRequestsCoreAsync(_searchCts!.Token); });
    private Task OnTypeKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _typeFilter.Keyword = value, args.Value?.ToString(), () => { _typeFilter.Page = 1; return LoadTypesCoreAsync(_searchCts!.Token); });
    private async Task DebounceAsync(Action<string> setter, string? value, Func<Task> loader)
    {
        setter(value ?? string.Empty); _searchCts?.Cancel(); _searchCts?.Dispose(); _searchCts = new CancellationTokenSource();
        try { await Task.Delay(400, _searchCts.Token); await loader(); await InvokeAsync(StateHasChanged); }
        catch (OperationCanceledException) { }
    }

    private async Task ResetRequestFiltersAsync()
    {
        _requestFilter.Type = null; _requestFilter.Status = null; _requestFilter.MemberId = null; _requestFilter.Keyword = string.Empty; _requestFilter.FromDate = null; _requestFilter.ToDate = null; _requestFilter.Page = 1;
        await LoadRequestsCoreAsync();
    }

    private async Task ChangePageAsync(string target, int page)
    {
        if (page < 1) return;
        if (target == "REQUEST") { _requestFilter.Page = page; await LoadRequestsCoreAsync(); }
        else { _typeFilter.Page = page; await LoadTypesCoreAsync(); }
    }

    private async Task<MemberRequestDto?> LoadDetailAsync(Guid id)
    {
        var response = await RequestService.GetByIdAsync(id);
        if (response is { Success: true, Data: not null }) return response.Data;
        ShowError(response, "Impossible de charger cette demande."); return null;
    }

    private async Task OpenDetailAsync(MemberRequestDto row)
    {
        var detail = await LoadDetailAsync(row.Id); if (detail is null) return;
        _selectedRequest = detail; _dialogKind = "DETAIL"; _dialogTitle = "Détail de la demande"; _dialogOpen = true;
    }

    private async Task OpenEditAsync(MemberRequestDto row)
    {
        var detail = await LoadDetailAsync(row.Id); if (detail is null) return;
        _selectedRequest = detail;
        _requestModel = new UpdateMemberRequestDto { RequestTypeId = detail.RequestTypeId, MemberId = detail.MemberId, LastName = detail.LastName, MiddleName = detail.MiddleName, FirstName = detail.FirstName, Phone = detail.Phone, Address = detail.Address, Subject = detail.Subject, Message = detail.Message, Status = NormalizeStatus(detail.Status) };
        _dialogKind = "EDIT"; _dialogTitle = "Modifier la demande"; _dialogOpen = true;
    }

    private async Task OpenStatusAsync(MemberRequestDto row)
    {
        var detail = await LoadDetailAsync(row.Id); if (detail is null) return;
        _selectedRequest = detail; _statusModel = NormalizeStatus(detail.Status); _dialogKind = "STATUS"; _dialogTitle = "Changer le statut"; _dialogOpen = true;
    }

    private void OpenNewType()
    {
        _editingTypeId = null; _typeModel = new UpdateRequestTypeDto { IsActive = true }; _dialogKind = "TYPE"; _dialogTitle = "Nouveau type de demande"; _dialogOpen = true;
    }

    private void EditType(RequestTypeDto item)
    {
        _editingTypeId = item.Id; _typeModel = new UpdateRequestTypeDto { Code = item.Code, OptionsName = item.OptionsName, Description = item.Description, IsActive = item.IsActive };
        _dialogKind = "TYPE"; _dialogTitle = "Modifier le type de demande"; _dialogOpen = true;
    }

    private async Task SaveDialogAsync()
    {
        if (_saving) return; _saving = true;
        try
        {
            if (_dialogKind == "EDIT") await SaveRequestAsync();
            else if (_dialogKind == "STATUS") await SaveStatusAsync();
            else if (_dialogKind == "TYPE") await SaveTypeAsync();
        }
        finally { _saving = false; }
    }

    private async Task SaveRequestAsync()
    {
        if (_selectedRequest is null) return;
        if (_requestModel.RequestTypeId == Guid.Empty || string.IsNullOrWhiteSpace(_requestModel.LastName) || string.IsNullOrWhiteSpace(_requestModel.FirstName) || string.IsNullOrWhiteSpace(_requestModel.Phone) || string.IsNullOrWhiteSpace(_requestModel.Subject) || string.IsNullOrWhiteSpace(_requestModel.Message)) { Error("Le type, les noms, le téléphone, le sujet et le message sont obligatoires."); return; }
        var response = await RequestService.UpdateAsync(_selectedRequest.Id, _requestModel);
        if (response is not { Success: true }) { ShowError(response, "Impossible de modifier la demande."); return; }
        Success("Demande modifiée."); CloseDialog(); await ReloadRequestsAndSummaryAsync();
    }

    private async Task SaveStatusAsync()
    {
        if (_selectedRequest is null || !_statuses.Contains(_statusModel)) { Error("Sélectionnez un statut valide."); return; }
        var response = await RequestService.UpdateStatusAsync(_selectedRequest.Id, _statusModel);
        if (response is not { Success: true }) { ShowError(response, "Impossible de changer le statut."); return; }
        Success("Statut mis à jour."); CloseDialog(); await ReloadRequestsAndSummaryAsync();
    }

    private async Task SaveTypeAsync()
    {
        if (string.IsNullOrWhiteSpace(_typeModel.Code) || string.IsNullOrWhiteSpace(_typeModel.OptionsName)) { Error("Le code et le libellé sont obligatoires."); return; }
        DataResponse<RequestTypeDto>? response = _editingTypeId.HasValue
            ? await RequestTypeService.UpdateAsync(_editingTypeId.Value, _typeModel)
            : await RequestTypeService.CreateAsync(new CreateRequestTypeDto { Code = _typeModel.Code, OptionsName = _typeModel.OptionsName, Description = _typeModel.Description, IsActive = _typeModel.IsActive });
        if (response is not { Success: true }) { ShowError(response, "Impossible d’enregistrer le type."); return; }
        Success("Type de demande enregistré."); CloseDialog(); await Task.WhenAll(LoadTypesCoreAsync(), LoadTypeLookupAsync());
    }

    private async Task DeleteRequestAsync(MemberRequestDto item)
    {
        var confirmed = await DialogService.ConfirmAsync("Voulez-vous supprimer définitivement cette demande ? Cette opération est irréversible.", "Confirmation");
        if (!confirmed) return;
        var response = await RequestService.DeleteAsync(item.Id);
        if (response is { Success: true }) { Success("Demande supprimée."); await ReloadRequestsAndSummaryAsync(); }
        else ShowError(response, "Impossible de supprimer cette demande.");
    }

    private async Task DeleteTypeAsync(RequestTypeDto item)
    {
        if (!await DialogService.ConfirmAsync($"Supprimer le type « {item.OptionsName} » ?", "Confirmation")) return;
        var response = await RequestTypeService.DeleteAsync(item.Id);
        if (response is { Success: true }) { Success("Type supprimé."); await Task.WhenAll(LoadTypesCoreAsync(), LoadTypeLookupAsync()); return; }
        if (item.RequestCount > 0 && item.IsActive && await DialogService.ConfirmAsync("Ce type est déjà utilisé. Voulez-vous le désactiver à la place ?", "Type utilisé"))
        {
            var update = await RequestTypeService.UpdateAsync(item.Id, new UpdateRequestTypeDto { Code = item.Code, OptionsName = item.OptionsName, Description = item.Description, IsActive = false });
            if (update is { Success: true }) { Success("Type désactivé."); await Task.WhenAll(LoadTypesCoreAsync(), LoadTypeLookupAsync()); return; }
            ShowError(update, "Impossible de désactiver ce type."); return;
        }
        ShowError(response, "Impossible de supprimer ce type.");
    }

    private async Task ReloadRequestsAndSummaryAsync() => await Task.WhenAll(LoadRequestsCoreAsync(), LoadSummaryCoreAsync());
    private void CloseDialog() { if (_saving) return; _dialogOpen = false; _dialogKind = string.Empty; _selectedRequest = null; _editingTypeId = null; }
    private string MemberName(Guid? id) { var member = _members.FirstOrDefault(x => x.Id == id); return member is null ? "Visiteur" : string.Join(" ", new[] { member.Name, member.Postname, member.Surname }.Where(x => !string.IsNullOrWhiteSpace(x))); }
    private static string FullName(MemberRequestDto item) => string.Join(" ", new[] { item.LastName, item.MiddleName, item.FirstName }.Where(x => !string.IsNullOrWhiteSpace(x)));
    private static string StatusLabel(string value) => NormalizeStatus(value) switch { "PENDING" => "En attente", "IN_PROGRESS" => "En traitement", "PROCESSED" => "Traitée", "REJECTED" => "Rejetée", "CANCELLED" => "Annulée", _ => value };
    private static string NormalizeStatus(string value) => value.Trim().Replace("-", "_").Replace(" ", "_").ToUpperInvariant();
    private void ShowError<T>(DataResponse<T>? response, string fallback) => Error(ResponseMessage(response, fallback));
    private static string ResponseMessage<T>(DataResponse<T>? response, string fallback) { if (!string.IsNullOrWhiteSpace(response?.Message)) return response.Message; var errors = string.Join(" ", response?.Error?.Where(x => !string.IsNullOrWhiteSpace(x)) ?? []); return string.IsNullOrWhiteSpace(errors) ? fallback : errors; }
    private void Error(string value) { _notice = value; _noticeIsError = true; }
    private void Success(string value) { _notice = value; _noticeIsError = false; }
    public void Dispose() { _searchCts?.Cancel(); _searchCts?.Dispose(); }
}

using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.ChurchDirectory;
using ChandorProject.Shared.DTOs.ChurchDirectory;
using ChandorProject.Shared.DTOs.Department;
using ChandorProject.Shared.DTOs.Member;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;
using Syncfusion.Blazor.Popups;

namespace ChandorAdmin.Pages.ncd;

public partial class ChurchDirectoryManagement : IDisposable
{
    [Inject] private IChurchDirectoryService DirectoryService { get; set; } = default!;
    [Inject] private IMemberService MemberService { get; set; } = default!;
    [Inject] private IDepartmentService DepartmentService { get; set; } = default!;
    [Inject] private SfDialogService DialogService { get; set; } = default!;

    private readonly DirectoryFilterState _typeFilter = new();
    private readonly DirectoryFilterState _itemFilter = new();
    private PagedResult<ChurchDirectoryTypeDto> _typePage = new();
    private PagedResult<ChurchDirectoryItemDto> _itemPage = new();
    private IReadOnlyList<ChurchDirectoryTypeDto> _types = [];
    private IReadOnlyList<ChurchDirectoryItemDto> _itemLookup = [];
    private IReadOnlyList<ChurchDirectoryTreeItemDto> _tree = [];
    private IReadOnlyList<ChurchDirectoryTypeDto> _publicTypes = [];
    private IReadOnlyList<ChurchDirectoryItemDto> _publicItems = [];
    private IReadOnlyList<MemberDto> _members = [];
    private IReadOnlyList<DepartmentDto> _departments = [];

    private int _selectedTab;
    private bool _loading;
    private bool _saving;
    private bool _dialogOpen;
    private string _dialogKind = string.Empty;
    private string _dialogTitle = string.Empty;
    private string _itemSubTab = "GENERAL";
    private string? _notice;
    private bool _noticeIsError;
    private CancellationTokenSource? _searchCts;

    private Guid? _editingId;
    private ChurchDirectoryTypeInputDto _typeModel = NewTypeModel();
    private ChurchDirectoryItemInputDto _itemModel = NewItemModel();
    private DirectoryImageUpload? _selectedImage;
    private bool _deleteImage;
    private Guid _treeTypeId;
    private Guid _previewTypeId;
    private ChurchDirectoryItemDto? _previewDetail;

    private static readonly string[] DisplayKinds = ["LOCATION", "ORGANIZATION"];
    private ChurchDirectoryTypeDto? PreviewType => _publicTypes.FirstOrDefault(x => x.Id == _previewTypeId);
    private string CurrentDisplayKind => _types.FirstOrDefault(x => x.Id == _itemModel.TypeId)?.DisplayKind ?? "LOCATION";
    private IEnumerable<ChurchDirectoryItemDto> ParentOptions => _itemLookup.Where(x => x.TypeId == _itemModel.TypeId && x.Id != _editingId);
    private IEnumerable<ChurchDirectoryItemDto> ParentFilterOptions => _itemLookup.Where(x => !_itemFilter.TypeId.HasValue || x.TypeId == _itemFilter.TypeId.Value);

    protected override async Task OnInitializedAsync() => await LoadInitialAsync();

    private async Task LoadInitialAsync()
    {
        _loading = true;
        try
        {
            await Task.WhenAll(LoadTypesCoreAsync(), LoadTypeLookupAsync(), LoadItemsCoreAsync(), LoadItemLookupAsync(), LoadMembersAsync(), LoadDepartmentsAsync(), LoadPublicTypesAsync());
            _treeTypeId = _types.FirstOrDefault()?.Id ?? Guid.Empty;
            _previewTypeId = _publicTypes.FirstOrDefault()?.Id ?? Guid.Empty;
        }
        finally { _loading = false; }
    }

    private async Task LoadTypesCoreAsync(CancellationToken token = default)
    {
        var response = await DirectoryService.GetAdminTypesAsync(_typeFilter, token);
        if (response is { Success: true, Data: not null }) _typePage = response.Data;
        else ShowError(response, "Impossible de charger les types de l’annuaire.");
    }

    private async Task LoadTypeLookupAsync(CancellationToken token = default)
    {
        var response = await DirectoryService.GetAdminTypesAsync(new DirectoryFilterState { PageSize = 100 }, token);
        if (response is { Success: true, Data: not null }) _types = response.Data.Items.OrderBy(x => x.SortOrder).ToList();
    }

    private async Task LoadItemsCoreAsync(CancellationToken token = default)
    {
        var response = await DirectoryService.GetAdminItemsAsync(_itemFilter, token);
        if (response is { Success: true, Data: not null }) _itemPage = response.Data;
        else ShowError(response, "Impossible de charger les éléments de l’annuaire.");
    }

    private async Task LoadItemLookupAsync(CancellationToken token = default)
    {
        var response = await DirectoryService.GetAdminItemsAsync(new DirectoryFilterState { PageSize = 100 }, token);
        if (response is { Success: true, Data: not null }) _itemLookup = response.Data.Items;
    }

    private async Task LoadMembersAsync()
    {
        var response = await MemberService.GetAllMembersAsync();
        if (response is { Success: true, Data: not null }) _members = response.Data.OrderBy(x => x.Name).ThenBy(x => x.Surname).ToList();
    }

    private async Task LoadDepartmentsAsync()
    {
        var response = await DepartmentService.GetDepartmentsAsync();
        if (response is { Success: true, Data: not null }) _departments = response.Data.OrderBy(x => x.Name).ToList();
    }

    private async Task LoadPublicTypesAsync(CancellationToken token = default)
    {
        var response = await DirectoryService.GetPublicTypesAsync(new DirectoryFilterState { IsActive = true, PageSize = 100 }, token);
        if (response is { Success: true, Data: not null }) _publicTypes = response.Data.Items;
    }

    private async Task LoadTreeAsync()
    {
        if (_treeTypeId == Guid.Empty) { _tree = []; return; }
        var response = await DirectoryService.GetTreeAsync(_treeTypeId, null);
        if (response is { Success: true, Data: not null }) _tree = response.Data;
        else ShowError(response, "Impossible de charger la hiérarchie.");
    }

    private async Task LoadPreviewAsync()
    {
        if (_previewTypeId == Guid.Empty) { _publicItems = []; return; }
        var response = await DirectoryService.GetPublicItemsAsync(_previewTypeId, new DirectoryFilterState { IsActive = true, PageSize = 100 });
        if (response is { Success: true, Data: not null }) _publicItems = response.Data.Items;
        else ShowError(response, "Impossible de charger la prévisualisation publique.");
    }

    private async Task OnSelectedTabChangedAsync()
    {
        _notice = null;
        if (_selectedTab == 2 && _tree.Count == 0) await LoadTreeAsync();
        if (_selectedTab == 3 && _publicItems.Count == 0) await LoadPreviewAsync();
    }

    private async Task RefreshSelectedAsync()
    {
        _loading = true;
        try
        {
            switch (_selectedTab)
            {
                case 0: await Task.WhenAll(LoadTypesCoreAsync(), LoadTypeLookupAsync()); break;
                case 1: await Task.WhenAll(LoadItemsCoreAsync(), LoadItemLookupAsync()); break;
                case 2: await LoadTreeAsync(); break;
                case 3: await Task.WhenAll(LoadPublicTypesAsync(), LoadPreviewAsync()); break;
            }
        }
        finally { _loading = false; }
    }

    private Task OnTypeKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _typeFilter.Keyword = value, args.Value?.ToString(), () => { _typeFilter.Page = 1; return LoadTypesCoreAsync(_searchCts!.Token); });
    private Task OnItemKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _itemFilter.Keyword = value, args.Value?.ToString(), () => { _itemFilter.Page = 1; return LoadItemsCoreAsync(_searchCts!.Token); });

    private async Task DebounceAsync(Action<string> setter, string? value, Func<Task> loader)
    {
        setter(value ?? string.Empty);
        _searchCts?.Cancel(); _searchCts?.Dispose(); _searchCts = new CancellationTokenSource();
        try { await Task.Delay(400, _searchCts.Token); await loader(); await InvokeAsync(StateHasChanged); }
        catch (OperationCanceledException) { }
    }

    private async Task FilterTypesAsync() { _typeFilter.Page = 1; await LoadTypesCoreAsync(); }
    private async Task FilterItemTypeAsync() { _itemFilter.ParentId = null; await FilterItemsAsync(); }
    private async Task FilterItemsAsync() { _itemFilter.Page = 1; await LoadItemsCoreAsync(); }

    private async Task ChangePageAsync(string target, int page)
    {
        if (page < 1) return;
        if (target == "TYPE") { _typeFilter.Page = page; await LoadTypesCoreAsync(); }
        else { _itemFilter.Page = page; await LoadItemsCoreAsync(); }
    }

    private void OpenNewDialog()
    {
        if (_selectedTab == 0) OpenNewType();
        else if (_selectedTab == 1) OpenNewItem();
    }

    private void OpenNewType()
    {
        _editingId = null; _typeModel = NewTypeModel(); _selectedImage = null; _deleteImage = false;
        _dialogKind = "TYPE"; _dialogTitle = "Nouveau type"; _dialogOpen = true;
    }

    private async Task EditType(ChurchDirectoryTypeDto item)
    {
        _loading = true;
        try
        {
            var response = await DirectoryService.GetAdminTypeByIdAsync(item.Id);
            if (response is not { Success: true, Data: not null })
            {
                ShowError(response, "Impossible de charger le détail de ce type.");
                return;
            }

            var detail = response.Data;
            _editingId = detail.Id; _selectedImage = null; _deleteImage = false; _dialogKind = "TYPE"; _dialogTitle = "Modifier le type";
            _typeModel = MapTypeToForm(detail);
            _dialogOpen = true;
        }
        finally
        {
            _loading = false;
        }
    }

    private static ChurchDirectoryTypeInputDto MapTypeToForm(ChurchDirectoryTypeDto item) => new()
    {
        Code = item.Code,
        Name = item.Name,
        Description = item.Description,
        DisplayKind = item.DisplayKind,
        Icon = item.Icon,
        HeroImageUrl = item.HeroImageUrl,
        SortOrder = item.SortOrder,
        IsActive = item.IsActive
    };

    private void OpenNewItem(Guid? parentId = null, Guid? typeId = null)
    {
        _editingId = null; _selectedImage = null; _deleteImage = false; _itemSubTab = "GENERAL"; _dialogKind = "ITEM"; _dialogTitle = parentId.HasValue ? "Nouvel enfant" : "Nouvel élément";
        _itemModel = NewItemModel(); _itemModel.ParentId = parentId; _itemModel.TypeId = typeId ?? _itemFilter.TypeId ?? Guid.Empty; EnsureDetails(); _dialogOpen = true;
    }

    private void AddTreeChild(ChurchDirectoryItemDto parent) => OpenNewItem(parent.Id, parent.TypeId);

    private void EditItem(ChurchDirectoryItemDto item)
    {
        _editingId = item.Id; _selectedImage = null; _deleteImage = false; _itemSubTab = "GENERAL"; _dialogKind = "ITEM"; _dialogTitle = "Modifier l’élément";
        _itemModel = new ChurchDirectoryItemInputDto
        {
            TypeId = item.TypeId, ParentId = item.ParentId, DepartmentId = item.DepartmentId, Code = item.Code, Name = item.Name,
            Description = item.Description, ImageUrl = item.ImageUrl, SortOrder = item.SortOrder, IsActive = item.IsActive,
            LocationDetails = item.LocationDetails is null ? null : new ChurchLocationDetailsDto { Area = item.LocationDetails.Area, Address = item.LocationDetails.Address, ReferencePoint = item.LocationDetails.ReferencePoint, Latitude = item.LocationDetails.Latitude, Longitude = item.LocationDetails.Longitude },
            DepartmentDetails = item.DepartmentDetails is null ? null : new ChurchDepartmentDetailsDto { Organization = item.DepartmentDetails.Organization, Mission = item.DepartmentDetails.Mission, History = item.DepartmentDetails.History },
            Contacts = item.Contacts.Select(x => new ChurchDirectoryContactDto { Id = x.Id, Type = x.Type, Label = x.Label, Value = x.Value, IsPrimary = x.IsPrimary }).ToList(),
            Members = item.Members.Select(x => new ChurchDirectoryMemberDto { Id = x.Id, MemberId = x.MemberId, Role = x.Role, IsPrimary = x.IsPrimary, MemberName = x.MemberName }).ToList()
        };
        EnsureDetails(); _dialogOpen = true;
    }

    private void OnItemTypeChanged(ChangeEventArgs args)
    {
        _itemModel.TypeId = Guid.TryParse(args.Value?.ToString(), out var id) ? id : Guid.Empty;
        _itemModel.ParentId = null; EnsureDetails();
    }

    private void EnsureDetails()
    {
        if (CurrentDisplayKind == "ORGANIZATION") { _itemModel.DepartmentDetails ??= new ChurchDepartmentDetailsDto(); _itemModel.LocationDetails = null; }
        else { _itemModel.LocationDetails ??= new ChurchLocationDetailsDto(); _itemModel.DepartmentDetails = null; }
    }

    private async Task SaveDialogAsync()
    {
        if (_saving) return;
        _saving = true;
        try { if (_dialogKind == "TYPE") await SaveTypeAsync(); else if (_dialogKind == "ITEM") await SaveItemAsync(); }
        finally { _saving = false; }
    }

    private async Task SaveTypeAsync()
    {
        if (string.IsNullOrWhiteSpace(_typeModel.Code) || string.IsNullOrWhiteSpace(_typeModel.Name)) { Error("Le code et le nom sont obligatoires."); return; }
        var response = _editingId.HasValue ? await DirectoryService.UpdateTypeAsync(_editingId.Value, _typeModel) : await DirectoryService.CreateTypeAsync(_typeModel);
        if (response is not { Success: true, Data: not null }) { ShowError(response, "Impossible d’enregistrer le type."); return; }
        var id = response.Data.Id;
        if (_selectedImage is not null)
        {
            var upload = await DirectoryService.UploadTypeHeroAsync(id, _selectedImage);
            if (upload is not { Success: true, Data: not null }) { ShowError(upload, "Le type est enregistré, mais l’image Hero n’a pas pu être envoyée."); return; }
            _typeModel = MapTypeToForm(upload.Data);
        }
        else if (_editingId.HasValue && _deleteImage)
        {
            var deletion = await DirectoryService.DeleteTypeHeroAsync(id);
            if (deletion is not { Success: true }) { ShowError(deletion, "Le type est enregistré, mais l’image Hero n’a pas pu être supprimée."); return; }
        }
        Success("Type enregistré."); CloseDialogCore(); await ReloadAfterTypeMutationAsync();
    }

    private async Task SaveItemAsync()
    {
        EnsureDetails();
        if (_itemModel.TypeId == Guid.Empty || string.IsNullOrWhiteSpace(_itemModel.Code) || string.IsNullOrWhiteSpace(_itemModel.Name)) { Error("Le type, le code et le nom sont obligatoires."); return; }
        if (_itemModel.Contacts.Any(x => string.IsNullOrWhiteSpace(x.Type) || string.IsNullOrWhiteSpace(x.Value))) { Error("Chaque contact doit avoir un type et une valeur."); return; }
        if (_itemModel.Members.GroupBy(x => x.MemberId).Any(x => x.Count() > 1)) { Error("Un membre ne peut être ajouté qu’une seule fois."); return; }
        var response = _editingId.HasValue ? await DirectoryService.UpdateItemAsync(_editingId.Value, _itemModel) : await DirectoryService.CreateItemAsync(_itemModel);
        if (response is not { Success: true, Data: not null }) { ShowError(response, "Impossible d’enregistrer l’élément."); return; }
        var id = response.Data.Id;
        if (_selectedImage is not null)
        {
            var upload = await DirectoryService.UploadItemImageAsync(id, _selectedImage);
            if (upload is not { Success: true }) { ShowError(upload, "L’élément est enregistré, mais son image n’a pas pu être envoyée."); return; }
        }
        else if (_editingId.HasValue && _deleteImage)
        {
            var deletion = await DirectoryService.DeleteItemImageAsync(id);
            if (deletion is not { Success: true }) { ShowError(deletion, "L’élément est enregistré, mais son image n’a pas pu être supprimée."); return; }
        }
        Success("Élément enregistré."); CloseDialogCore(); await ReloadAfterItemMutationAsync();
    }

    private async Task DeleteTypeAsync(ChurchDirectoryTypeDto item)
    {
        if (!await DialogService.ConfirmAsync($"Supprimer le type « {item.Name} » ?", "Confirmation")) return;
        var response = await DirectoryService.DeleteTypeAsync(item.Id);
        if (response is { Success: true }) { Success("Type supprimé."); await ReloadAfterTypeMutationAsync(); } else ShowError(response, "Impossible de supprimer ce type. Vérifiez qu’il ne contient plus d’éléments.");
    }

    private async Task DeleteItemAsync(ChurchDirectoryItemDto item)
    {
        if (!await DialogService.ConfirmAsync($"Supprimer l’élément « {item.Name} » ?", "Confirmation")) return;
        var response = await DirectoryService.DeleteItemAsync(item.Id);
        if (response is { Success: true }) { Success("Élément supprimé."); await ReloadAfterItemMutationAsync(); } else ShowError(response, "Impossible de supprimer cet élément. Vérifiez qu’il n’a plus d’enfants.");
    }

    private async Task ReloadAfterTypeMutationAsync()
    {
        await Task.WhenAll(LoadTypesCoreAsync(), LoadTypeLookupAsync(), LoadPublicTypesAsync());
        if (_treeTypeId != Guid.Empty) await LoadTreeAsync();
    }

    private async Task ReloadAfterItemMutationAsync()
    {
        await Task.WhenAll(LoadItemsCoreAsync(), LoadItemLookupAsync(), LoadTypesCoreAsync());
        if (_treeTypeId != Guid.Empty) await LoadTreeAsync();
        if (_previewTypeId != Guid.Empty) await LoadPreviewAsync();
    }

    private void OpenPreviewDetail(ChurchDirectoryItemDto item) { _previewDetail = item; _dialogKind = "PREVIEW"; _dialogTitle = item.Name; _dialogOpen = true; }
    private void CloseDialog() { if (!_saving) CloseDialogCore(); }
    private void CloseDialogCore() { _dialogOpen = false; _dialogKind = string.Empty; _editingId = null; _selectedImage = null; _deleteImage = false; }

    private void ShowError<T>(DataResponse<T>? response, string fallback) => Error(ResponseMessage(response, fallback));
    private static string ResponseMessage<T>(DataResponse<T>? response, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(response?.Message)) return response.Message;
        var errors = string.Join(" ", response?.Error?.Where(x => !string.IsNullOrWhiteSpace(x)) ?? []);
        return string.IsNullOrWhiteSpace(errors) ? fallback : errors;
    }
    private void Error(string text) { _notice = text; _noticeIsError = true; }
    private void Success(string text) { _notice = text; _noticeIsError = false; }
    private static ChurchDirectoryTypeInputDto NewTypeModel() => new() { DisplayKind = "LOCATION", IsActive = true };
    private static ChurchDirectoryItemInputDto NewItemModel() => new() { IsActive = true };
    public void Dispose() { _searchCts?.Cancel(); _searchCts?.Dispose(); }
}

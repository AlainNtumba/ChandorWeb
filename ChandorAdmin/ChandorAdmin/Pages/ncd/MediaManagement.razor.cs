using ChandorAdmin.Configuration;
using ChandorAdmin.Interfaces.Api;
using ChandorAdmin.Models.Media;
using ChandorProject.Shared.DTOs.Media;
using ChandorProject.Shared.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Options;
using Syncfusion.Blazor.Popups;
using System.Globalization;
using System.Text;

namespace ChandorAdmin.Pages.ncd;

public partial class MediaManagement : IDisposable
{
    private const long MaximumUploadSize = 100L * 1024 * 1024;

    [Inject] private IMediaService MediaService { get; set; } = default!;
    [Inject] private SfDialogService DialogService { get; set; } = default!;
    [Inject] private IOptions<ChandorApiOptions> ApiOptions { get; set; } = default!;

    private readonly string[] _statuses = ["DRAFT", "PUBLISHED", "ARCHIVED"];
    private readonly string[] _itemTypes = ["ARTICLE", "EPISODE", "EVENT", "QUOTE", "PRODUCT", "SERVICE_VIDEO", "SHORT_VIDEO"];
    private readonly string[] _assetTypes = ["IMAGE", "VIDEO", "AUDIO", "DOCUMENT"];
    private readonly string[] _providers = ["LOCAL", "YOUTUBE", "VIMEO", "CDN", "EXTERNAL"];
    private readonly string[] _roles = ["PRIMARY", "BANNER", "COVER", "TRAILER", "BACKGROUND", "THUMBNAIL", "GALLERY"];
    private readonly string[] _periods = ["RECENT", "THIS_WEEK", "NEXT_WEEK", "UPCOMING", "LAST_WEEK"];

    private IReadOnlyList<MediaCategoryDto> _categories = [];
    private IReadOnlyList<MediaCollectionDto> _collectionLookup = [];
    private MediaCollectionPageDto _collections = new();
    private ContentItemPageDto _items = new();
    private MediaAssetPageDto _assets = new();
    private MediaFeedPageDto _feed = new();
    private IReadOnlyList<MediaAssetDto> _attachmentAssets = [];

    private readonly MediaFilterState _collectionFilter = new();
    private readonly MediaFilterState _itemFilter = new();
    private readonly MediaFilterState _assetFilter = new();
    private Guid _feedCategoryId;
    private string _feedPeriod = "RECENT";
    private string _feedKeyword = string.Empty;
    private int _feedPage = 1;

    private int _selectedTab;
    private bool _loading;
    private bool _dialogOpen;
    private bool _saving;
    private string _dialogKind = string.Empty;
    private string _dialogTitle = string.Empty;
    private string _contentSubTab = "GENERAL";
    private string? _notice;
    private bool _noticeIsError;
    private CancellationTokenSource? _searchCts;

    private Guid? _editingId;
    private MediaCategoryInputDto _categoryModel = new();
    private MediaCollectionInputDto _collectionModel = new();
    private ContentItemInputDto _itemModel = NewItemModel();
    private MediaAssetInputDto _assetModel = NewAssetModel();
    private string _assetEditorMode = "UPLOAD";
    private MediaUploadFile? _uploadFile;
    private int _uploadProgress;
    private string? _fileError;

    private string _attachmentOwnerType = string.Empty;
    private Guid _attachmentOwnerId;
    private Guid _attachmentAssetId;
    private string _attachmentRole = "PRIMARY";
    private int _attachmentSortOrder;
    private IReadOnlyList<MediaLinkDto> _attachedMedia = [];
    private MediaUploadFile? _attachmentUploadFile;
    private string? _detachingMediaKey;
    private string? _attachmentError;
    private MediaFeedDetailDto? _feedDetail;

    protected override async Task OnInitializedAsync() => await LoadInitialAsync();

    private async Task LoadInitialAsync()
    {
        _loading = true;
        try
        {
            await LoadCategoriesCoreAsync();
            await Task.WhenAll(LoadCollectionsCoreAsync(), LoadCollectionLookupCoreAsync(), LoadItemsCoreAsync(), LoadAssetsCoreAsync());
        }
        finally { _loading = false; }
    }

    private async Task RefreshSelectedAsync()
    {
        _loading = true;
        try
        {
            switch (_selectedTab)
            {
                case 0: await LoadCategoriesCoreAsync(); break;
                case 1: await LoadCollectionsCoreAsync(); break;
                case 2: await LoadItemsCoreAsync(); break;
                case 3: await LoadAssetsCoreAsync(); break;
                case 4: await LoadFeedCoreAsync(); break;
            }
        }
        finally { _loading = false; }
    }

    private async Task OnSelectedTabChangedAsync()
    {
        _notice = null;
        if (_selectedTab == 4 && _feedCategoryId != Guid.Empty && _feed.Items.Count == 0)
            await RefreshSelectedAsync();
    }

    private async Task LoadCategoriesCoreAsync(CancellationToken token = default)
    {
        var response = await MediaService.GetCategoriesAsync(true, token);
        if (response is { Success: true, Data: not null })
        {
            _categories = response.Data.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).ToList();
            if (_feedCategoryId == Guid.Empty)
                _feedCategoryId = _categories.FirstOrDefault(x => x.IsActive)?.Id ?? Guid.Empty;
        }
        else ShowError(response, "Impossible de charger les catégories.");
    }

    private async Task LoadCollectionsCoreAsync(CancellationToken token = default)
    {
        var response = await MediaService.GetCollectionsAsync(_collectionFilter, token);
        if (response is { Success: true, Data: not null }) _collections = response.Data;
        else ShowError(response, "Impossible de charger les collections.");
    }

    private async Task LoadCollectionLookupCoreAsync(CancellationToken token = default)
    {
        var response = await MediaService.GetCollectionsAsync(new MediaFilterState { PageSize = 100 }, token);
        if (response is { Success: true, Data: not null }) _collectionLookup = response.Data.Items;
    }

    private async Task LoadItemsCoreAsync(CancellationToken token = default)
    {
        var response = await MediaService.GetItemsAsync(_itemFilter, token);
        if (response is { Success: true, Data: not null }) _items = response.Data;
        else ShowError(response, "Impossible de charger les contenus.");
    }

    private async Task LoadAssetsCoreAsync(CancellationToken token = default)
    {
        var response = await MediaService.GetAssetsAsync(_assetFilter, token);
        if (response is { Success: true, Data: not null }) _assets = response.Data;
        else ShowError(response, "Impossible de charger la médiathèque.");
    }

    private async Task LoadFeedCoreAsync(CancellationToken token = default)
    {
        if (_feedCategoryId == Guid.Empty)
        {
            _feed = new MediaFeedPageDto();
            return;
        }
        var response = await MediaService.GetFeedAsync(_feedCategoryId, _feedPeriod, _feedKeyword, _feedPage, 20, token);
        if (response is { Success: true, Data: not null }) _feed = response.Data;
        else ShowError(response, "Impossible de charger la prévisualisation.");
    }

    private Task OnCollectionKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _collectionFilter.Keyword = value, args.Value?.ToString(), () => { _collectionFilter.Page = 1; return LoadCollectionsCoreAsync(_searchCts!.Token); });
    private Task OnItemKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _itemFilter.Keyword = value, args.Value?.ToString(), () => { _itemFilter.Page = 1; return LoadItemsCoreAsync(_searchCts!.Token); });
    private Task OnAssetKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _assetFilter.Keyword = value, args.Value?.ToString(), () => { _assetFilter.Page = 1; return LoadAssetsCoreAsync(_searchCts!.Token); });
    private Task OnFeedKeywordAsync(ChangeEventArgs args) => DebounceAsync(value => _feedKeyword = value, args.Value?.ToString(), () => { _feedPage = 1; return LoadFeedCoreAsync(_searchCts!.Token); });

    private async Task DebounceAsync(Action<string> setter, string? value, Func<Task> load)
    {
        setter(value ?? string.Empty);
        _searchCts?.Cancel();
        _searchCts?.Dispose();
        _searchCts = new CancellationTokenSource();
        try
        {
            await Task.Delay(400, _searchCts.Token);
            await load();
            await InvokeAsync(StateHasChanged);
        }
        catch (OperationCanceledException) { }
    }

    private async Task FilterCollectionsAsync() { _collectionFilter.Page = 1; await LoadCollectionsCoreAsync(); }
    private async Task FilterItemCategoryAsync() { _itemFilter.CollectionId = null; await FilterItemsAsync(); }
    private async Task FilterItemsAsync() { _itemFilter.Page = 1; await LoadItemsCoreAsync(); }
    private async Task FilterAssetsAsync() { _assetFilter.Page = 1; await LoadAssetsCoreAsync(); }
    private async Task FilterFeedAsync() { _feedPage = 1; await LoadFeedCoreAsync(); }

    private async Task ChangePageAsync(string target, int page)
    {
        if (page < 1) return;
        _loading = true;
        try
        {
            switch (target)
            {
                case "COLLECTION": _collectionFilter.Page = page; await LoadCollectionsCoreAsync(); break;
                case "ITEM": _itemFilter.Page = page; await LoadItemsCoreAsync(); break;
                case "ASSET": _assetFilter.Page = page; await LoadAssetsCoreAsync(); break;
                case "FEED": _feedPage = page; await LoadFeedCoreAsync(); break;
            }
        }
        finally { _loading = false; }
    }

    private void OpenNewDialog()
    {
        _editingId = null;
        _fileError = null;
        switch (_selectedTab)
        {
            case 0:
                _dialogKind = "CATEGORY"; _dialogTitle = "Nouvelle catégorie"; _categoryModel = new MediaCategoryInputDto(); break;
            case 1:
                _dialogKind = "COLLECTION"; _dialogTitle = "Nouvelle collection"; _collectionModel = new MediaCollectionInputDto(); break;
            case 2:
                _dialogKind = "ITEM"; _dialogTitle = "Nouveau contenu"; _itemModel = NewItemModel(); _contentSubTab = "GENERAL"; break;
            case 3:
                _dialogKind = "ASSET"; _dialogTitle = "Ajouter un média"; _assetModel = NewAssetModel(); _assetEditorMode = "UPLOAD"; _uploadFile = null; break;
            default: return;
        }
        _dialogOpen = true;
    }

    private void EditCategory(MediaCategoryDto item)
    {
        _editingId = item.Id; _dialogKind = "CATEGORY"; _dialogTitle = "Modifier la catégorie";
        _categoryModel = new MediaCategoryInputDto { Code = item.Code, Name = item.Name, Icon = item.Icon, IconColor = item.IconColor, IconBackground = item.IconBackground, ModuleType = item.ModuleType, SortOrder = item.SortOrder, IsActive = item.IsActive };
        _dialogOpen = true;
    }

    private void EditCollection(MediaCollectionDto item)
    {
        _editingId = item.Id; _dialogKind = "COLLECTION"; _dialogTitle = "Modifier la collection";
        _collectionModel = new MediaCollectionInputDto { CategoryId = item.CategoryId, Title = item.Title, Description = item.Description, Status = item.Status, SortOrder = item.SortOrder, PublishedAt = item.PublishedAt };
        _dialogOpen = true;
    }

    private void EditItem(ContentItemDto item)
    {
        _editingId = item.Id; _dialogKind = "ITEM"; _dialogTitle = "Modifier le contenu"; _contentSubTab = "GENERAL";
        _itemModel = new ContentItemInputDto
        {
            CategoryId = item.CategoryId, CollectionId = item.CollectionId, Slug = item.Slug, ItemType = item.ItemType,
            Title = item.Title, Description = item.Description, Status = item.Status, SortOrder = item.SortOrder,
            PublishedAt = item.PublishedAt, Episode = item.Episode is null ? null : new EpisodeDetailInputDto { EpisodeNumber = item.Episode.EpisodeNumber, Speaker = item.Episode.Speaker, RecordedAt = item.Episode.RecordedAt, DurationSeconds = item.Episode.DurationSeconds },
            Event = item.Event is null ? null : new EventDetailInputDto { StartsAt = item.Event.StartsAt, EndsAt = item.Event.EndsAt, Location = item.Event.Location, RegistrationUrl = item.Event.RegistrationUrl },
            Quote = item.Quote is null ? null : new QuoteDetailInputDto { QuoteText = item.Quote.QuoteText, Author = item.Quote.Author },
            Product = item.Product is null ? null : new ProductDetailInputDto { Sku = item.Product.Sku, Price = item.Product.Price, Currency = item.Product.Currency, StockQuantity = item.Product.StockQuantity, PurchaseUrl = item.Product.PurchaseUrl }
        };
        EnsureItemDetails();
        _dialogOpen = true;
    }

    private void EditAsset(MediaAssetDto item)
    {
        _editingId = item.Id; _dialogKind = "ASSET"; _dialogTitle = "Modifier le média"; _assetEditorMode = "EXTERNAL";
        _assetModel = new MediaAssetInputDto { AssetType = item.AssetType, Provider = item.Provider, Url = item.Url, StorageKey = item.StorageKey, MimeType = item.MimeType, ThumbnailUrl = item.ThumbnailUrl, DurationSeconds = item.DurationSeconds };
        _dialogOpen = true;
    }

    private void OnItemTypeChanged(ChangeEventArgs args)
    {
        _itemModel.ItemType = args.Value?.ToString() ?? "ARTICLE";
        EnsureItemDetails();
    }

    private void EnsureItemDetails()
    {
        _itemModel.Episode = _itemModel.ItemType == "EPISODE" ? _itemModel.Episode ?? new EpisodeDetailInputDto() : null;
        _itemModel.Event = _itemModel.ItemType == "EVENT" ? _itemModel.Event ?? new EventDetailInputDto { StartsAt = DateTime.Now } : null;
        _itemModel.Quote = _itemModel.ItemType == "QUOTE" ? _itemModel.Quote ?? new QuoteDetailInputDto() : null;
        _itemModel.Product = _itemModel.ItemType == "PRODUCT" ? _itemModel.Product ?? new ProductDetailInputDto() : null;
    }

    private async Task SaveDialogAsync()
    {
        if (_saving) return;
        _saving = true; _fileError = null;
        try
        {
            switch (_dialogKind)
            {
                case "CATEGORY": await SaveCategoryAsync(); break;
                case "COLLECTION": await SaveCollectionAsync(); break;
                case "ITEM": await SaveItemAsync(); break;
                case "ASSET": await SaveAssetAsync(); break;
                case "ATTACHMENT": await SaveAttachmentAsync(); break;
            }
        }
        finally { _saving = false; }
    }

    private async Task SaveCategoryAsync()
    {
        if (string.IsNullOrWhiteSpace(_categoryModel.Code) || string.IsNullOrWhiteSpace(_categoryModel.Name)) { Error("Le code et le nom sont obligatoires."); return; }
        var response = _editingId.HasValue ? await MediaService.UpdateCategoryAsync(_editingId.Value, _categoryModel) : await MediaService.CreateCategoryAsync(_categoryModel);
        if (!Succeeded(response, "Impossible d’enregistrer la catégorie.")) return;
        Success("Catégorie enregistrée."); CloseDialog(); await LoadCategoriesCoreAsync();
    }

    private async Task SaveCollectionAsync()
    {
        if (_collectionModel.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(_collectionModel.Title)) { Error("La catégorie et le titre sont obligatoires."); return; }
        var response = _editingId.HasValue ? await MediaService.UpdateCollectionAsync(_editingId.Value, _collectionModel) : await MediaService.CreateCollectionAsync(_collectionModel);
        if (!Succeeded(response, "Impossible d’enregistrer la collection.")) return;
        Success("Collection enregistrée."); CloseDialog(); await Task.WhenAll(LoadCollectionsCoreAsync(), LoadCollectionLookupCoreAsync(), LoadCategoriesCoreAsync());
    }

    private async Task SaveItemAsync()
    {
        EnsureItemDetails();
        if (_itemModel.CategoryId == Guid.Empty || string.IsNullOrWhiteSpace(_itemModel.Title)) { Error("La catégorie et le titre sont obligatoires."); return; }
        if (string.IsNullOrWhiteSpace(_itemModel.Slug)) _itemModel.Slug = CreateSlug(_itemModel.Title);
        if (string.IsNullOrWhiteSpace(_itemModel.Slug)) { Error("Le titre doit contenir au moins une lettre ou un chiffre."); return; }
        var response = _editingId.HasValue ? await MediaService.UpdateItemAsync(_editingId.Value, _itemModel) : await MediaService.CreateItemAsync(_itemModel);
        if (!Succeeded(response, "Impossible d’enregistrer le contenu.")) return;
        Success("Contenu enregistré."); CloseDialog(); await Task.WhenAll(LoadItemsCoreAsync(), LoadCategoriesCoreAsync());
    }

    private static string CreateSlug(string value)
    {
        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var slug = new StringBuilder(normalized.Length);
        var separatorPending = false;

        foreach (var character in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(character) == UnicodeCategory.NonSpacingMark) continue;
            if (char.IsLetterOrDigit(character))
            {
                if (separatorPending && slug.Length > 0) slug.Append('-');
                slug.Append(character);
                separatorPending = false;
            }
            else
            {
                separatorPending = slug.Length > 0;
            }
        }

        return slug.ToString();
    }

    private async Task SaveAssetAsync()
    {
        DataResponse<MediaAssetDto>? response;
        if (!_editingId.HasValue && _assetEditorMode == "UPLOAD")
        {
            if (_uploadFile is null) { _fileError = "Sélectionnez un fichier."; return; }
            _uploadProgress = 35;
            response = await MediaService.UploadAssetAsync(_uploadFile, _assetModel.ThumbnailUrl, _assetModel.DurationSeconds);
            _uploadProgress = 100;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(_assetModel.Provider) || string.IsNullOrWhiteSpace(_assetModel.AssetType) || string.IsNullOrWhiteSpace(_assetModel.Url)) { Error("Le type, le provider et l’URL sont obligatoires."); return; }
            response = _editingId.HasValue ? await MediaService.UpdateAssetAsync(_editingId.Value, _assetModel) : await MediaService.CreateExternalAssetAsync(_assetModel);
        }
        if (!Succeeded(response, "Impossible d’enregistrer le média.")) return;
        Success("Média enregistré."); CloseDialog(); await LoadAssetsCoreAsync();
    }

    private async Task OnUploadSelectedAsync(InputFileChangeEventArgs args)
    {
        _fileError = null;
        var file = args.File;
        if (file.Size <= 0 || file.Size > MaximumUploadSize) { _fileError = "Le fichier doit avoir une taille comprise entre 1 octet et 100 Mo."; return; }
        try
        {
            await using var stream = file.OpenReadStream(MaximumUploadSize);
            using var buffer = new MemoryStream((int)file.Size);
            await stream.CopyToAsync(buffer);
            var upload = new MediaUploadFile(file.Name, string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType, buffer.ToArray());
            if (_dialogKind == "ATTACHMENT") _attachmentUploadFile = upload; else _uploadFile = upload;
        }
        catch (IOException) { _fileError = "Impossible de lire ce fichier."; }
    }

    private async Task OpenAttachmentAsync(string ownerType, Guid ownerId, IReadOnlyList<MediaLinkDto> media)
    {
        _loading = true;
        try
        {
            var response = await MediaService.GetAssetsAsync(new MediaFilterState { PageSize = 100 });
            if (response is not { Success: true, Data: not null }) { ShowError(response, "Impossible de charger les médias disponibles."); return; }
            _attachmentAssets = response.Data.Items;
            _attachmentOwnerType = ownerType; _attachmentOwnerId = ownerId; _attachedMedia = media;
            _attachmentAssetId = Guid.Empty; _attachmentRole = "PRIMARY"; _attachmentSortOrder = 0; _attachmentUploadFile = null; _attachmentError = null; _detachingMediaKey = null;
            _dialogKind = "ATTACHMENT"; _dialogTitle = "Gérer les médias associés"; _dialogOpen = true;
        }
        finally { _loading = false; }
    }

    private Task OpenCollectionAttachment(MediaCollectionDto item) => OpenAttachmentAsync("COLLECTION", item.Id, item.Media);
    private Task OpenItemAttachment(ContentItemDto item) => OpenAttachmentAsync("ITEM", item.Id, item.Media);

    private async Task SaveAttachmentAsync()
    {
        var assetId = _attachmentAssetId;
        if (_attachmentUploadFile is not null)
        {
            _uploadProgress = 35;
            var upload = await MediaService.UploadAssetAsync(_attachmentUploadFile, null, null);
            if (upload is not { Success: true, Data: not null }) { ShowError(upload, "Le téléversement a échoué."); return; }
            assetId = upload.Data.Id;
            _uploadProgress = 100;
        }
        if (assetId == Guid.Empty) { Error("Choisissez un média existant ou téléversez un fichier."); return; }
        var input = new MediaAttachmentInputDto { MediaAssetId = assetId, Role = _attachmentRole, SortOrder = _attachmentSortOrder };
        var response = _attachmentOwnerType == "COLLECTION"
            ? await MediaService.AttachCollectionMediaAsync(_attachmentOwnerId, input)
            : await MediaService.AttachItemMediaAsync(_attachmentOwnerId, input);
        if (!Succeeded(response, "Impossible d’attacher ce média.")) return;
        Success("Média attaché."); CloseDialog();
        if (_attachmentOwnerType == "COLLECTION") await LoadCollectionsCoreAsync(); else await LoadItemsCoreAsync();
    }

    private async Task DetachAsync(MediaLinkDto media)
    {
        var key = MediaKey(media);
        if (_detachingMediaKey is not null) return;

        _detachingMediaKey = key;
        _attachmentError = null;
        try
        {
            var response = _attachmentOwnerType == "COLLECTION"
                ? await MediaService.DetachCollectionMediaAsync(_attachmentOwnerId, media.Id, media.Role)
                : await MediaService.DetachItemMediaAsync(_attachmentOwnerId, media.Id, media.Role);
            if (response is not { Success: true })
            {
                _attachmentError = ResponseMessage(response, "Impossible de détacher ce média.");
                return;
            }

            _attachedMedia = _attachedMedia.Where(x => x.Id != media.Id || x.Role != media.Role).ToList();
            Success("Média détaché.");
            if (_attachmentOwnerType == "COLLECTION") await LoadCollectionsCoreAsync(); else await LoadItemsCoreAsync();
        }
        catch (Exception ex)
        {
            _attachmentError = $"Impossible de détacher ce média : {ex.Message}";
        }
        finally
        {
            _detachingMediaKey = null;
        }
    }

    private static string MediaKey(MediaLinkDto media) => $"{media.Id:D}:{media.Role}";
    private bool IsDetaching(MediaLinkDto media) => _detachingMediaKey == MediaKey(media);

    private string? GetMediaPreviewUrl(MediaLinkDto media)
    {
        var value = string.Equals(media.AssetType, "IMAGE", StringComparison.OrdinalIgnoreCase)
            ? media.Url
            : media.ThumbnailUrl;
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (Uri.TryCreate(value, UriKind.Absolute, out var absolute)) return absolute.ToString();

        var configuredBase = new Uri(ApiOptions.Value.BaseUrl.TrimEnd('/') + "/");
        var origin = new Uri(configuredBase.GetLeftPart(UriPartial.Authority) + "/");
        return new Uri(origin, value.TrimStart('/')).ToString();
    }

    private static string MediaTypeIcon(string type) => type.ToUpperInvariant() switch
    {
        "VIDEO" => "e-video",
        "AUDIO" => "e-audio",
        "DOCUMENT" => "e-file-document",
        _ => "e-image"
    };

    private async Task DeleteCategoryAsync(MediaCategoryDto item)
    {
        if (!await ConfirmDeleteAsync(item.Name)) return;
        var response = await MediaService.DeleteCategoryAsync(item.Id);
        if (Succeeded(response, "Impossible de supprimer la catégorie.")) { Success("Catégorie supprimée."); await LoadCategoriesCoreAsync(); }
    }

    private async Task DeleteCollectionAsync(MediaCollectionDto item)
    {
        if (!await ConfirmDeleteAsync(item.Title)) return;
        var response = await MediaService.DeleteCollectionAsync(item.Id);
        if (Succeeded(response, "Impossible de supprimer la collection.")) { Success("Collection supprimée."); await Task.WhenAll(LoadCollectionsCoreAsync(), LoadCollectionLookupCoreAsync(), LoadCategoriesCoreAsync()); }
    }

    private async Task DeleteItemAsync(ContentItemDto item)
    {
        if (!await ConfirmDeleteAsync(item.Title)) return;
        var response = await MediaService.DeleteItemAsync(item.Id);
        if (Succeeded(response, "Impossible de supprimer le contenu.")) { Success("Contenu supprimé."); await Task.WhenAll(LoadItemsCoreAsync(), LoadCategoriesCoreAsync()); }
    }

    private async Task DeleteAssetAsync(MediaAssetDto item)
    {
        if (!await ConfirmDeleteAsync(item.Url ?? item.Id.ToString())) return;
        var response = await MediaService.DeleteAssetAsync(item.Id);
        if (Succeeded(response, "Impossible de supprimer le média. Détachez-le d’abord s’il est utilisé.")) { Success("Média supprimé."); await LoadAssetsCoreAsync(); }
    }

    private Task<bool> ConfirmDeleteAsync(string name) => DialogService.ConfirmAsync($"Supprimer « {name} » ? Cette action est irréversible.", "Confirmation");

    private async Task OpenFeedDetailAsync(MediaFeedItemDto item)
    {
        var response = await MediaService.GetFeedItemAsync(item.Id);
        if (response is not { Success: true, Data: not null }) { ShowError(response, "Impossible de charger ce contenu."); return; }
        _feedDetail = response.Data; _dialogKind = "FEED"; _dialogTitle = item.Title; _dialogOpen = true;
    }

    private void CloseDialog()
    {
        if (_saving) return;
        _dialogOpen = false; _dialogKind = string.Empty; _editingId = null; _uploadFile = null; _attachmentUploadFile = null; _uploadProgress = 0; _fileError = null; _attachmentError = null; _detachingMediaKey = null;
    }

    private void ShowError<T>(DataResponse<T>? response, string fallback) => Error(ResponseMessage(response, fallback));
    private bool Succeeded<T>(DataResponse<T>? response, string fallback)
    {
        if (response is { Success: true }) return true;
        ShowError(response, fallback); return false;
    }

    private static string ResponseMessage<T>(DataResponse<T>? response, string fallback)
    {
        if (!string.IsNullOrWhiteSpace(response?.Message)) return response.Message;
        var errors = string.Join(" ", response?.Error?.Where(x => !string.IsNullOrWhiteSpace(x)) ?? []);
        return string.IsNullOrWhiteSpace(errors) ? fallback : errors;
    }

    private void Error(string message) { _notice = message; _noticeIsError = true; }
    private void Success(string message) { _notice = message; _noticeIsError = false; }

    private static ContentItemInputDto NewItemModel() => new() { ItemType = "ARTICLE", Status = "DRAFT" };
    private static MediaAssetInputDto NewAssetModel() => new() { AssetType = "IMAGE", Provider = "YOUTUBE" };
    private IEnumerable<MediaCollectionDto> ItemCollections => _collectionLookup.Where(x => _itemModel.CategoryId == Guid.Empty || x.CategoryId == _itemModel.CategoryId);
    private IEnumerable<MediaCollectionDto> FilterCollections => _collectionLookup.Where(x => !_itemFilter.CategoryId.HasValue || x.CategoryId == _itemFilter.CategoryId.Value);

    public void Dispose()
    {
        _searchCts?.Cancel();
        _searchCts?.Dispose();
    }
}

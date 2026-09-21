using ChandorProject.Shared.Models;

namespace ChandorProject.Shared.DTOs.Media;

public class MediaCategoryInputDto
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public string IconColor { get; set; } = string.Empty;
    public string IconBackground { get; set; } = string.Empty;
    public string ModuleType { get; set; } = "CONTENT";
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class MediaCategoryDto : MediaCategoryInputDto
{
    public Guid Id { get; set; }
    public int CollectionCount { get; set; }
    public int ItemCount { get; set; }
}

public class MediaCollectionInputDto
{
    public Guid CategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "DRAFT";
    public int SortOrder { get; set; }
    public DateTime? PublishedAt { get; set; }
}

public class MediaCollectionDto : MediaCollectionInputDto
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string CategoryCode { get; set; } = string.Empty;
    public int ItemCount { get; set; }
    public IReadOnlyList<MediaLinkDto> Media { get; set; } = [];
}

public class MediaAssetInputDto
{
    public string AssetType { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? Url { get; set; }
    public string? StorageKey { get; set; }
    public string? MimeType { get; set; }
    public string? ThumbnailUrl { get; set; }
    public int? DurationSeconds { get; set; }
}

public class MediaAssetDto : MediaAssetInputDto
{
    public Guid Id { get; set; }
}

public class MediaAssetPageDto : PagedResult<MediaAssetDto> { }

public class MediaAttachmentInputDto
{
    public Guid MediaAssetId { get; set; }
    public string Role { get; set; } = "PRIMARY";
    public int SortOrder { get; set; }
}

public class MediaLinkDto : MediaAssetDto
{
    public string Role { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

public class ContentItemInputDto
{
    public Guid CategoryId { get; set; }
    public Guid? CollectionId { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string ItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "DRAFT";
    public int SortOrder { get; set; }
    public DateTime? PublishedAt { get; set; }
    public EpisodeDetailInputDto? Episode { get; set; }
    public EventDetailInputDto? Event { get; set; }
    public QuoteDetailInputDto? Quote { get; set; }
    public ProductDetailInputDto? Product { get; set; }
}

public class ContentItemDto : ContentItemInputDto
{
    public Guid Id { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string? CollectionTitle { get; set; }
    public IReadOnlyList<MediaLinkDto> Media { get; set; } = [];
}

public class EpisodeDetailInputDto
{
    public int EpisodeNumber { get; set; }
    public string Speaker { get; set; } = string.Empty;
    public DateTime? RecordedAt { get; set; }
    public int? DurationSeconds { get; set; }
}

public class EventDetailInputDto
{
    public DateTime StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }
    public string Location { get; set; } = string.Empty;
    public string? RegistrationUrl { get; set; }
}

public class QuoteDetailInputDto
{
    public string QuoteText { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
}

public class ProductDetailInputDto
{
    public string Sku { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public int StockQuantity { get; set; }
    public string? PurchaseUrl { get; set; }
}

public class MediaCollectionPageDto : PagedResult<MediaCollectionDto> { }
public class ContentItemPageDto : PagedResult<ContentItemDto> { }

public class MediaFeedItemDto
{
    public Guid Id { get; set; }
    public string SourceType { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string CategoryCode { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public Guid? CollectionId { get; set; }
    public string? CollectionTitle { get; set; }
    public string ItemType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime ContentDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Location { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? PosterLink { get; set; }
    public string? VideoLink { get; set; }
}

public class MediaFeedDetailDto : MediaFeedItemDto
{
    public string? Slug { get; set; }
    public string? RecurrenceRule { get; set; }
    public string? RecurrenceException { get; set; }
    public IReadOnlyList<MediaLinkDto> Media { get; set; } = [];
    public EpisodeDetailInputDto? Episode { get; set; }
    public EventDetailInputDto? Event { get; set; }
    public QuoteDetailInputDto? Quote { get; set; }
    public ProductDetailInputDto? Product { get; set; }
}

public class MediaFeedPageDto : PagedResult<MediaFeedItemDto>
{
    public string Period { get; set; } = string.Empty;
    public string Keyword { get; set; } = string.Empty;
}

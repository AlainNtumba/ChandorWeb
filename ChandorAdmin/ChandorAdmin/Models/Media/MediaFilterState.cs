namespace ChandorAdmin.Models.Media;

public sealed class MediaFilterState
{
    public Guid? CategoryId { get; set; }
    public Guid? CollectionId { get; set; }
    public string? Status { get; set; }
    public string? ItemType { get; set; }
    public string? AssetType { get; set; }
    public string? Provider { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

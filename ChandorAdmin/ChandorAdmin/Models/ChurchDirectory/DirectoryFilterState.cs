namespace ChandorAdmin.Models.ChurchDirectory;

public sealed class DirectoryFilterState
{
    public Guid? TypeId { get; set; }
    public Guid? ParentId { get; set; }
    public bool? IsActive { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

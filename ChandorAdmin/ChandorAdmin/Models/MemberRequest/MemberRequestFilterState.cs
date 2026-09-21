namespace ChandorAdmin.Models.MemberRequest;

public sealed class MemberRequestFilterState
{
    public string? Type { get; set; }
    public string? Status { get; set; }
    public Guid? MemberId { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class RequestTypeFilterState
{
    public bool? IsActive { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

namespace ChandorAdmin.Models.Notification;

public sealed class NotificationAdminFilterState
{
    public string? Status { get; set; }
    public string? AudienceType { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool? IsActive { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public sealed class NotificationRecipientFilterState
{
    public string? State { get; set; }
    public string Keyword { get; set; } = string.Empty;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

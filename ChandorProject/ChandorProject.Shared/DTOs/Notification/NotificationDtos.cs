using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.Notification;

public class CreateNotificationDto
{
    [Required(ErrorMessage = "This field is required.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public string Content { get; set; } = string.Empty;

    public string Link { get; set; } = string.Empty;
    public bool CanExpire { get; set; }
    public int Level { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public Guid UserId { get; set; }
}

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Link { get; set; } = string.Empty;
    public bool CanExpire { get; set; }
    public int Level { get; set; }
    public DateTime DateStart { get; set; }
    public DateTime DateEnd { get; set; }
    public Guid UserId { get; set; }
    public string Status { get; set; } = "Active";
}

public class NotificationInputDto
{
    [Required, MaxLength(150)] public string Title { get; set; } = string.Empty;
    [Required] public string Message { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string AudienceType { get; set; } = string.Empty;
    [Required, MaxLength(20)] public string Status { get; set; } = "DRAFT";
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> MemberIds { get; set; } = [];
    public List<Guid> MemberTypeIds { get; set; } = [];
    public List<Guid> AgeGroupIds { get; set; } = [];
    public List<Guid> DepartmentTeamIds { get; set; } = [];
}

public class NotificationAdminDto : NotificationInputDto
{
    public Guid Id { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int RecipientCount { get; set; }
    public int DeliveredCount { get; set; }
    public int ReadCount { get; set; }
    public int DismissedCount { get; set; }
}

public class NotificationRecipientAdminDto
{
    public Guid MemberId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public DateTime? DeliveredAt { get; set; }
    public DateTime? ReadAt { get; set; }
    public DateTime? DismissedAt { get; set; }
    public bool IsRead => ReadAt.HasValue;
}

public class NotificationSummaryDto
{
    public int TotalCount { get; set; }
    public int DraftCount { get; set; }
    public int PublishedCount { get; set; }
    public int ArchivedCount { get; set; }
    public int ActiveCount { get; set; }
    public int ScheduledCount { get; set; }
    public int ExpiredCount { get; set; }
    public int TotalRecipients { get; set; }
    public int TotalRead { get; set; }
}

public class MemberNotificationDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string AudienceType { get; set; } = string.Empty;
    public string SenderName { get; set; } = "Chandelier";
    public DateTime? PublishedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsRead { get; set; }
}

public class UpdateNotificationReadDto
{
    public bool IsRead { get; set; }
}

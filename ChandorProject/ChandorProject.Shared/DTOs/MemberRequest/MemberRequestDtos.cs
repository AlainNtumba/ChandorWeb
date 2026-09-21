using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.MemberRequest;

public class CreateMemberRequestDto
{
    [Required] public Guid RequestTypeId { get; set; }
    public Guid? MemberId { get; set; }
    [Required, MaxLength(100)] public string LastName { get; set; } = string.Empty;
    [MaxLength(100)] public string? MiddleName { get; set; }
    [Required, MaxLength(100)] public string FirstName { get; set; } = string.Empty;
    [Required, MaxLength(55)] public string Phone { get; set; } = string.Empty;
    [MaxLength(255)] public string? Address { get; set; }
    [Required, MaxLength(500)] public string Subject { get; set; } = string.Empty;
    [Required] public string Message { get; set; } = string.Empty;
}

public class UpdateMemberRequestDto : CreateMemberRequestDto
{
    [Required, MaxLength(30)] public string Status { get; set; } = "PENDING";
}

public class UpdateMemberRequestStatusDto
{
    [Required, MaxLength(30)] public string Status { get; set; } = string.Empty;
}

public class MemberRequestDto
{
    public Guid Id { get; set; }
    public Guid RequestTypeId { get; set; }
    public string RequestTypeCode { get; set; } = string.Empty;
    public string RequestTypeOptionsName { get; set; } = string.Empty;
    public Guid? MemberId { get; set; }
    public string LastName { get; set; } = string.Empty;
    public string? MiddleName { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class MemberRequestTypeCountDto
{
    public string TypeCode { get; set; } = string.Empty;
    public string TypeName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class MemberRequestSummaryDto
{
    public int TotalCount { get; set; }
    public int PendingCount { get; set; }
    public int InProgressCount { get; set; }
    public int ProcessedCount { get; set; }
    public int RejectedCount { get; set; }
    public int CancelledCount { get; set; }
    public IReadOnlyList<MemberRequestTypeCountDto> ByType { get; set; } = [];
}

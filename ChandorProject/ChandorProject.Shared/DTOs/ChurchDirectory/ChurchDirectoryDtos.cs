using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.ChurchDirectory;

public class ChurchDirectoryTypeInputDto
{
    [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    [Required, MaxLength(30)] public string DisplayKind { get; set; } = string.Empty;
    [MaxLength(50)] public string? Icon { get; set; }
    [MaxLength(500)] public string? HeroImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ChurchDirectoryTypeDto : ChurchDirectoryTypeInputDto
{
    public Guid Id { get; set; }
    public int ItemCount { get; set; }
    public int ActiveItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ChurchLocationDetailsDto
{
    [MaxLength(200)] public string? Area { get; set; }
    [MaxLength(500)] public string? Address { get; set; }
    [MaxLength(500)] public string? ReferencePoint { get; set; }
    [Range(typeof(decimal), "-90", "90")] public decimal? Latitude { get; set; }
    [Range(typeof(decimal), "-180", "180")] public decimal? Longitude { get; set; }
}

public class ChurchDepartmentDetailsDto
{
    public string? Organization { get; set; }
    public string? Mission { get; set; }
    public string? History { get; set; }
}

public class ChurchDirectoryContactDto
{
    public Guid? Id { get; set; }
    [Required, MaxLength(30)] public string Type { get; set; } = string.Empty;
    [MaxLength(100)] public string? Label { get; set; }
    [Required, MaxLength(250)] public string Value { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
}

public class ChurchDirectoryMemberDto
{
    public Guid? Id { get; set; }
    [Required] public Guid MemberId { get; set; }
    [MaxLength(100)] public string Role { get; set; } = string.Empty;
    public bool IsPrimary { get; set; }
    public string? MemberName { get; set; }
}

public class ChurchDirectoryItemInputDto
{
    [Required] public Guid TypeId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? DepartmentId { get; set; }
    [Required, MaxLength(80)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    [MaxLength(500)] public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public ChurchLocationDetailsDto? LocationDetails { get; set; }
    public ChurchDepartmentDetailsDto? DepartmentDetails { get; set; }
    public List<ChurchDirectoryContactDto> Contacts { get; set; } = [];
    public List<ChurchDirectoryMemberDto> Members { get; set; } = [];
}

public class ChurchDirectoryItemDto : ChurchDirectoryItemInputDto
{
    public Guid Id { get; set; }
    public string TypeCode { get; set; } = string.Empty;
    public string DisplayKind { get; set; } = string.Empty;
    public string? ParentName { get; set; }
    public string? DepartmentName { get; set; }
    public int ChildCount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class ChurchDirectoryTreeItemDto : ChurchDirectoryItemDto
{
    public List<ChurchDirectoryTreeItemDto> Children { get; set; } = [];
}

/// <summary>
/// Simplified payload used by the administration form to save a directory type
/// and all of its location items in one transaction.
/// </summary>
public class ChurchDirectoryTypeWithItemsInputDto
{
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    public List<ChurchDirectorySimpleItemInputDto> Items { get; set; } = [];
}

public class ChurchDirectorySimpleItemInputDto
{
    public Guid? Id { get; set; }
    [Required, MaxLength(150)] public string Name { get; set; } = string.Empty;
    [Required] public Guid ResponsibleMemberId { get; set; }
    [Required, MaxLength(500)] public string Address { get; set; } = string.Empty;
    [Required, MaxLength(250)] public string Contact { get; set; } = string.Empty;
}

public class ChurchDirectoryTypeWithItemsDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string DisplayKind { get; set; } = string.Empty;
    public string? HeroImageUrl { get; set; }
    public List<ChurchDirectorySimpleItemDto> Items { get; set; } = [];
}

public class ChurchDirectorySimpleItemDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid ResponsibleMemberId { get; set; }
    public string ResponsibleMemberName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}

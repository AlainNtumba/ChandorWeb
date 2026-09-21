using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.RequestType;

public class CreateRequestTypeDto
{
    [Required, MaxLength(50)] public string Code { get; set; } = string.Empty;
    [Required, MaxLength(255)] public string OptionsName { get; set; } = string.Empty;
    [MaxLength(500)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateRequestTypeDto : CreateRequestTypeDto;

public class RequestTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string OptionsName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int RequestCount { get; set; }
}

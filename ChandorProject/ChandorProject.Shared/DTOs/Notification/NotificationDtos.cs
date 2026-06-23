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

namespace ChandorProject.Shared.DTOs.ChurchProgram;

public class CongrationProgramFeedDto
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string Theme { get; set; } = string.Empty;
    public string Lieu { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string RecurrenceRule { get; set; } = string.Empty;
    public string RecurrenceException { get; set; } = string.Empty;
    public string PosterLink { get; set; } = string.Empty;
    public string VideoLink { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public Guid ProgramTypeId { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid DepartmentTeamId { get; set; }
}

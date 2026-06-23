using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.MemberActivity;

public class MemberActivityDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid DepartmentTeamId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberRoleId { get; set; }
}

public class NewMemberActivityDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid DepartmentTeamId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberRoleId { get; set; }
}

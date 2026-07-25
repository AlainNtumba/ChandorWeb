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

public class MemberResponsibilityDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Postname { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
    public string MemberType { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
    public string RoleDescription { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public Guid MemberId { get; set; }
    public Guid DepartmentTeamId { get; set; }
    public Guid MemberRoleId { get; set; }
}

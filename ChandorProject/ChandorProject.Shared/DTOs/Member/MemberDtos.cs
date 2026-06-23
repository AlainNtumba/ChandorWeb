using System.ComponentModel.DataAnnotations;

namespace ChandorProject.Shared.DTOs.Member;

public class MemberDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Surname { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Postname { get; set; } = string.Empty;

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime Birthday { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public bool Gender { get; set; }

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Country { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Town { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Suburb { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Note { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid AgeGroupId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberTypeId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid UserId { get; set; }

    public List<string> Emails { get; set; } = [];

    public List<string> PhoneNumbers { get; set; } = [];
}

public class MemberDetailsDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Surname { get; set; } = string.Empty;
    public string Postname { get; set; } = string.Empty;
    public DateTime Birthday { get; set; }
    public bool Gender { get; set; }
    public string GenderDescription { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    public string Town { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public string AgeGroup { get; set; } = string.Empty;
    public string MemberType { get; set; } = string.Empty;
    public string LastUpdatedBy { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
    public DateTime Created { get; set; }
    public Guid AgeGroupId { get; set; }
    public Guid MemberTypeId { get; set; }
}

public class NewMemberDto
{
    [Required(ErrorMessage = "This field is required.")]
    [StringLength(256, MinimumLength = 5, ErrorMessage = "Username email must be between 5 and 256 characters.")]
    [EmailAddress(ErrorMessage = "Username must be a valid email address.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Surname { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Postname { get; set; } = string.Empty;

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime Birthday { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public bool Gender { get; set; }

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Country { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Town { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Suburb { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Note { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid AgeGroupId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberTypeId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid UserId { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? NumPhone1 { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format.")]
    public string? NumPhone2 { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email1 { get; set; }
    
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string? Email2 { get; set; }
}

public class UpdateMemberDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(256, MinimumLength = 5, ErrorMessage = "Username email must be between 5 and 256 characters.")]
    [EmailAddress(ErrorMessage = "Username must be a valid email address.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Surname { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Postname { get; set; } = string.Empty;

    [DataType(DataType.Date, ErrorMessage = "Invalid date format.")]
    public DateTime Birthday { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public bool Gender { get; set; }

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Country { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Town { get; set; } = string.Empty;

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Suburb { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Address { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string Note { get; set; } = string.Empty;

    [Required(ErrorMessage = "This field is required.")]
    public Guid AgeGroupId { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    public Guid MemberTypeId { get; set; }
}


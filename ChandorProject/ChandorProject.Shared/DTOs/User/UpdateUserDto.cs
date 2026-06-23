using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChandorProject.Shared.DTOs.User;

public class UpdateUserDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string? Username { get; set; }

    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string? Password { get; set; }

    [StringLength(256, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string? Salt { get; set; }
}

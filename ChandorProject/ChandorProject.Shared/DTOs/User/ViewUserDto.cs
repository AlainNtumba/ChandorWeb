using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChandorProject.Shared.DTOs.User;

public class ViewUserDto
{
    [Required(ErrorMessage = "This field is required.")]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "This field is required.")]
    [StringLength(100, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string? Username { get; set; }

    [StringLength(500, ErrorMessage = "The field exceeds the maximum allowed length.")]
    public string? ProfilPicture { get; set; }

    [DataType(DataType.DateTime, ErrorMessage = "Invalid date time format.")]
    public DateTime Updated { get; set; }
}

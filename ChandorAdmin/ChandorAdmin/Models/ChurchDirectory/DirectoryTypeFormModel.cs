using System.ComponentModel.DataAnnotations;

namespace ChandorAdmin.Models.ChurchDirectory;

public sealed class DirectoryTypeFormModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "Le nom du type est obligatoire.")]
    [MaxLength(150, ErrorMessage = "Le nom ne peut pas dépasser 150 caractères.")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500, ErrorMessage = "La description ne peut pas dépasser 500 caractères.")]
    public string? Description { get; set; }

    public string? HeroImageUrl { get; set; }
    public List<DirectoryItemFormModel> Items { get; set; } = [];
}

public sealed class DirectoryItemFormModel
{
    public Guid? Id { get; set; }

    public bool IsActive { get; set; } = true;

    [Required(ErrorMessage = "Le nom de l’élément est obligatoire.")]
    [MaxLength(150, ErrorMessage = "Le nom ne peut pas dépasser 150 caractères.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sélectionnez un responsable.")]
    public Guid ResponsibleMemberId { get; set; }

    [Required(ErrorMessage = "L’adresse est obligatoire.")]
    [MaxLength(500, ErrorMessage = "L’adresse ne peut pas dépasser 500 caractères.")]
    public string Address { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le contact est obligatoire.")]
    [MaxLength(250, ErrorMessage = "Le contact ne peut pas dépasser 250 caractères.")]
    public string Contact { get; set; } = string.Empty;
}

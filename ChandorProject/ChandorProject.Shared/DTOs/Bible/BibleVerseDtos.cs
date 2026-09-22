using System.ComponentModel.DataAnnotations;
using ChandorProject.Shared.Models;

namespace ChandorProject.Shared.DTOs.Bible;

public class CreateBibleVerseDto
{
    [Required(ErrorMessage = "Le livre est obligatoire.")]
    public string BookName { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Le chapitre doit être supérieur ou égal à 1.")]
    public int Chapter { get; set; } = 1;

    [Required(ErrorMessage = "Le verset ou la plage est obligatoire.")]
    public string Verse { get; set; } = string.Empty;

    [Required(ErrorMessage = "Le contenu est obligatoire.")]
    public string Content { get; set; } = string.Empty;

    [Required(ErrorMessage = "La pensée du jour est obligatoire.")]
    public string DailyThought { get; set; } = string.Empty;

    public DateOnly PublicationDate { get; set; } = DateOnly.FromDateTime(DateTime.Today);
}

public class UpdateBibleVerseDto : CreateBibleVerseDto;

public class BibleVerseDto : CreateBibleVerseDto
{
    public Guid Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public DateTime Created { get; set; }
    public DateTime Updated { get; set; }
}

public class BibleVersePageDto : PagedResult<BibleVerseDto>;

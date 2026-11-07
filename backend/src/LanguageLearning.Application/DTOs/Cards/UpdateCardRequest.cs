
using System.ComponentModel.DataAnnotations;

namespace LanguageLearning.Application.DTOs.Cards;

public class UpdateCardRequest
{
    [Required]
    [Range(1, int.MaxValue)]
    public int? BaseVersion { get; set; }

    public string? Front { get; set; }

    public string? Back { get; set; }
}

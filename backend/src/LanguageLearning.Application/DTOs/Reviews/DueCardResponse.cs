
namespace LanguageLearning.Application.DTOs.Reviews;

public sealed class DueCardResponse
{
    public Guid CardId { get; set; }

    public Guid DeckId { get; set; }

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public DateTime DueAt { get; set; }

    public string AlgorithmVersion { get; set; } = string.Empty;

    public ReviewIntervalsResponse? Intervals { get; set; }
}

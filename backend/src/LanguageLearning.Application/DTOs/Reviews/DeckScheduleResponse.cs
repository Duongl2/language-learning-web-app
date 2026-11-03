
namespace LanguageLearning.Application.DTOs.Reviews;

public sealed class DeckScheduleResponse
{
    public Guid CardId { get; set; }

    public DateTime DueAt { get; set; }

    public string StateJson { get; set; } = "{}";

    public string AlgorithmVersion { get; set; } = "srs-v1";

    public DateTime? LastReviewedAt { get; set; }
}


namespace LanguageLearning.Application.DTOs.Reviews;

public sealed class SubmitReviewResponse
{
    public Guid CardId { get; set; }

    public Guid ClientEventId { get; set; }

    public bool AlreadyProcessed { get; set; }

    public DateTime? NextDueAt { get; set; }
}

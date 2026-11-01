
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.DTOs.Reviews;

public sealed class SubmitReviewRequest
{
    public Guid CardId { get; set; }

    public Guid ClientEventId { get; set; }

    public SrsRating Rating { get; set; }
}

using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LanguageLearning.Application.DTOs.Learning;

public sealed record QuizAnswerInput(Guid QuestionId, JsonElement Answer);

public sealed class QuizSubmission
{
    public Guid AttemptId { get; set; }
    [Required] public List<QuizAnswerInput> Answers { get; set; } = new();
}

public sealed record AnswerResult(Guid QuestionId, bool IsCorrect, string Explanation, string CorrectAnswer);

public sealed record QuizResult(Guid Id, Guid LessonId, int Score, int MaxScore,
    DateTime CompletedAt, IReadOnlyList<AnswerResult> Answers);

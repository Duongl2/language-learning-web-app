using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LanguageLearning.Application.DTOs.Learning;

public sealed record QuizOptionDto(Guid Id, string Content);
public sealed record MatchingItem(string Id, string Text);
public sealed record QuizQuestionDto(Guid Id, string Type, string Prompt,
    IReadOnlyList<QuizOptionDto> Options, IReadOnlyList<MatchingItem> Left,
    IReadOnlyList<MatchingItem> Right);
public sealed record QuizAnswerInput(Guid QuestionId, JsonElement Answer);
public sealed class QuizSubmission
{
    public Guid AttemptId { get; set; }
    [Required] public List<QuizAnswerInput> Answers { get; set; } = new();
}
public sealed record AnswerResult(Guid QuestionId, bool IsCorrect, string Explanation, string CorrectAnswer);
public sealed record QuizResult(Guid Id, Guid LessonId, int Score, int MaxScore,
    DateTime CompletedAt, IReadOnlyList<AnswerResult> Answers);
public sealed record LessonProgressDto(Guid LessonId, string Title, string Status);
public sealed record CourseProgressDto(Guid CourseId, string Title, int CompletedLessons,
    int TotalLessons, double Percent, IReadOnlyList<LessonProgressDto> Lessons);
public sealed record ResumeLessonDto(Guid LessonId, string Title, Guid CourseId);
public sealed record ProgressResponse(IReadOnlyList<CourseProgressDto> Courses, ResumeLessonDto? Resume);
public sealed class UpdateProgressRequest
{
    [Required] public string Status { get; set; } = "InProgress";
}
public sealed class GoalRequest
{
    [Range(1, 1000)] public int DailyTarget { get; set; } = 10;
}
public sealed record ActivityDay(string Date, int ReviewCount, int LessonsCompleted, bool GoalMet);
public sealed record RatingCount(string Rating, int Count);
public sealed record StatisticsOverview(int NewCards, int LearnedCards, int DueCards,
    int ReviewCount, double Retention, IReadOnlyList<RatingCount> Ratings);
public sealed record StreakResponse(int CurrentStreak, int LongestStreak, int DailyTarget,
    int TodayReviews, string Timezone, IReadOnlyList<ActivityDay> Days);
public sealed record CsvRow(int Line, string Front, string Back, string? Error);
public sealed record CsvPreview(IReadOnlyList<CsvRow> Rows, int ValidCount, int InvalidCount);
public sealed class CsvRequest
{
    [Required, MaxLength(2_000_000)] public string Content { get; set; } = string.Empty;
}
public sealed record PublicDeckDto(Guid Id, Guid LanguageId, string Title, string Description,
    int CardCount, IReadOnlyList<string> Tags);
public sealed record CardPreview(string Front, string Back);
public sealed record PublicDeckPreview(PublicDeckDto Deck, IReadOnlyList<CardPreview> Cards);

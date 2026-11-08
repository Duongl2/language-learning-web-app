using System.Text.Json;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.Services;

public sealed class QuizService(ILearningRepository repository, ICurrentUserService currentUser)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private sealed class QuestionConfig
    {
        public List<string> Answers { get; set; } = new();
        public List<MatchingItem> Left { get; set; } = new();
        public List<MatchingItem> Right { get; set; } = new();
        public Dictionary<string, string> Pairs { get; set; } = new();
    }

    private static QuestionConfig Config(Question question)
    {
        try { return JsonSerializer.Deserialize<QuestionConfig>(question.ConfigJson, JsonOptions) ?? new(); }
        catch (JsonException) { throw new ArgumentException("Question configuration is invalid."); }
    }

    public async Task<List<QuizQuestionDto>> GetAsync(Guid lessonId, CancellationToken ct)
    {
        await RequireLessonAsync(lessonId, ct);
        return (await repository.GetQuestionsAsync(lessonId, ct)).Select(question =>
        {
            var config = question.Type == QuestionType.Matching ? Config(question) : new QuestionConfig();
            return new QuizQuestionDto(question.Id, question.Type.ToString(), question.Prompt,
                question.Options.OrderBy(x => x.SortOrder).Select(x => new QuizOptionDto(x.Id, x.Content)).ToList(),
                config.Left, config.Right.OrderBy(x => x.Id, StringComparer.Ordinal).ToList());
        }).ToList();
    }

    public static bool IsCorrect(Question question, JsonElement answer)
    {
        switch (question.Type)
        {
            case QuestionType.MultipleChoice:
                return answer.ValueKind == JsonValueKind.String && Guid.TryParse(answer.GetString(), out var optionId) &&
                    question.Options.Any(x => x.Id == optionId && x.IsCorrect);
            case QuestionType.FillInBlank:
                return answer.ValueKind == JsonValueKind.String && Config(question).Answers.Any(x =>
                    string.Equals(x.Trim().Normalize(), answer.GetString()?.Trim().Normalize(), StringComparison.OrdinalIgnoreCase));
            case QuestionType.Matching:
                var pairs = Config(question).Pairs;
                if (answer.ValueKind != JsonValueKind.Object || pairs.Count == 0) return false;
                var properties = answer.EnumerateObject().ToList();
                return properties.Count == pairs.Count && properties.Select(x => x.Name).Distinct().Count() == pairs.Count &&
                    properties.All(x => x.Value.ValueKind == JsonValueKind.String && pairs.TryGetValue(x.Name, out var expected) && expected == x.Value.GetString());
            default: throw new ArgumentException("Unsupported question type.");
        }
    }

    public async Task<QuizResult> SubmitAsync(Guid lessonId, QuizSubmission request, CancellationToken ct)
    {
        var userId = currentUser.GetRequiredUserId();
        await RequireLessonAsync(lessonId, ct);
        if (request.AttemptId == Guid.Empty) throw new ArgumentException("AttemptId is required for safe retries.");
        var existing = await repository.GetAttemptAsync(request.AttemptId, ct);
        if (existing != null)
        {
            if (existing.UserId != userId || existing.LessonId != lessonId) throw new ConflictException("Attempt identifier is already in use.");
            if (request.Answers.Count != existing.Answers.Count || request.Answers.Select(x => x.QuestionId).Distinct().Count() != request.Answers.Count || request.Answers.Any(input =>
                !existing.Answers.Any(saved => saved.QuestionId == input.QuestionId && saved.AnswerJson == input.Answer.GetRawText())))
                throw new ConflictException("Retry answers differ from the original attempt.");
            return Result(existing);
        }
        var questions = await repository.GetQuestionsAsync(lessonId, ct);
        if (questions.Count == 0) throw new ArgumentException("This lesson has no quiz questions.");
        if (request.Answers.Count != questions.Count || request.Answers.Select(x => x.QuestionId).Distinct().Count() != questions.Count ||
            request.Answers.Any(x => !questions.Any(q => q.Id == x.QuestionId)))
            throw new ArgumentException("Submit exactly one answer for every question.");
        var attempt = new QuizAttempt { Id = request.AttemptId, UserId = userId, LessonId = lessonId, MaxScore = questions.Count };
        foreach (var question in questions)
        {
            var input = request.Answers.Single(x => x.QuestionId == question.Id);
            var correct = IsCorrect(question, input.Answer);
            if (correct) attempt.Score++;
            attempt.Answers.Add(new QuizAnswer
            {
                QuestionId = question.Id,
                Question = question,
                AnswerJson = input.Answer.GetRawText(),
                IsCorrect = correct
            });
        }
        await repository.AddAttemptAsync(attempt, ct);
        await repository.SaveChangesAsync(ct);
        return Result(attempt);
    }

    public async Task<List<QuizResult>> HistoryAsync(Guid lessonId, CancellationToken ct)
    {
        await RequireLessonAsync(lessonId, ct);
        return (await repository.GetAttemptsAsync(currentUser.GetRequiredUserId(), lessonId, ct)).Select(Result).ToList();
    }

    private async Task RequireLessonAsync(Guid id, CancellationToken ct)
    {
        if (await repository.GetPublishedLessonAsync(id, ct) == null) throw new NotFoundException("Published lesson not found.");
    }

    private static QuizResult Result(QuizAttempt attempt) => new(attempt.Id, attempt.LessonId, attempt.Score,
        attempt.MaxScore, attempt.CompletedAt, attempt.Answers.Select(x => new AnswerResult(x.QuestionId, x.IsCorrect, x.Question.Explanation, Solution(x.Question))).ToList());

    private static string Solution(Question question) => question.Type switch
    {
        QuestionType.MultipleChoice => string.Join(", ", question.Options.Where(x => x.IsCorrect).OrderBy(x => x.SortOrder).Select(x => x.Content)),
        QuestionType.FillInBlank => string.Join(" / ", Config(question).Answers),
        QuestionType.Matching => string.Join("; ", Config(question).Pairs.Select(pair =>
            $"{Config(question).Left.FirstOrDefault(x => x.Id == pair.Key)?.Text ?? pair.Key} → {Config(question).Right.FirstOrDefault(x => x.Id == pair.Value)?.Text ?? pair.Value}")),
        _ => ""
    };
}

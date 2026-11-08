using System.Text.Json;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.Services;

public sealed class QuizReadService(IQuizReadRepository repository)
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

    private async Task RequireLessonAsync(Guid id, CancellationToken ct)
    {
        if (await repository.GetPublishedLessonAsync(id, ct) == null) throw new NotFoundException("Published lesson not found.");
    }
}

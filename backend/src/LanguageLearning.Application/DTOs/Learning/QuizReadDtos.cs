using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LanguageLearning.Application.DTOs.Learning;

public sealed record QuizOptionDto(Guid Id, string Content);

public sealed record MatchingItem(string Id, string Text);

public sealed record QuizQuestionDto(Guid Id, string Type, string Prompt,
    IReadOnlyList<QuizOptionDto> Options, IReadOnlyList<MatchingItem> Left,
    IReadOnlyList<MatchingItem> Right);

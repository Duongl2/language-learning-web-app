using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.DTOs.AdminContent;

public record LanguageInput(string Code, string Name, bool IsActive = true);
public record LanguageResponse(Guid Id, string Code, string Name, bool IsActive);

public record CourseInput(Guid LanguageId, string Title, string Level, string Description, ContentStatus Status);
public record CourseResponse(Guid Id, Guid LanguageId, string Title, string Level, string Description, ContentStatus Status);

public record UnitInput(Guid CourseId, string Title, int SortOrder);
public record UnitResponse(Guid Id, Guid CourseId, string Title, int SortOrder);

public record LessonInput(Guid UnitId, string Title, string Content, int SortOrder, ContentStatus Status);
public record LessonResponse(Guid Id, Guid UnitId, string Title, string Content, int SortOrder, ContentStatus Status);

public record VocabularyInput(Guid LessonId, string Term, string Phonetic, string Meaning, string Example, string? AudioUrl, string? ImageUrl);
public record VocabularyResponse(Guid Id, Guid LessonId, Guid LanguageId, string Term, string Phonetic, string Meaning, string Example, string? AudioUrl, string? ImageUrl);

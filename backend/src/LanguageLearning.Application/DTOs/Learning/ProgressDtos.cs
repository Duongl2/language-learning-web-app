using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LanguageLearning.Application.DTOs.Learning;

public sealed record LessonProgressDto(Guid LessonId, string Title, string Status);

public sealed record CourseProgressDto(Guid CourseId, string Title, int CompletedLessons,
    int TotalLessons, double Percent, IReadOnlyList<LessonProgressDto> Lessons);

public sealed record ResumeLessonDto(Guid LessonId, string Title, Guid CourseId);

public sealed record ProgressResponse(IReadOnlyList<CourseProgressDto> Courses, ResumeLessonDto? Resume);

public sealed class UpdateProgressRequest
{
    [Required] public string Status { get; set; } = "InProgress";
}

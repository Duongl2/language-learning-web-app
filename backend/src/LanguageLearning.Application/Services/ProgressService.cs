using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.Services;

public sealed class ProgressService(IProgressRepository repository, ICurrentUserService currentUser)
{
    public async Task EnrollAsync(Guid courseId, CancellationToken ct)
    {
        if (await repository.GetPublishedCourseAsync(courseId, ct) == null) throw new NotFoundException("Published course not found.");
        await repository.EnrollAsync(currentUser.GetRequiredUserId(), courseId, ct);
        await repository.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(Guid lessonId, string status, CancellationToken ct)
    {
        if (status is not ("NotStarted" or "InProgress" or "Completed")) throw new ArgumentException("Invalid lesson progress status.");
        var lesson = await repository.GetPublishedLessonAsync(lessonId, ct) ?? throw new NotFoundException("Published lesson not found.");
        var userId = currentUser.GetRequiredUserId();
        await repository.EnrollAsync(userId, lesson.Unit.CourseId, ct);
        await repository.SaveProgressAsync(userId, lessonId, status, ct);
        await repository.SaveChangesAsync(ct);
    }

    public async Task<ProgressResponse> GetAsync(CancellationToken ct)
    {
        var userId = currentUser.GetRequiredUserId();
        var enrollments = await repository.GetEnrollmentsAsync(userId, ct);
        var progress = await repository.GetProgressAsync(userId, ct);
        var courses = enrollments.Select(enrollment =>
        {
            var lessons = enrollment.Course.Units.OrderBy(x => x.SortOrder).SelectMany(x => x.Lessons.OrderBy(l => l.SortOrder))
                .Where(x => x.Status == ContentStatus.Published).Select(lesson => new LessonProgressDto(lesson.Id, lesson.Title,
                    progress.FirstOrDefault(x => x.LessonId == lesson.Id)?.Status ?? "NotStarted")).ToList();
            var completed = lessons.Count(x => x.Status == "Completed");
            return new CourseProgressDto(enrollment.CourseId, enrollment.Course.Title, completed, lessons.Count,
                lessons.Count == 0 ? 0 : Math.Round(100.0 * completed / lessons.Count, 1), lessons);
        }).ToList();
        var visibleIds = courses.SelectMany(x => x.Lessons).Select(x => x.LessonId).ToHashSet();
        var recent = progress.Where(x => x.Status == "InProgress" && visibleIds.Contains(x.LessonId))
            .OrderByDescending(x => x.LastAccessedAt).FirstOrDefault();
        return new ProgressResponse(courses, recent == null ? null : new ResumeLessonDto(recent.LessonId, recent.Lesson.Title, recent.Lesson.Unit.CourseId));
    }
}

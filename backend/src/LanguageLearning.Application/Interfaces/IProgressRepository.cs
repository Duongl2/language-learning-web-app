using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface IProgressRepository
{
    Task<Lesson?> GetPublishedLessonAsync(Guid id, CancellationToken ct);
    Task<Course?> GetPublishedCourseAsync(Guid id, CancellationToken ct);
    Task<List<CourseEnrollment>> GetEnrollmentsAsync(Guid userId, CancellationToken ct);
    Task<List<LessonProgress>> GetProgressAsync(Guid userId, CancellationToken ct);
    Task EnrollAsync(Guid userId, Guid courseId, CancellationToken ct);
    Task SaveProgressAsync(Guid userId, Guid lessonId, string status, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

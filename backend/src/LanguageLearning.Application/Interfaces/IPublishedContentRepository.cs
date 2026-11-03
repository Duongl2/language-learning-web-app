using LanguageLearning.Application.Common;
using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface IPublishedContentRepository
{
    Task<PagedResponse<Course>> ListCoursesAsync(PaginationParams pagination, Guid? languageId, string? level, CancellationToken ct);
    Task<Course?> GetCourseAsync(Guid id, CancellationToken ct);
    Task<PagedResponse<Unit>> ListUnitsAsync(Guid courseId, PaginationParams pagination, CancellationToken ct);
    Task<Unit?> GetUnitAsync(Guid id, CancellationToken ct);
    Task<PagedResponse<Lesson>> ListLessonsAsync(Guid unitId, PaginationParams pagination, CancellationToken ct);
    Task<Lesson?> GetLessonAsync(Guid id, CancellationToken ct);
}

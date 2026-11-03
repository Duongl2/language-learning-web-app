using LanguageLearning.Application.Common;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public class PublishedContentRepository : IPublishedContentRepository
{
    private readonly AppDbContext _context;

    public PublishedContentRepository(AppDbContext context) => _context = context;

    public async Task<PagedResponse<Course>> ListCoursesAsync(PaginationParams pagination, Guid? languageId, string? level, CancellationToken ct)
    {
        var query = _context.Courses.AsNoTracking().Where(course =>
            course.Status == ContentStatus.Published && course.Language.IsActive);
        if (languageId.HasValue)
            query = query.Where(course => course.LanguageId == languageId.Value);
        if (!string.IsNullOrWhiteSpace(level))
        {
            var selectedLevel = level.Trim().ToUpperInvariant();
            query = query.Where(course => course.Level.ToUpper() == selectedLevel);
        }
        var total = await query.CountAsync(ct);
        var items = await query.OrderBy(course => course.Title).ThenBy(course => course.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync(ct);
        return new PagedResponse<Course>(items, pagination.Page, pagination.PageSize, total);
    }

    public Task<Course?> GetCourseAsync(Guid id, CancellationToken ct) =>
        _context.Courses.AsNoTracking().FirstOrDefaultAsync(course =>
            course.Id == id && course.Status == ContentStatus.Published && course.Language.IsActive, ct);

    public async Task<PagedResponse<Unit>> ListUnitsAsync(Guid courseId, PaginationParams pagination, CancellationToken ct)
    {
        var query = _context.Units.AsNoTracking().Where(unit => unit.CourseId == courseId &&
            unit.Course.Status == ContentStatus.Published && unit.Course.Language.IsActive)
            .OrderBy(unit => unit.SortOrder).ThenBy(unit => unit.Id);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync(ct);
        return new PagedResponse<Unit>(items, pagination.Page, pagination.PageSize, total);
    }

    public Task<Unit?> GetUnitAsync(Guid id, CancellationToken ct) =>
        _context.Units.AsNoTracking().FirstOrDefaultAsync(unit => unit.Id == id &&
            unit.Course.Status == ContentStatus.Published && unit.Course.Language.IsActive, ct);

    public async Task<PagedResponse<Lesson>> ListLessonsAsync(Guid unitId, PaginationParams pagination, CancellationToken ct)
    {
        var query = _context.Lessons.AsNoTracking().Where(lesson => lesson.UnitId == unitId &&
            lesson.Status == ContentStatus.Published &&
            lesson.Unit.Course.Status == ContentStatus.Published && lesson.Unit.Course.Language.IsActive)
            .OrderBy(lesson => lesson.SortOrder).ThenBy(lesson => lesson.Id);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize).ToListAsync(ct);
        return new PagedResponse<Lesson>(items, pagination.Page, pagination.PageSize, total);
    }

    public Task<Lesson?> GetLessonAsync(Guid id, CancellationToken ct) =>
        _context.Lessons.AsNoTracking().FirstOrDefaultAsync(lesson => lesson.Id == id &&
            lesson.Status == ContentStatus.Published &&
            lesson.Unit.Course.Status == ContentStatus.Published && lesson.Unit.Course.Language.IsActive, ct);
}

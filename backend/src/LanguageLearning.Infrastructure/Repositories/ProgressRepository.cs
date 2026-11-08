using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class ProgressRepository(AppDbContext db) : IProgressRepository
{
    private IQueryable<Lesson> PublishedLessons => db.Lessons.Where(x =>
        x.Status == ContentStatus.Published && x.Unit.Course.Status == ContentStatus.Published &&
        x.Unit.Course.Language.IsActive);

    public Task<Lesson?> GetPublishedLessonAsync(Guid id, CancellationToken ct) =>
        PublishedLessons.Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Course?> GetPublishedCourseAsync(Guid id, CancellationToken ct) =>
        db.Courses.FirstOrDefaultAsync(x => x.Id == id && x.Status == ContentStatus.Published && x.Language.IsActive, ct);

    public Task<List<CourseEnrollment>> GetEnrollmentsAsync(Guid userId, CancellationToken ct) =>
        db.CourseEnrollments.AsNoTracking().Include(x => x.Course).ThenInclude(x => x.Language)
            .Include(x => x.Course).ThenInclude(x => x.Units).ThenInclude(x => x.Lessons)
            .Where(x => x.UserId == userId && x.Course.Status == ContentStatus.Published && x.Course.Language.IsActive)
            .OrderBy(x => x.EnrolledAt).ToListAsync(ct);

    public Task<List<LessonProgress>> GetProgressAsync(Guid userId, CancellationToken ct) =>
        db.LessonProgresses.AsNoTracking().Include(x => x.Lesson).ThenInclude(x => x.Unit)
            .Where(x => x.UserId == userId).ToListAsync(ct);

    public async Task EnrollAsync(Guid userId, Guid courseId, CancellationToken ct)
    {
        if (!await db.CourseEnrollments.AnyAsync(x => x.UserId == userId && x.CourseId == courseId, ct))
            db.CourseEnrollments.Add(new CourseEnrollment { UserId = userId, CourseId = courseId });
    }

    public async Task SaveProgressAsync(Guid userId, Guid lessonId, string status, CancellationToken ct)
    {
        var progress = await db.LessonProgresses.FirstOrDefaultAsync(x => x.UserId == userId && x.LessonId == lessonId, ct);
        if (progress == null)
        {
            progress = new LessonProgress { UserId = userId, LessonId = lessonId };
            db.LessonProgresses.Add(progress);
        }
        // Completion is monotonic so a delayed retry cannot undo learning history.
        if (progress.Status != "Completed") progress.Status = status;
        if (progress.Status == "Completed") progress.CompletedAt ??= DateTime.UtcNow;
        progress.LastAccessedAt = DateTime.UtcNow;
    }

    public async Task SaveChangesAsync(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException("Learning progress was changed by another request. Retry to load its latest state.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is MySqlConnector.MySqlException { Number: 1062 })
        {
            throw new ConflictException("A concurrent request already saved this operation. Retry with the same identifier.");
        }
    }
}

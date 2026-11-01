using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Common;
using LanguageLearning.Domain.Common;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public class AdminContentRepository : IAdminContentRepository
{
    private readonly AppDbContext _context;

    public AdminContentRepository(AppDbContext context) => _context = context;

    public async Task<PagedResponse<T>> ListAsync<T>(PaginationParams pagination, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        var query = _context.Set<T>().AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(entity => entity.CreatedAt).ThenBy(entity => entity.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize).Take(pagination.PageSize)
            .ToListAsync(cancellationToken);
        return new PagedResponse<T>(items, pagination.Page, pagination.PageSize, total);
    }

    public Task<T?> FindAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity =>
        _context.Set<T>().FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

    public Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity =>
        _context.Set<T>().AddAsync(entity, cancellationToken).AsTask();

    public void Remove<T>(T entity) where T : BaseEntity => _context.Set<T>().Remove(entity);

    public async Task<bool> HasDependentsAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity
    {
        if (typeof(T) == typeof(Language))
            return await _context.Courses.AnyAsync(value => value.LanguageId == id, cancellationToken) ||
                await _context.Decks.AnyAsync(value => value.LanguageId == id, cancellationToken) ||
                await _context.Vocabularies.AnyAsync(value => value.LanguageId == id, cancellationToken);
        if (typeof(T) == typeof(Course))
            return await _context.Units.AnyAsync(value => value.CourseId == id, cancellationToken) ||
                await _context.CourseEnrollments.AnyAsync(value => value.CourseId == id, cancellationToken);
        if (typeof(T) == typeof(Unit))
            return await _context.Lessons.AnyAsync(value => value.UnitId == id, cancellationToken);
        if (typeof(T) == typeof(Lesson))
            return await _context.Vocabularies.AnyAsync(value => value.LessonId == id, cancellationToken) ||
                await _context.Questions.AnyAsync(value => value.LessonId == id, cancellationToken) ||
                await _context.LessonProgresses.AnyAsync(value => value.LessonId == id, cancellationToken) ||
                await _context.QuizAttempts.AnyAsync(value => value.LessonId == id, cancellationToken);
        return false;
    }

    public async Task SaveAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            (exception.InnerException?.Message ?? exception.Message).Contains("foreign key", StringComparison.OrdinalIgnoreCase))
        {
            throw new ConflictException("Content is referenced by other records.");
        }
    }
}

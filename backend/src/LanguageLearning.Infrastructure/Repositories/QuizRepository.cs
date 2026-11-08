using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class QuizRepository(AppDbContext db) : IQuizRepository
{
    private IQueryable<Lesson> PublishedLessons => db.Lessons.Where(x =>
        x.Status == ContentStatus.Published && x.Unit.Course.Status == ContentStatus.Published &&
        x.Unit.Course.Language.IsActive);

    public Task<Lesson?> GetPublishedLessonAsync(Guid id, CancellationToken ct) =>
        PublishedLessons.Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<Question>> GetQuestionsAsync(Guid lessonId, CancellationToken ct) =>
        db.Questions.AsNoTracking().Include(x => x.Options).Where(x => x.LessonId == lessonId)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToListAsync(ct);

    public Task<QuizAttempt?> GetAttemptAsync(Guid id, CancellationToken ct) =>
        db.QuizAttempts.Include(x => x.Answers).ThenInclude(x => x.Question).ThenInclude(x => x.Options)
            .FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<QuizAttempt>> GetAttemptsAsync(Guid userId, Guid lessonId, CancellationToken ct) =>
        db.QuizAttempts.AsNoTracking().Include(x => x.Answers).ThenInclude(x => x.Question).ThenInclude(x => x.Options)
            .Where(x => x.UserId == userId && x.LessonId == lessonId)
            .OrderByDescending(x => x.CompletedAt).Take(50).ToListAsync(ct);

    public async Task AddAttemptAsync(QuizAttempt attempt, CancellationToken ct)
    {
        foreach (var answer in attempt.Answers) db.Attach(answer.Question);
        await db.QuizAttempts.AddAsync(attempt, ct);
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

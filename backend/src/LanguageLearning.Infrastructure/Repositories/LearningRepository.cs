using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class LearningRepository(AppDbContext db) : ILearningRepository
{
    private IQueryable<Lesson> PublishedLessons => db.Lessons.Where(x =>
        x.Status == ContentStatus.Published && x.Unit.Course.Status == ContentStatus.Published &&
        x.Unit.Course.Language.IsActive);

    public Task<Lesson?> GetPublishedLessonAsync(Guid id, CancellationToken ct) =>
        PublishedLessons.Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<Course?> GetPublishedCourseAsync(Guid id, CancellationToken ct) =>
        db.Courses.FirstOrDefaultAsync(x => x.Id == id && x.Status == ContentStatus.Published && x.Language.IsActive, ct);

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

    public async Task<User> GetUserAsync(Guid id, CancellationToken ct) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException("User not found.");

    public Task<List<ReviewEvent>> GetReviewsAsync(Guid userId, CancellationToken ct) =>
        db.ReviewEvents.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(ct);

    public Task<List<CardSchedule>> GetSchedulesAsync(Guid userId, CancellationToken ct) =>
        db.CardSchedules.AsNoTracking().Where(x => x.UserId == userId && !x.Card.Deck.IsDeleted && x.Card.Deck.OwnerId == userId).ToListAsync(ct);

    public Task<int> CountCardsAsync(Guid userId, CancellationToken ct) =>
        db.Cards.CountAsync(x => x.Deck.OwnerId == userId && !x.Deck.IsDeleted, ct);

    public Task<UserGoal?> GetGoalAsync(Guid userId, CancellationToken ct) =>
        db.UserGoals.FirstOrDefaultAsync(x => x.UserId == userId && x.GoalType == "CardsReview", ct);

    public async Task SetGoalAsync(Guid userId, int target, CancellationToken ct)
    {
        var goal = await GetGoalAsync(userId, ct);
        if (goal == null) db.UserGoals.Add(new UserGoal { UserId = userId, DailyTarget = target });
        else goal.DailyTarget = target;
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

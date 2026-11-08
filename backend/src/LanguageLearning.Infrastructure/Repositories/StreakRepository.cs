using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class StreakRepository(AppDbContext db) : IStreakRepository
{
    public async Task<User> GetUserAsync(Guid id, CancellationToken ct) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException("User not found.");

    public Task<List<ReviewEvent>> GetReviewsAsync(Guid userId, CancellationToken ct) =>
        db.ReviewEvents.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(ct);

    public Task<List<LessonProgress>> GetProgressAsync(Guid userId, CancellationToken ct) =>
        db.LessonProgresses.AsNoTracking().Include(x => x.Lesson).ThenInclude(x => x.Unit)
            .Where(x => x.UserId == userId).ToListAsync(ct);

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

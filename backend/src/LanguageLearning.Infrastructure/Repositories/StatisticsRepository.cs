using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class StatisticsRepository(AppDbContext db) : IStatisticsRepository
{
    public async Task<User> GetUserAsync(Guid id, CancellationToken ct) =>
        await db.Users.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct)
        ?? throw new NotFoundException("User not found.");

    public Task<List<ReviewEvent>> GetReviewsAsync(Guid userId, CancellationToken ct) =>
        db.ReviewEvents.AsNoTracking().Where(x => x.UserId == userId).ToListAsync(ct);

    public Task<List<CardSchedule>> GetSchedulesAsync(Guid userId, CancellationToken ct) =>
        db.CardSchedules.AsNoTracking().Where(x => x.UserId == userId && !x.Card.Deck.IsDeleted && x.Card.Deck.OwnerId == userId).ToListAsync(ct);

    public Task<int> CountCardsAsync(Guid userId, CancellationToken ct) =>
        db.Cards.CountAsync(x => x.Deck.OwnerId == userId && !x.Deck.IsDeleted, ct);

    public Task<List<LessonProgress>> GetProgressAsync(Guid userId, CancellationToken ct) =>
        db.LessonProgresses.AsNoTracking().Include(x => x.Lesson).ThenInclude(x => x.Unit)
            .Where(x => x.UserId == userId).ToListAsync(ct);

    public Task<UserGoal?> GetGoalAsync(Guid userId, CancellationToken ct) =>
        db.UserGoals.FirstOrDefaultAsync(x => x.UserId == userId && x.GoalType == "CardsReview", ct);
}

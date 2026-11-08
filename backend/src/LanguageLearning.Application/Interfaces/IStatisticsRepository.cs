using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface IStatisticsRepository
{
    Task<User> GetUserAsync(Guid id, CancellationToken ct);
    Task<List<ReviewEvent>> GetReviewsAsync(Guid userId, CancellationToken ct);
    Task<List<CardSchedule>> GetSchedulesAsync(Guid userId, CancellationToken ct);
    Task<int> CountCardsAsync(Guid userId, CancellationToken ct);
    Task<List<LessonProgress>> GetProgressAsync(Guid userId, CancellationToken ct);
    Task<UserGoal?> GetGoalAsync(Guid userId, CancellationToken ct);
}

using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface IStreakRepository
{
    Task<User> GetUserAsync(Guid id, CancellationToken ct);
    Task<List<ReviewEvent>> GetReviewsAsync(Guid userId, CancellationToken ct);
    Task<List<LessonProgress>> GetProgressAsync(Guid userId, CancellationToken ct);
    Task<UserGoal?> GetGoalAsync(Guid userId, CancellationToken ct);
    Task SetGoalAsync(Guid userId, int target, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface ILearningRepository
{
    Task<Lesson?> GetPublishedLessonAsync(Guid id, CancellationToken ct);
    Task<Course?> GetPublishedCourseAsync(Guid id, CancellationToken ct);
    Task<List<Question>> GetQuestionsAsync(Guid lessonId, CancellationToken ct);
    Task<QuizAttempt?> GetAttemptAsync(Guid id, CancellationToken ct);
    Task<List<QuizAttempt>> GetAttemptsAsync(Guid userId, Guid lessonId, CancellationToken ct);
    Task AddAttemptAsync(QuizAttempt attempt, CancellationToken ct);
    Task<List<CourseEnrollment>> GetEnrollmentsAsync(Guid userId, CancellationToken ct);
    Task<List<LessonProgress>> GetProgressAsync(Guid userId, CancellationToken ct);
    Task EnrollAsync(Guid userId, Guid courseId, CancellationToken ct);
    Task SaveProgressAsync(Guid userId, Guid lessonId, string status, CancellationToken ct);
    Task<User> GetUserAsync(Guid id, CancellationToken ct);
    Task<List<ReviewEvent>> GetReviewsAsync(Guid userId, CancellationToken ct);
    Task<List<CardSchedule>> GetSchedulesAsync(Guid userId, CancellationToken ct);
    Task<int> CountCardsAsync(Guid userId, CancellationToken ct);
    Task<UserGoal?> GetGoalAsync(Guid userId, CancellationToken ct);
    Task SetGoalAsync(Guid userId, int target, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

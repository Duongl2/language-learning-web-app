using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface IQuizRepository
{
    Task<Lesson?> GetPublishedLessonAsync(Guid id, CancellationToken ct);
    Task<List<Question>> GetQuestionsAsync(Guid lessonId, CancellationToken ct);
    Task<QuizAttempt?> GetAttemptAsync(Guid id, CancellationToken ct);
    Task<List<QuizAttempt>> GetAttemptsAsync(Guid userId, Guid lessonId, CancellationToken ct);
    Task AddAttemptAsync(QuizAttempt attempt, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}

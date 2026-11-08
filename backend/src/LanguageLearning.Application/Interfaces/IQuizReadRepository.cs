using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface IQuizReadRepository
{
    Task<Lesson?> GetPublishedLessonAsync(Guid id, CancellationToken ct);
    Task<List<Question>> GetQuestionsAsync(Guid lessonId, CancellationToken ct);
}

using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class QuizReadRepository(AppDbContext db) : IQuizReadRepository
{
    private IQueryable<Lesson> PublishedLessons => db.Lessons.Where(x =>
        x.Status == ContentStatus.Published && x.Unit.Course.Status == ContentStatus.Published &&
        x.Unit.Course.Language.IsActive);

    public Task<Lesson?> GetPublishedLessonAsync(Guid id, CancellationToken ct) =>
        PublishedLessons.Include(x => x.Unit).FirstOrDefaultAsync(x => x.Id == id, ct);

    public Task<List<Question>> GetQuestionsAsync(Guid lessonId, CancellationToken ct) =>
        db.Questions.AsNoTracking().Include(x => x.Options).Where(x => x.LessonId == lessonId)
            .OrderBy(x => x.CreatedAt).ThenBy(x => x.Id).ToListAsync(ct);
}

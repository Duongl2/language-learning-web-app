using LanguageLearning.Domain.Common;
using LanguageLearning.Application.Common;
using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface IAdminContentRepository
{
    Task<PagedResponse<T>> ListAsync<T>(PaginationParams pagination, CancellationToken cancellationToken = default) where T : BaseEntity;
    Task<PagedResponse<Vocabulary>> ListVocabulariesAsync(Guid lessonId, string? keyword, PaginationParams pagination, CancellationToken cancellationToken = default);
    Task<T?> FindAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity;
    Task AddAsync<T>(T entity, CancellationToken cancellationToken = default) where T : BaseEntity;
    void Remove<T>(T entity) where T : BaseEntity;
    Task<bool> HasDependentsAsync<T>(Guid id, CancellationToken cancellationToken = default) where T : BaseEntity;
    Task<bool> UnitHasLearningHistoryAsync(Guid id, CancellationToken cancellationToken = default);
    Task SaveAsync(CancellationToken cancellationToken = default);
}


using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface ISyncRepository
{
    Task<SyncOperation?> GetByClientOperationIdAsync(
        Guid userId,
        Guid clientOperationId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SyncOperation operation,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default);
}

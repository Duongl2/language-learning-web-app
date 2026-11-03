
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class SyncRepository : ISyncRepository
{
    private readonly AppDbContext _context;

    public SyncRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SyncOperation?> GetByClientOperationIdAsync(
        Guid userId,
        Guid clientOperationId,
        CancellationToken cancellationToken = default)
    {
        return await _context.SyncOperations
            .AsNoTracking()
            .FirstOrDefaultAsync(
                operation =>
                    operation.UserId == userId &&
                    operation.ClientOperationId == clientOperationId,
                cancellationToken);
    }

    public async Task AddAsync(
        SyncOperation operation,
        CancellationToken cancellationToken = default)
    {
        await _context.SyncOperations.AddAsync(
            operation,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is
                MySqlConnector.MySqlException mysqlException &&
            mysqlException.Number == 1062 &&
            mysqlException.Message.Contains(
                "ClientOperationId",
                StringComparison.OrdinalIgnoreCase))
        {
            _context.ChangeTracker.Clear();
            throw new DuplicateSyncOperationException();
        }
    }

    public async Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            try
            {
                await using var transaction =
                    await _context.Database.BeginTransactionAsync(
                        cancellationToken);

                var result = await action(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                _context.ChangeTracker.Clear();
                throw;
            }
        });
    }
}

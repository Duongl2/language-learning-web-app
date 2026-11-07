
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class SyncPullRepository : ISyncPullRepository
{
    private readonly AppDbContext _context;

    public SyncPullRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Deck>> GetChangedDecksAsync(
        Guid userId,
        DateTime afterChangedAtUtc,
        Guid afterId,
        int take,
        CancellationToken cancellationToken = default)
    {
        if (take < 1 || take > 101)
        {
            throw new ArgumentOutOfRangeException(nameof(take));
        }

        var lastId = afterId.ToString("D");
        var results = new List<Deck>(take);

        var query = _context.Decks
            .AsNoTracking()
            .Where(deck =>
                deck.OwnerId == userId &&
                (deck.UpdatedAt ?? deck.CreatedAt) >= afterChangedAtUtc)
            .Include(deck => deck.Tags)
            .OrderBy(deck => deck.UpdatedAt ?? deck.CreatedAt)
            .ThenBy(deck => deck.Id);

        await foreach (var deck in query
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            var changedAt = deck.UpdatedAt ?? deck.CreatedAt;

            if (changedAt == afterChangedAtUtc &&
                string.CompareOrdinal(
                    deck.Id.ToString("D"), lastId) <= 0)
            {
                continue;
            }

            results.Add(deck);

            if (results.Count == take)
            {
                break;
            }
        }

        return results;
    }
}

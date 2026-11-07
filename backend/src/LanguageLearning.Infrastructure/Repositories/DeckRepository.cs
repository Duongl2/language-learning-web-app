using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public class DeckRepository : IDeckRepository
{
    private readonly AppDbContext _context;

    public DeckRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Deck> Items, int TotalItems)> GetAccessibleDecksAsync(
        Guid userId,
        int page,
        int pageSize,
        bool onlyMine = false,
        CancellationToken cancellationToken = default,
        string? search = null)
    {
        var query = _context.Decks
            .AsNoTracking()
            .Where(d => !d.IsDeleted);

        if (onlyMine)
        {
            query = query.Where(d => d.OwnerId == userId);
        }
        else
        {
            query = query.Where(d =>
                d.OwnerId == userId || d.IsPublic);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var keyword = search.Trim();

            query = query.Where(d =>
                d.Title.Contains(keyword) ||
                d.Tags.Any(t => t.Tag.Contains(keyword)));
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(d => d.CreatedAt)
            .ThenBy(d => d.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(d => d.Tags)
            .ToListAsync(cancellationToken);

        return (items, totalItems);
    }

    public async Task<List<Deck>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Decks
            .AsNoTracking()
            .Where(d => !d.IsDeleted)
            .Include(d => d.Tags)
            .Include(d => d.Cards)
                .ThenInclude(c => c.Schedules)
            .ToListAsync(cancellationToken);
    }

    public async Task<Deck?> GetByIdAsync(
        Guid deckId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Decks
            .Include(d => d.Tags)
            .Include(d => d.Cards)
                .ThenInclude(c => c.Schedules)
            .FirstOrDefaultAsync(
                d => d.Id == deckId && !d.IsDeleted,
                cancellationToken);
    }

    public async Task<bool> LanguageExistsAsync(
        Guid languageId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Languages
            .AnyAsync(
                l => l.Id == languageId,
                cancellationToken);
    }

    public async Task AddAsync(
        Deck deck,
        CancellationToken cancellationToken = default)
    {
        await _context.Decks.AddAsync(
            deck,
            cancellationToken);
    }

    public void Update(Deck deck)
    {
        _context.Decks.Update(deck);
    }

    public void Delete(Deck deck)
    {
        deck.IsDeleted = true;
        deck.DeletedAt = DateTime.UtcNow;
        deck.Version++;

        var entry = _context.Entry(deck);

        entry.Property(d => d.IsDeleted).IsModified = true;
        entry.Property(d => d.DeletedAt).IsModified = true;
        entry.Property(d => d.Version).IsModified = true;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConflictException(
                "Deck was modified by another device. Refresh and retry.");
        }
    }

    public async Task<Dictionary<Guid, (int TotalCards, int NewCards, int DueCards)>>
        GetDeckStatisticsBatchAsync(
            IReadOnlyCollection<Guid> deckIds,
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        if (deckIds.Count == 0)
        {
            return new Dictionary<Guid, (int, int, int)>();
        }

        var now = DateTime.UtcNow;
        var ids = deckIds.Distinct().ToArray();

        var statistics = await _context.Cards
            .AsNoTracking()
            .Where(card => ids.Contains(card.DeckId))
            .GroupBy(card => card.DeckId)
            .Select(group => new
            {
                DeckId = group.Key,
                TotalCards = group.Count(),
                NewCards = group.Count(card =>
                    !card.Schedules.Any(schedule =>
                        schedule.UserId == userId)),
                DueCards = group.Count(card =>
                    card.Schedules.Any(schedule =>
                        schedule.UserId == userId &&
                        schedule.DueAt <= now))
            })
            .ToListAsync(cancellationToken);

        return statistics.ToDictionary(
            item => item.DeckId,
            item => (
                item.TotalCards,
                item.NewCards,
                item.DueCards
            ));
    }

    public async Task<(int TotalCards, int NewCards, int DueCards)>
        GetDeckStatisticsAsync(
            Guid deckId,
            Guid userId,
            CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        var totalCards = await _context.Cards
            .CountAsync(
                card => card.DeckId == deckId,
                cancellationToken);

        var newCards = await _context.Cards
            .CountAsync(
                card => card.DeckId == deckId &&
                    !card.Schedules.Any(s => s.UserId == userId),
                cancellationToken);

        var dueCards = await _context.Cards
            .CountAsync(
                card => card.DeckId == deckId &&
                    card.Schedules.Any(s =>
                        s.UserId == userId && s.DueAt <= now),
                cancellationToken);

        return (totalCards, newCards, dueCards);
    }
}

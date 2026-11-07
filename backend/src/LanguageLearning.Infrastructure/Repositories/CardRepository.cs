
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public class CardRepository : ICardRepository
{
    private readonly AppDbContext _context;

    public CardRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<Card>> GetByDeckIdAsync(
        Guid deckId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Cards
            .AsNoTracking()
            .Where(card => card.DeckId == deckId)
            .OrderBy(card => card.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Card?> GetByIdAsync(
        Guid deckId,
        Guid cardId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Cards
            .FirstOrDefaultAsync(
                card => card.DeckId == deckId &&
                        card.Id == cardId,
                cancellationToken);
    }

    public async Task AddAsync(
        Card card,
        CancellationToken cancellationToken = default)
    {
        await _context.Cards.AddAsync(
            card,
            cancellationToken);
    }

    public void Update(Card card)
    {
        _context.Cards.Update(card);
    }

    public async Task<bool> HasReviewEventsAsync(
        Guid cardId,
        CancellationToken cancellationToken = default)
    {
        return await _context.ReviewEvents
            .AnyAsync(
                reviewEvent => reviewEvent.CardId == cardId,
                cancellationToken);
    }

    public void Delete(Card card)
    {
        _context.Cards.Remove(card);
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
                "Card was modified by another device. Refresh and retry.");
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is MySqlConnector.MySqlException mysqlException &&
            mysqlException.Number == 1451 &&
            mysqlException.Message.Contains(
                "FK_review_events_cards_CardId",
                StringComparison.Ordinal))
        {
            throw new ConflictException(
                "Không thể xóa Flashcard đã có lịch sử ôn tập.");
        }
    }
}

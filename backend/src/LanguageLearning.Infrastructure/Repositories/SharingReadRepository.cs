using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class SharingReadRepository(AppDbContext db) : ISharingReadRepository
{
    private IQueryable<Deck> PublicDecks => db.Decks.Where(x => x.IsPublic && !x.IsDeleted && x.Language.IsActive);

    public async Task<(List<PublicDeckDto> Items, int Total)> ListAsync(string? search, Guid? languageId, int page, int pageSize, CancellationToken ct)
    {
        var query = PublicDecks.AsNoTracking();
        if (languageId.HasValue) query = query.Where(x => x.LanguageId == languageId);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(x => x.Title.Contains(term) || x.Tags.Any(t => t.Tag.Contains(term)));
        }
        var count = await query.CountAsync(ct);
        var items = await query.OrderByDescending(x => x.CreatedAt).ThenBy(x => x.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(x => new PublicDeckDto(x.Id, x.LanguageId, x.Title, x.Description, x.Cards.Count, x.Tags.Select(t => t.Tag).ToList()))
            .ToListAsync(ct);
        return (items, count);
    }

    public async Task<PublicDeckPreview?> PreviewAsync(Guid id, CancellationToken ct)
    {
        var deck = await PublicDecks.AsNoTracking().Where(x => x.Id == id).Select(x =>
            new PublicDeckDto(x.Id, x.LanguageId, x.Title, x.Description, x.Cards.Count, x.Tags.Select(t => t.Tag).ToList())).FirstOrDefaultAsync(ct);
        if (deck == null) return null;
        var cards = await db.Cards.AsNoTracking().Where(x => x.DeckId == id).OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .Take(5).Select(x => new CardPreview(x.Front, x.Back)).ToListAsync(ct);
        return new PublicDeckPreview(deck, cards);
    }
}

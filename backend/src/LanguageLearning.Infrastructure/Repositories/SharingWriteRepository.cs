using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Infrastructure.Repositories;

public sealed class SharingWriteRepository(AppDbContext db) : ISharingWriteRepository
{
    private IQueryable<Deck> PublicDecks => db.Decks.Where(x => x.IsPublic && !x.IsDeleted && x.Language.IsActive);

    public async Task<Guid> CloneAsync(Guid id, Guid ownerId, CancellationToken ct)
    {
        var source = await PublicDecks.AsNoTracking().Include(x => x.Tags).Include(x => x.Cards).FirstOrDefaultAsync(x => x.Id == id, ct)
            ?? throw new NotFoundException("Public deck not found.");
        var clone = new Deck
        {
            OwnerId = ownerId,
            LanguageId = source.LanguageId,
            Title = source.Title,
            Description = source.Description,
            SourceDeckId = source.Id,
            IsPublic = false
        };
        foreach (var card in source.Cards) clone.Cards.Add(new Card
        {
            Front = card.Front,
            Back = card.Back,
            AudioUrl = card.AudioUrl,
            ImageUrl = card.ImageUrl
        });
        foreach (var tag in source.Tags) clone.Tags.Add(new DeckTag { Tag = tag.Tag });
        db.Decks.Add(clone);
        await db.SaveChangesAsync(ct);
        return clone.Id;
    }

    public async Task HideAsync(Guid id, Guid actorId, CancellationToken ct)
    {
        var deck = await db.Decks.FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct) ?? throw new NotFoundException("Deck not found.");
        deck.IsPublic = false;
        deck.Version++;
        db.AuditLogs.Add(new AuditLog { ActorId = actorId, Action = "HidePublicDeck", TargetType = "Deck", TargetId = id });
        await db.SaveChangesAsync(ct);
    }
}

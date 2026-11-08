using System.Text;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Services;

public sealed class CsvService(IDeckRepository decks, ICardRepository cards, ICurrentUserService currentUser)
{
    public async Task<int> ImportAsync(Guid deckId, string content, CancellationToken ct)
    {
        var deck = await OwnedDeckAsync(deckId, ct);
        var preview = CsvPreviewService.Preview(content);
        if (preview.InvalidCount > 0 || preview.ValidCount == 0) throw new ArgumentException("Fix all CSV errors before importing.");
        foreach (var row in preview.Rows)
            await cards.AddAsync(new Card { DeckId = deck.Id, Front = row.Front, Back = row.Back }, ct);
        deck.Version++;
        // EF commits the complete batch and deck version in one SaveChanges transaction.
        await decks.SaveChangesAsync(ct);
        return preview.ValidCount;
    }

    public async Task<string> ExportAsync(Guid deckId, CancellationToken ct)
    {
        var deck = await OwnedDeckAsync(deckId, ct);
        return "front,back\r\n" + string.Join("\r\n", deck.Cards.OrderBy(x => x.CreatedAt).ThenBy(x => x.Id)
            .Select(x => $"{Escape(x.Front)},{Escape(x.Back)}")) + "\r\n";
    }

    public static string Escape(string value)
    {
        var trimmed = value.TrimStart();
        if ((trimmed.Length > 0 && "=+-@".Contains(trimmed[0])) || value.StartsWith('\t') || value.StartsWith('\r') || value.StartsWith('\n'))
            value = "'" + value;
        return "\"" + value.Replace("\"", "\"\"") + "\"";
    }

    private async Task<Deck> OwnedDeckAsync(Guid id, CancellationToken ct)
    {
        var deck = await decks.GetByIdAsync(id, ct);
        if (deck == null || deck.OwnerId != currentUser.GetRequiredUserId()) throw new NotFoundException("Owned deck not found.");
        return deck;
    }
}

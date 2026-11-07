
using System.Globalization;
using System.Text;
using LanguageLearning.Application.DTOs.Sync;
using LanguageLearning.Application.Interfaces;

namespace LanguageLearning.Application.Services;

public sealed class SyncPullService : ISyncPullService
{
    private readonly ISyncPullRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public SyncPullService(
        ISyncPullRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<SyncPullResponse> PullAsync(
        string? cursor,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        if (limit < 1 || limit > 100)
        {
            throw new ArgumentException(
                "Limit must be between 1 and 100.");
        }

        var userId = _currentUserService.GetRequiredUserId();

        var (afterChangedAt, afterId) = DecodeCursor(cursor);

        var decks = await _repository.GetChangedDecksAsync(
            userId,
            afterChangedAt,
            afterId,
            limit + 1,
            cancellationToken);

        var hasMore = decks.Count > limit;
        var page = decks.Take(limit).ToList();

        var response = new SyncPullResponse
        {
            HasMore = hasMore,
            NextCursor = cursor
        };

        foreach (var deck in page)
        {
            var changedAt = DateTime.SpecifyKind(
                deck.UpdatedAt ?? deck.CreatedAt,
                DateTimeKind.Utc);

            response.Changes.Add(new SyncChangeResponse
            {
                EntityType = "deck",
                EntityId = deck.Id,
                Version = deck.Version,
                Action = deck.IsDeleted ? "delete" : "upsert",
                ChangedAt = changedAt,
                Deck = deck.IsDeleted
                    ? null
                    : new SyncDeckSnapshot
                    {
                        Id = deck.Id,
                        LanguageId = deck.LanguageId,
                        Title = deck.Title,
                        Description = deck.Description,
                        IsPublic = deck.IsPublic,
                        Version = deck.Version,
                        Tags = deck.Tags
                            .Select(tag => tag.Tag)
                            .OrderBy(tag => tag)
                            .ToList()
                    }
            });
        }

        if (page.Count > 0)
        {
            var last = page[^1];

            var lastChangedAt = DateTime.SpecifyKind(
                last.UpdatedAt ?? last.CreatedAt,
                DateTimeKind.Utc);

            response.NextCursor = EncodeCursor(
                lastChangedAt,
                last.Id);
        }

        return response;
    }

    private static string EncodeCursor(
        DateTime changedAt,
        Guid entityId)
    {
        var value =
            changedAt.ToString("O", CultureInfo.InvariantCulture)
            + "|"
            + entityId.ToString("D");

        return Convert.ToBase64String(
                Encoding.UTF8.GetBytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private static (DateTime ChangedAt, Guid EntityId)
        DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return (
                new DateTime(
                    1000, 1, 1, 0, 0, 0,
                    DateTimeKind.Utc),
                Guid.Empty);
        }

        if (cursor.Length > 512)
        {
            throw new ArgumentException("Invalid cursor.");
        }

        try
        {
            var base64 = cursor
                .Replace('-', '+')
                .Replace('_', '/');

            base64 = base64.PadRight(
                ((base64.Length + 3) / 4) * 4,
                '=');

            var value = Encoding.UTF8.GetString(
                Convert.FromBase64String(base64));

            var parts = value.Split('|');

            if (parts.Length != 2 ||
                !DateTime.TryParseExact(
                    parts[0],
                    "O",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.RoundtripKind,
                    out var changedAt) ||
                changedAt.Kind != DateTimeKind.Utc ||
                changedAt.Year < 1000 ||
                !Guid.TryParse(parts[1], out var entityId))
            {
                throw new ArgumentException("Invalid cursor.");
            }

            return (changedAt, entityId);
        }
        catch (FormatException)
        {
            throw new ArgumentException("Invalid cursor.");
        }
    }
}

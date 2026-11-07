
using LanguageLearning.Application.Common;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Decks;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Services;

public class DeckService : IDeckService
{
    private readonly IDeckRepository _deckRepository;
    private readonly ICurrentUserService _currentUserService;

    public DeckService(
        IDeckRepository deckRepository,
        ICurrentUserService currentUserService)
    {
        _deckRepository = deckRepository;
        _currentUserService = currentUserService;
    }

    public async Task<DeckResponse> CreateDeckAsync(
        CreateDeckRequest request,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();

        if (request.LanguageId == Guid.Empty ||
            !await _deckRepository.LanguageExistsAsync(
                request.LanguageId, cancellationToken))
        {
            throw new ArgumentException("Invalid language.");
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException(
                "Deck title is required.");
        }

        var deck = new Deck
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            LanguageId = request.LanguageId,
            Title = request.Title.Trim(),
            Description = request.Description?.Trim() ?? string.Empty,
            IsPublic = request.IsPublic,
            Version = 1
        };

        foreach (var tag in request.Tags ?? new List<string>())
        {
            var normalizedTag = tag?.Trim();

            if (string.IsNullOrWhiteSpace(normalizedTag))
            {
                continue;
            }

            if (deck.Tags.Any(t => t.Tag == normalizedTag))
            {
                continue;
            }

            deck.Tags.Add(new DeckTag
            {
                Id = Guid.NewGuid(),
                Tag = normalizedTag
            });
        }

        await _deckRepository.AddAsync(
            deck, cancellationToken);

        await _deckRepository.SaveChangesAsync(
            cancellationToken);

        return new DeckResponse
        {
            Id = deck.Id,
            OwnerId = deck.OwnerId,
            LanguageId = deck.LanguageId,
            Title = deck.Title,
            Description = deck.Description,
            IsPublic = deck.IsPublic,
            Version = deck.Version,
            Tags = deck.Tags.Select(t => t.Tag).ToList(),
            TotalCards = 0,
            NewCards = 0,
            DueCards = 0,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt
        };
    }

    public async Task<PagedResponse<DeckResponse>> GetDecksAsync(
        PaginationParams pagination,
        bool onlyMine = false,
        CancellationToken cancellationToken = default,
        string? search = null)
    {
        var userId = _currentUserService.GetRequiredUserId();

        ArgumentNullException.ThrowIfNull(pagination);

        var (decks, totalItems) =
            await _deckRepository.GetAccessibleDecksAsync(
                userId,
                pagination.Page,
                pagination.PageSize,
                onlyMine,
                cancellationToken,
                search);

        var deckIds = decks
            .Select(deck => deck.Id)
            .ToArray();

        var statistics =
            await _deckRepository.GetDeckStatisticsBatchAsync(
                deckIds,
                userId,
                cancellationToken);

        var items = new List<DeckResponse>();

        foreach (var deck in decks)
        {
            var stats = statistics.TryGetValue(
                deck.Id, out var deckStats)
                ? deckStats
                : (TotalCards: 0, NewCards: 0, DueCards: 0);

            items.Add(new DeckResponse
            {
                Id = deck.Id,
                OwnerId = deck.OwnerId,
                LanguageId = deck.LanguageId,
                Title = deck.Title,
                Description = deck.Description,
                IsPublic = deck.IsPublic,
                Version = deck.Version,
                Tags = deck.Tags
                    .Select(tag => tag.Tag)
                    .ToList(),
                TotalCards = stats.TotalCards,
                NewCards = stats.NewCards,
                DueCards = stats.DueCards,
                CreatedAt = deck.CreatedAt,
                UpdatedAt = deck.UpdatedAt
            });
        }

        return new PagedResponse<DeckResponse>(
            items,
            pagination.Page,
            pagination.PageSize,
            totalItems);
    }

    public async Task<DeckResponse> GetDeckByIdAsync(
        Guid deckId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var deck = await _deckRepository.GetByIdAsync(
            deckId, cancellationToken);

        if (deck == null ||
            (deck.OwnerId != userId && !deck.IsPublic))
        {
            throw new NotFoundException(
                "The requested deck does not exist or is not accessible.");
        }

        var stats = await _deckRepository.GetDeckStatisticsAsync(
            deckId,
            userId,
            cancellationToken);

        return new DeckResponse
        {
            Id = deck.Id,
            OwnerId = deck.OwnerId,
            LanguageId = deck.LanguageId,
            Title = deck.Title,
            Description = deck.Description,
            IsPublic = deck.IsPublic,
            Version = deck.Version,
            Tags = deck.Tags
                .Select(tag => tag.Tag)
                .ToList(),
            TotalCards = stats.TotalCards,
            NewCards = stats.NewCards,
            DueCards = stats.DueCards,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt
        };
    }

    public async Task<DeckResponse> UpdateDeckAsync(
        Guid deckId,
        UpdateDeckRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var userId = _currentUserService.GetRequiredUserId();

        var deck = await _deckRepository.GetByIdAsync(
            deckId, cancellationToken);

        if (deck == null || deck.OwnerId != userId)
        {
            throw new NotFoundException(
                "The requested deck does not exist or is not accessible.");
        }

        if (!request.BaseVersion.HasValue)
        {
            throw new ArgumentException(
                "BaseVersion is required.");
        }

        if (request.BaseVersion.Value != deck.Version)
        {
            throw new ConflictException(
                $"Deck version conflict. Client version: {request.BaseVersion.Value}, Server version: {deck.Version}.");
        }

        if (request.Title != null)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                throw new ArgumentException(
                    "Deck title cannot be empty.");
            }

            deck.Title = request.Title.Trim();
        }

        if (request.Description != null)
        {
            deck.Description = request.Description.Trim();
        }

        if (request.LanguageId.HasValue)
        {
            if (request.LanguageId.Value == Guid.Empty ||
                !await _deckRepository.LanguageExistsAsync(
                    request.LanguageId.Value,
                    cancellationToken))
            {
                throw new ArgumentException(
                    "Invalid language.");
            }

            deck.LanguageId = request.LanguageId.Value;
        }

        if (request.IsPublic.HasValue)
        {
            deck.IsPublic = request.IsPublic.Value;
        }

        if (request.Tags != null)
        {
            var newTags = request.Tags
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Select(tag => tag.Trim())
                .Distinct()
                .ToList();

            var tagsToRemove = deck.Tags
                .Where(tag => !newTags.Contains(tag.Tag))
                .ToList();

            foreach (var tag in tagsToRemove)
            {
                deck.Tags.Remove(tag);
            }

            foreach (var tag in newTags)
            {
                if (deck.Tags.Any(t => t.Tag == tag))
                {
                    continue;
                }

                deck.Tags.Add(new DeckTag
                {
                    Id = Guid.NewGuid(),
                    Tag = tag
                });
            }
        }

        deck.Version++;

        await _deckRepository.SaveChangesAsync(
            cancellationToken);

        var stats = await _deckRepository.GetDeckStatisticsAsync(
            deck.Id,
            userId,
            cancellationToken);

        return new DeckResponse
        {
            Id = deck.Id,
            OwnerId = deck.OwnerId,
            LanguageId = deck.LanguageId,
            Title = deck.Title,
            Description = deck.Description,
            IsPublic = deck.IsPublic,
            Version = deck.Version,
            Tags = deck.Tags
                .Select(t => t.Tag)
                .ToList(),
            TotalCards = stats.TotalCards,
            NewCards = stats.NewCards,
            DueCards = stats.DueCards,
            CreatedAt = deck.CreatedAt,
            UpdatedAt = deck.UpdatedAt
        };
    }

    public async Task DeleteDeckAsync(
        Guid deckId,
        CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var deck = await _deckRepository.GetByIdAsync(
            deckId, cancellationToken);

        if (deck == null || deck.OwnerId != userId)
        {
            throw new NotFoundException(
                "The requested deck does not exist or is not accessible.");
        }

        _deckRepository.Delete(deck);

        await _deckRepository.SaveChangesAsync(
            cancellationToken);
    }
}

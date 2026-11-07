
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Cards;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Services;

public class CardService : ICardService
{
    private readonly ICardRepository _cardRepository;
    private readonly IDeckRepository _deckRepository;
    private readonly ICurrentUserService _currentUserService;

    public CardService(
        ICardRepository cardRepository,
        IDeckRepository deckRepository,
        ICurrentUserService currentUserService)
    {
        _cardRepository = cardRepository;
        _deckRepository = deckRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<CardResponse>> GetCardsAsync(
        Guid deckId,
        CancellationToken cancellationToken = default)
    {
        await GetAccessibleDeckAsync(
            deckId, false, cancellationToken);

        var cards = await _cardRepository.GetByDeckIdAsync(
            deckId, cancellationToken);

        return cards.Select(ToResponse).ToList();
    }

    public async Task<CardResponse> GetCardByIdAsync(
        Guid deckId,
        Guid cardId,
        CancellationToken cancellationToken = default)
    {
        await GetAccessibleDeckAsync(
            deckId, false, cancellationToken);

        var card = await GetRequiredCardAsync(
            deckId, cardId, cancellationToken);

        return ToResponse(card);
    }

    public async Task<CardResponse> CreateCardAsync(
        Guid deckId,
        CreateCardRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await GetAccessibleDeckAsync(
            deckId, true, cancellationToken);

        if (string.IsNullOrWhiteSpace(request.Front))
        {
            throw new ArgumentException(
                "Card front is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Back))
        {
            throw new ArgumentException(
                "Card back is required.");
        }

        var card = new Card
        {
            Id = Guid.NewGuid(),
            DeckId = deckId,
            Front = request.Front.Trim(),
            Back = request.Back.Trim(),
            Version = 1
        };

        await _cardRepository.AddAsync(
            card, cancellationToken);

        await _cardRepository.SaveChangesAsync(
            cancellationToken);

        return ToResponse(card);
    }

    public async Task<CardResponse> UpdateCardAsync(
        Guid deckId,
        Guid cardId,
        UpdateCardRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        await GetAccessibleDeckAsync(
            deckId, true, cancellationToken);

        var card = await GetRequiredCardAsync(
            deckId, cardId, cancellationToken);

        if (!request.BaseVersion.HasValue)
        {
            throw new ArgumentException(
                "BaseVersion is required.");
        }

        if (request.BaseVersion.Value != card.Version)
        {
            throw new ConflictException(
                $"Card version conflict. Client version: {request.BaseVersion.Value}, Server version: {card.Version}.");
        }

        if (request.Front != null)
        {
            if (string.IsNullOrWhiteSpace(request.Front))
            {
                throw new ArgumentException(
                    "Card front cannot be empty.");
            }

            card.Front = request.Front.Trim();
        }

        if (request.Back != null)
        {
            if (string.IsNullOrWhiteSpace(request.Back))
            {
                throw new ArgumentException(
                    "Card back cannot be empty.");
            }

            card.Back = request.Back.Trim();
        }

        card.Version++;

        await _cardRepository.SaveChangesAsync(
            cancellationToken);

        return ToResponse(card);
    }

    public async Task DeleteCardAsync(
        Guid deckId,
        Guid cardId,
        CancellationToken cancellationToken = default)
    {
        await GetAccessibleDeckAsync(
            deckId, true, cancellationToken);

        var card = await GetRequiredCardAsync(
            deckId, cardId, cancellationToken);

        var hasReviewEvents =
            await _cardRepository.HasReviewEventsAsync(
                cardId, cancellationToken);

        if (hasReviewEvents)
        {
            throw new ConflictException(
                "Không thể xóa Flashcard đã có lịch sử ôn tập.");
        }

        _cardRepository.Delete(card);

        await _cardRepository.SaveChangesAsync(
            cancellationToken);
    }

    private async Task<Deck> GetAccessibleDeckAsync(
        Guid deckId,
        bool requireOwner,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.GetRequiredUserId();

        var deck = await _deckRepository.GetByIdAsync(
            deckId, cancellationToken);

        if (deck == null ||
            (requireOwner
                ? deck.OwnerId != userId
                : deck.OwnerId != userId && !deck.IsPublic))
        {
            throw new NotFoundException(
                "The requested deck does not exist or is not accessible.");
        }

        return deck;
    }

    private async Task<Card> GetRequiredCardAsync(
        Guid deckId,
        Guid cardId,
        CancellationToken cancellationToken)
    {
        var card = await _cardRepository.GetByIdAsync(
            deckId, cardId, cancellationToken);

        if (card == null)
        {
            throw new NotFoundException(
                "The requested card does not exist.");
        }

        return card;
    }

    private static CardResponse ToResponse(Card card)
    {
        return new CardResponse
        {
            Id = card.Id,
            DeckId = card.DeckId,
            Front = card.Front,
            Back = card.Back,
            Version = card.Version
        };
    }
}

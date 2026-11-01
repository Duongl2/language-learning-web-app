
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Reviews;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Srs;

namespace LanguageLearning.Application.Services;

public sealed class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly IDeckRepository _deckRepository;
    private readonly ICurrentUserService _currentUserService;

    public ReviewService(
        IReviewRepository reviewRepository,
        IDeckRepository deckRepository,
        ICurrentUserService currentUserService)
    {
        _reviewRepository = reviewRepository;
        _deckRepository = deckRepository;
        _currentUserService = currentUserService;
    }

    public async Task<List<DueCardResponse>> GetDueCardsAsync(
        Guid deckId,
        CancellationToken cancellationToken = default)
    {
        if (deckId == Guid.Empty)
        {
            throw new ArgumentException("A valid deck ID is required.");
        }

        var userId = _currentUserService.GetRequiredUserId();

        var deck = await _deckRepository.GetByIdAsync(
            deckId,
            cancellationToken);

        if (deck == null ||
            (deck.OwnerId != userId && !deck.IsPublic))
        {
            throw new NotFoundException(
                "The requested deck does not exist or is not accessible.");
        }

        var nowUtc = DateTime.UtcNow;

        var schedules = await _reviewRepository.GetDueSchedulesAsync(
            userId,
            deckId,
            nowUtc,
            cancellationToken);

        return schedules.Select(schedule => new DueCardResponse
        {
            CardId = schedule.CardId,
            DeckId = schedule.Card.DeckId,
            Front = schedule.Card.Front,
            Back = schedule.Card.Back,
            DueAt = schedule.DueAt,
            AlgorithmVersion = schedule.AlgorithmVersion,

            Intervals = schedule.AlgorithmVersion == "srs-v1"
                ? ReviewIntervalCalculator.Calculate(schedule.StateJson)
                : null
        }).ToList();
    }
}

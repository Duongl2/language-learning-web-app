
using System.Text.Json;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Reviews;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Srs;
using LanguageLearning.Domain.Entities;

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

    public async Task<SubmitReviewResponse> SubmitReviewAsync(
        SubmitReviewRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (request.CardId == Guid.Empty ||
            request.ClientEventId == Guid.Empty)
        {
            throw new ArgumentException(
                "CardId and ClientEventId are required.");
        }

        if (!Enum.IsDefined(request.Rating))
        {
            throw new ArgumentException("Invalid review rating.");
        }

        var userId = _currentUserService.GetRequiredUserId();

        var existingEvent =
            await _reviewRepository.GetReviewEventByClientEventIdAsync(
                userId,
                request.ClientEventId,
                cancellationToken);

        if (existingEvent != null)
        {
            if (existingEvent.CardId != request.CardId ||
                existingEvent.Rating != request.Rating)
            {
                throw new ConflictException(
                    "ClientEventId was already used for another review.");
            }

            var schedule = await _reviewRepository.GetScheduleAsync(
                userId,
                request.CardId,
                cancellationToken);

            return new SubmitReviewResponse
            {
                CardId = existingEvent.CardId,
                ClientEventId = existingEvent.ClientEventId,
                AlreadyProcessed = true,
                NextDueAt = schedule?.DueAt
            };
        }

        var currentSchedule = await _reviewRepository.GetScheduleAsync(
            userId,
            request.CardId,
            cancellationToken);

        if (currentSchedule == null)
        {
            throw new NotFoundException(
                "Review schedule not found.");
        }

        if (currentSchedule.AlgorithmVersion != "srs-v1")
        {
            throw new ConflictException(
                "Unsupported SRS algorithm version.");
        }

        SrsState currentState;

        if (string.IsNullOrWhiteSpace(currentSchedule.StateJson) ||
            currentSchedule.StateJson.Trim() == "{}")
        {
            currentState = new SrsState(0, 0, 2.5);
        }
        else
        {
            currentState = JsonSerializer.Deserialize<SrsState>(
                currentSchedule.StateJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? throw new ConflictException(
                "Invalid SRS state.");
        }

        var nowUtc = DateTime.UtcNow;
        var scheduler = new SrsV1Scheduler();

        var (updatedState, nextDueAt) = scheduler.Schedule(
            currentState,
            request.Rating,
            nowUtc);

        var reviewEvent = new ReviewEvent
        {
            UserId = userId,
            CardId = request.CardId,
            ClientEventId = request.ClientEventId,
            Rating = request.Rating,
            ReviewedAt = nowUtc,
            AlgorithmVersion = scheduler.AlgorithmVersion
        };

        currentSchedule.StateJson = JsonSerializer.Serialize(updatedState);
        currentSchedule.DueAt = nextDueAt;
        currentSchedule.LastReviewedAt = nowUtc;

        await _reviewRepository.AddReviewEventAsync(
            reviewEvent,
            cancellationToken);
        try
        {
            await _reviewRepository.SaveChangesAsync(cancellationToken);

            return new SubmitReviewResponse
            {
                CardId = request.CardId,
                ClientEventId = request.ClientEventId,
                AlreadyProcessed = false,
                NextDueAt = nextDueAt
            };
        }
        catch (DuplicateReviewEventException)
        {
            var savedEvent =
                await _reviewRepository.GetReviewEventByClientEventIdAsync(
                    userId,
                    request.ClientEventId,
                    cancellationToken);

            if (savedEvent == null)
            {
                throw;
            }

            if (savedEvent.CardId != request.CardId ||
                savedEvent.Rating != request.Rating)
            {
                throw new ConflictException(
                    "ClientEventId was already used for another review.");
            }

            var savedSchedule =
                await _reviewRepository.GetScheduleAsync(
                    userId,
                    request.CardId,
                    cancellationToken);

            return new SubmitReviewResponse
            {
                CardId = savedEvent.CardId,
                ClientEventId = savedEvent.ClientEventId,
                AlreadyProcessed = true,
                NextDueAt = savedSchedule?.DueAt
            };
        }
    }

    public async Task<List<DeckScheduleResponse>> GetDeckSchedulesAsync(
        Guid deckId,
        CancellationToken cancellationToken = default)
    {
        if (deckId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid deck ID is required.");
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

        var schedules = await _reviewRepository.GetDeckSchedulesAsync(
            userId,
            deckId,
            cancellationToken);

        return schedules.Select(schedule => new DeckScheduleResponse
        {
            CardId = schedule.CardId,
            DueAt = schedule.DueAt,
            StateJson = schedule.StateJson,
            AlgorithmVersion = schedule.AlgorithmVersion,
            LastReviewedAt = schedule.LastReviewedAt
        }).ToList();
    }

    public async Task<List<DueCardResponse>> GetDueCardsAsync(
        Guid deckId,
        CancellationToken cancellationToken = default)
    {
        if (deckId == Guid.Empty)
        {
            throw new ArgumentException(
                "A valid deck ID is required.");
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

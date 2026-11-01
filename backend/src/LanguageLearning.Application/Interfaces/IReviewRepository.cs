
using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface IReviewRepository
{
    Task<List<CardSchedule>> GetDueSchedulesAsync(
        Guid userId,
        Guid deckId,
        DateTime nowUtc,
        CancellationToken cancellationToken = default);

    Task<CardSchedule?> GetScheduleAsync(
        Guid userId,
        Guid cardId,
        CancellationToken cancellationToken = default);

    Task AddScheduleAsync(
        CardSchedule schedule,
        CancellationToken cancellationToken = default);

    Task AddReviewEventAsync(
        ReviewEvent reviewEvent,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);

    Task<ReviewEvent?> GetReviewEventByClientEventIdAsync(
        Guid userId,
        Guid clientEventId,
        CancellationToken cancellationToken = default);

}

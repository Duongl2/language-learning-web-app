
using System.Text.Json;
using LanguageLearning.Application.DTOs.Reviews;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Application.Srs;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using LanguageLearning.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using MySqlConnector;
using Xunit;

namespace LanguageLearning.Tests;

public class ReviewConcurrencyTests
{
    [Fact]
    public async Task ConcurrentSubmissions_SameClientEventId_AppliedOnce()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ISSUE27_TEST_CONNECTION")
            ?? throw new InvalidOperationException(
                "ISSUE27_TEST_CONNECTION is not configured.");

        var connectionBuilder =
            new MySqlConnectionStringBuilder(connectionString);

        if (connectionBuilder.Database != "language_learning_issue27_test" &&
            connectionBuilder.Database != "language_learning_test_db")
        {
            throw new InvalidOperationException(
                "Concurrency tests must use a dedicated test database.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        var user = new User
        {
            Email = $"issue27-{Guid.NewGuid():N}@test.local"
        };

        var language = new Language
        {
            Code = "en",
            Name = "English"
        };

        var deck = new Deck
        {
            Owner = user,
            Language = language,
            Title = "Issue 27 Test Deck"
        };

        var card = new Card
        {
            Deck = deck,
            Front = "Hello",
            Back = "Xin chào"
        };

        var schedule = new CardSchedule
        {
            User = user,
            Card = card,
            DueAt = DateTime.UtcNow.AddMinutes(-1),
            StateJson = JsonSerializer.Serialize(
                new SrsState(0, 0, 2.5)),
            AlgorithmVersion = "srs-v1"
        };

        await using (var seedContext = new AppDbContext(options))
        {
            seedContext.CardSchedules.Add(schedule);
            await seedContext.SaveChangesAsync();
        }

        var currentUser = new Mock<ICurrentUserService>();
        currentUser
            .Setup(x => x.GetRequiredUserId())
            .Returns(user.Id);

        var deckRepository = new Mock<IDeckRepository>();

        await using var contextA = new AppDbContext(options);
        await using var contextB = new AppDbContext(options);

        
var checkCount = 0;

var bothChecked = new TaskCompletionSource<bool>(
    TaskCreationOptions.RunContinuationsAsynchronously);

async Task WaitForBothChecksAsync()
{
    if (Interlocked.Increment(ref checkCount) == 2)
    {
        bothChecked.TrySetResult(true);
    }

    await bothChecked.Task.WaitAsync(
        TimeSpan.FromSeconds(10));
}

        var serviceA = new ReviewService(
            new SynchronizingReviewRepository(
                new ReviewRepository(contextA),
                WaitForBothChecksAsync),
            deckRepository.Object,
            currentUser.Object);

        var serviceB = new ReviewService(
            new SynchronizingReviewRepository(
                new ReviewRepository(contextB),
                WaitForBothChecksAsync),
            deckRepository.Object,
            currentUser.Object);

        var clientEventId = Guid.NewGuid();

        var request = new SubmitReviewRequest
        {
            CardId = card.Id,
            ClientEventId = clientEventId,
            Rating = SrsRating.Good
        };

        var start = new TaskCompletionSource(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Task<SubmitReviewResponse> SendAsync(
            ReviewService service)
        {
            return Task.Run(async () =>
            {
                await start.Task;
                return await service.SubmitReviewAsync(request);
            });
        }

        var taskA = SendAsync(serviceA);
        var taskB = SendAsync(serviceB);

        start.SetResult();

        var results = await Task.WhenAll(taskA, taskB);

        await using var verifyContext = new AppDbContext(options);

        var savedEvents = await verifyContext.ReviewEvents
            .AsNoTracking()
            .Where(e =>
                e.UserId == user.Id &&
                e.ClientEventId == clientEventId)
            .ToListAsync();

        var savedSchedule = await verifyContext.CardSchedules
            .AsNoTracking()
            .SingleAsync(s =>
                s.UserId == user.Id &&
                s.CardId == card.Id);

        var finalState = JsonSerializer.Deserialize<SrsState>(
            savedSchedule.StateJson);

        Assert.Single(savedEvents);

        Assert.Single(results, r => !r.AlreadyProcessed);
        Assert.Single(results, r => r.AlreadyProcessed);

        Assert.NotNull(finalState);
        Assert.Equal(1, finalState!.Repetitions);
        Assert.Equal(1.0, finalState.IntervalDays);
        Assert.Equal(2.5, finalState.EaseFactor);
    }
    
private sealed class SynchronizingReviewRepository
    : IReviewRepository
{
    private readonly IReviewRepository _inner;
    private readonly Func<Task> _waitAsync;

    public SynchronizingReviewRepository(
        IReviewRepository inner,
        Func<Task> waitAsync)
    {
        _inner = inner;
        _waitAsync = waitAsync;
    }

    public async Task<ReviewEvent?>
        GetReviewEventByClientEventIdAsync(
            Guid userId,
            Guid clientEventId,
            CancellationToken cancellationToken = default)
    {
        var existing =
            await _inner.GetReviewEventByClientEventIdAsync(
                userId,
                clientEventId,
                cancellationToken);

        if (existing == null)
        {
            await _waitAsync();
        }

        return existing;
    }

        public Task<List<CardSchedule>> GetDueSchedulesAsync(
            Guid userId,
            Guid deckId,
            DateTime nowUtc,
            CancellationToken cancellationToken = default)
            => _inner.GetDueSchedulesAsync(
                userId, deckId, nowUtc, cancellationToken);

        public Task<CardSchedule?> GetScheduleAsync(
            Guid userId,
            Guid cardId,
            CancellationToken cancellationToken = default)
            => _inner.GetScheduleAsync(
                userId, cardId, cancellationToken);

        public Task AddScheduleAsync(
            CardSchedule schedule,
            CancellationToken cancellationToken = default)
            => _inner.AddScheduleAsync(
                schedule, cancellationToken);

        public Task AddReviewEventAsync(
            ReviewEvent reviewEvent,
            CancellationToken cancellationToken = default)
            => _inner.AddReviewEventAsync(
                reviewEvent, cancellationToken);

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
            => _inner.SaveChangesAsync(cancellationToken);
    }

}

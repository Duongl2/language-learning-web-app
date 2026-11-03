
using System.Text.Json;
using LanguageLearning.Application.DTOs.Sync;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Application.Srs;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using LanguageLearning.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using MySqlConnector;
using Xunit;

namespace LanguageLearning.Tests;

public sealed class SyncConcurrencyTests
{
    [Fact]
    public async Task TC16_ConcurrentPush_SameOperation_AppliedOnce()
    {

        var connectionString =
            Environment.GetEnvironmentVariable("ISSUE47_TEST_CONNECTION")
            ?? Environment.GetEnvironmentVariable("ISSUE27_TEST_CONNECTION")
            ?? throw new InvalidOperationException(
                "Test database connection is not configured.");

        var connectionBuilder =
            new MySqlConnectionStringBuilder(connectionString);

        var allowedDatabases = new[]
        {
            "language_learning_issue47_test",
            "language_learning_issue27_test",
            "language_learning_test_db"
        };

        if (!allowedDatabases.Contains(
            connectionBuilder.Database,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "TC-16 requires a dedicated test database.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36)),
                mysql => mysql.EnableRetryOnFailure(
                    3,
                    TimeSpan.FromSeconds(5),
                    null))
            .Options;

        Guid userId;
        Guid cardId;

        await using (var seed = new AppDbContext(options))
        {
            await seed.Database.EnsureCreatedAsync();

            var user = new User
            {
                Email = $"issue47-{Guid.NewGuid():N}@test.local"
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
                Title = "Issue 47 TC-16"
            };

            var card = new Card
            {
                Deck = deck,
                Front = "Hello",
                Back = "Xin chao"
            };

            var schedule = new CardSchedule
            {
                User = user,
                Card = card,
                DueAt = DateTime.UtcNow.AddMinutes(-1),
                StateJson = "{}",
                AlgorithmVersion = "srs-v1"
            };

            seed.CardSchedules.Add(schedule);
            await seed.SaveChangesAsync();

            userId = user.Id;
            cardId = card.Id;
        }

        var currentUser = new Mock<ICurrentUserService>();

        currentUser
            .Setup(x => x.GetRequiredUserId())
            .Returns(userId);

        var operationId = Guid.NewGuid();
        var clientEventId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        var operation = new SyncOperationRequest
        {
            OperationId = operationId,
            Type = "Review",
            EntityId = cardId,
            OccurredAt = now,
            Payload = JsonSerializer.SerializeToElement(new
            {
                clientEventId,
                cardId,
                rating = "Good",
                reviewedAt = now,
                algorithmVersion = "srs-v1"
            })
        };

        var request = new SyncPushRequest
        {
            Operations = [operation]
        };

        var barrier = new LookupBarrier();

        var start = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously);

        Task<SyncPushResponse> SendAsync()
        {
            return Task.Run(async () =>
            {
                await start.Task;

                await using var context = new AppDbContext(options);

                var syncRepository = new CoordinatedSyncRepository(
                    new SyncRepository(context),
                    barrier);

                var reviewService = new ReviewService(
                    new ReviewRepository(context),
                    new Mock<IDeckRepository>().Object,
                    currentUser.Object);

                var syncService = new SyncService(
                    syncRepository,
                    reviewService,
                    currentUser.Object);

                return await syncService.PushAsync(request);
            });
        }

        var task1 = SendAsync();
        var task2 = SendAsync();

        start.SetResult(true);

        var responses = await Task.WhenAll(task1, task2);

        var statuses = responses
            .Select(r => Assert.Single(r.Results).Status)
            .OrderBy(s => s)
            .ToArray();

        Assert.Equal(
            new[] { "accepted", "duplicate" },
            statuses);

        await using var verify = new AppDbContext(options);

        var events = await verify.ReviewEvents
            .AsNoTracking()
            .Where(e =>
                e.UserId == userId &&
                e.ClientEventId == clientEventId)
            .ToListAsync();

        var ledgers = await verify.SyncOperations
            .AsNoTracking()
            .Where(o =>
                o.UserId == userId &&
                o.ClientOperationId == operationId)
            .ToListAsync();

        var scheduleAfter = await verify.CardSchedules
            .AsNoTracking()
            .SingleAsync(s =>
                s.UserId == userId &&
                s.CardId == cardId);

        var state = JsonSerializer.Deserialize<SrsState>(
            scheduleAfter.StateJson);

        Assert.Single(events);
        Assert.Single(ledgers);
        Assert.NotNull(state);
        Assert.Equal(1, state!.Repetitions);
        Assert.Equal(1.0, state.IntervalDays);
    }

    private sealed class LookupBarrier
    {
        private int _count;

        private readonly TaskCompletionSource<bool> _ready =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public async Task WaitAsync(
            CancellationToken cancellationToken)
        {
            if (Interlocked.Increment(ref _count) == 2)
            {
                _ready.TrySetResult(true);
            }

            await _ready.Task.WaitAsync(
                TimeSpan.FromSeconds(10),
                cancellationToken);
        }
    }

    private sealed class CoordinatedSyncRepository : ISyncRepository
    {
        private readonly ISyncRepository _inner;
        private readonly LookupBarrier _barrier;

        public CoordinatedSyncRepository(
            ISyncRepository inner,
            LookupBarrier barrier)
        {
            _inner = inner;
            _barrier = barrier;
        }

        public async Task<SyncOperation?> GetByClientOperationIdAsync(
            Guid userId,
            Guid clientOperationId,
            CancellationToken cancellationToken = default)
        {
            var existing = await _inner.GetByClientOperationIdAsync(
                userId,
                clientOperationId,
                cancellationToken);

            if (existing == null)
            {
                await _barrier.WaitAsync(cancellationToken);
            }

            return existing;
        }

        public Task AddAsync(
            SyncOperation operation,
            CancellationToken cancellationToken = default)
        {
            return _inner.AddAsync(operation, cancellationToken);
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            return _inner.SaveChangesAsync(cancellationToken);
        }

        public Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default)
        {
            return _inner.ExecuteInTransactionAsync(
                action,
                cancellationToken);
        }
    }
}

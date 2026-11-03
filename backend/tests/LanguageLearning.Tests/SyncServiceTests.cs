
using System.Text.Json;
using LanguageLearning.Application.DTOs.Reviews;
using LanguageLearning.Application.DTOs.Sync;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using Moq;
using Xunit;

namespace LanguageLearning.Tests;

public class SyncServiceTests
{
    [Fact]
    public async Task PushAsync_FirstReviewAccepted_RetryDuplicate()
    {
        var repository = new TestSyncRepository();
        var reviewService = new Mock<IReviewService>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(x => x.GetRequiredUserId())
            .Returns(Guid.NewGuid());

        reviewService
            .Setup(x => x.SubmitReviewAsync(
                It.IsAny<SubmitReviewRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmitReviewResponse
            {
                AlreadyProcessed = false
            });

        var service = new SyncService(
            repository,
            reviewService.Object,
            currentUser.Object);

        var operationId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var time = DateTimeOffset.UtcNow;

        var operation = CreateOperation(
            operationId, cardId, eventId, time, "Good");

        var request = new SyncPushRequest
        {
            Operations = [operation]
        };

        var first = await service.PushAsync(request);
        var second = await service.PushAsync(request);

        Assert.Equal(
            "accepted",
            Assert.Single(first.Results).Status);

        Assert.Equal(
            "duplicate",
            Assert.Single(second.Results).Status);

        reviewService.Verify(
            x => x.SubmitReviewAsync(
                It.IsAny<SubmitReviewRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PushAsync_SameOperationIdDifferentData_ReturnsConflict()
    {
        var repository = new TestSyncRepository();
        var reviewService = new Mock<IReviewService>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(x => x.GetRequiredUserId())
            .Returns(Guid.NewGuid());

        reviewService
            .Setup(x => x.SubmitReviewAsync(
                It.IsAny<SubmitReviewRequest>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new SubmitReviewResponse());

        var service = new SyncService(
            repository,
            reviewService.Object,
            currentUser.Object);

        var operationId = Guid.NewGuid();
        var cardId = Guid.NewGuid();
        var eventId = Guid.NewGuid();
        var time = DateTimeOffset.UtcNow;

        var first = CreateOperation(
            operationId, cardId, eventId, time, "Good");

        var changed = CreateOperation(
            operationId, cardId, eventId, time, "Easy");

        var accepted = await service.PushAsync(
            new SyncPushRequest { Operations = [first] });

        var conflict = await service.PushAsync(
            new SyncPushRequest { Operations = [changed] });

        Assert.Equal(
            "accepted",
            Assert.Single(accepted.Results).Status);

        Assert.Equal(
            "conflict",
            Assert.Single(conflict.Results).Status);

        reviewService.Verify(
            x => x.SubmitReviewAsync(
                It.IsAny<SubmitReviewRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PushAsync_InvalidOperations_ReturnIndependentStatuses()
    {
        var repository = new TestSyncRepository();
        var reviewService = new Mock<IReviewService>();
        var currentUser = new Mock<ICurrentUserService>();

        currentUser.Setup(x => x.GetRequiredUserId())
            .Returns(Guid.NewGuid());

        var service = new SyncService(
            repository,
            reviewService.Object,
            currentUser.Object);

        var time = DateTimeOffset.UtcNow;

        var unsupported = CreateOperation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            time,
            "Good");

        unsupported.Type = "Unsupported";

        var invalidVersion = CreateOperation(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            time,
            "Good",
            "srs-v999");

        var result = await service.PushAsync(
            new SyncPushRequest
            {
                Operations = [unsupported, invalidVersion]
            });

        Assert.Equal(2, result.Results.Count);
        Assert.Equal("rejected", result.Results[0].Status);
        Assert.Equal("conflict", result.Results[1].Status);

        reviewService.Verify(
            x => x.SubmitReviewAsync(
                It.IsAny<SubmitReviewRequest>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static SyncOperationRequest CreateOperation(
        Guid operationId,
        Guid cardId,
        Guid eventId,
        DateTimeOffset time,
        string rating,
        string version = "srs-v1")
    {
        return new SyncOperationRequest
        {
            OperationId = operationId,
            Type = "Review",
            EntityId = cardId,
            OccurredAt = time,
            Payload = JsonSerializer.SerializeToElement(new
            {
                clientEventId = eventId,
                cardId,
                rating,
                reviewedAt = time,
                algorithmVersion = version
            })
        };
    }

    private sealed class TestSyncRepository : ISyncRepository
    {
        private readonly Dictionary<(Guid, Guid), SyncOperation>
            _operations = new();

        private SyncOperation? _pending;

        public Task<SyncOperation?> GetByClientOperationIdAsync(
            Guid userId,
            Guid clientOperationId,
            CancellationToken cancellationToken = default)
        {
            _operations.TryGetValue(
                (userId, clientOperationId),
                out var operation);

            return Task.FromResult(operation);
        }

        public Task AddAsync(
            SyncOperation operation,
            CancellationToken cancellationToken = default)
        {
            _pending = operation;
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            if (_pending != null)
            {
                _operations.Add(
                    (_pending.UserId, _pending.ClientOperationId),
                    _pending);

                _pending = null;
            }

            return Task.CompletedTask;
        }

        public Task<TResult> ExecuteInTransactionAsync<TResult>(
            Func<CancellationToken, Task<TResult>> action,
            CancellationToken cancellationToken = default)
        {
            return action(cancellationToken);
        }
    }
}

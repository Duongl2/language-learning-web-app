
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using Moq;
using Xunit;

namespace LanguageLearning.Tests;

public class SyncPullServiceTests
{
    private static SyncPullService CreateService(
        Mock<ISyncPullRepository> repositoryMock,
        Guid userId)
    {
        var currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock
            .Setup(service => service.GetRequiredUserId())
            .Returns(userId);

        return new SyncPullService(
            repositoryMock.Object,
            currentUserMock.Object);
    }

    private static Deck CreateDeck(
        Guid userId,
        string title,
        DateTime changedAt,
        int version = 1,
        bool isDeleted = false)
    {
        return new Deck
        {
            Id = Guid.NewGuid(),
            OwnerId = userId,
            LanguageId = Guid.NewGuid(),
            Title = title,
            Description = "Test description",
            Version = version,
            IsDeleted = isDeleted,
            CreatedAt = changedAt.AddDays(-1),
            UpdatedAt = changedAt,
            DeletedAt = isDeleted ? changedAt : null,
            Tags = new List<DeckTag>
            {
                new DeckTag
                {
                    Id = Guid.NewGuid(),
                    Tag = "test"
                }
            }
        };
    }

    [Fact]
    public async Task PullAsync_FirstPage_ReturnsChangesAndHasMore()
    {
        var userId = Guid.NewGuid();
        var now = new DateTime(
            2026, 11, 3, 10, 0, 0,
            DateTimeKind.Utc);

        var deck1 = CreateDeck(
            userId, "Deck 1", now);

        var deck2 = CreateDeck(
            userId, "Deck 2", now.AddMinutes(1));

        var deck3 = CreateDeck(
            userId, "Deck 3", now.AddMinutes(2));

        var repositoryMock = new Mock<ISyncPullRepository>();

        repositoryMock
            .Setup(repo => repo.GetChangedDecksAsync(
                userId,
                It.IsAny<DateTime>(),
                Guid.Empty,
                3,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Deck>
            {
                deck1,
                deck2,
                deck3
            });

        var service = CreateService(
            repositoryMock, userId);

        var result = await service.PullAsync(null, 2);

        Assert.Equal(2, result.Changes.Count);
        Assert.True(result.HasMore);
        Assert.False(
            string.IsNullOrWhiteSpace(result.NextCursor));

        Assert.Equal(deck1.Id, result.Changes[0].EntityId);
        Assert.Equal(deck2.Id, result.Changes[1].EntityId);
        Assert.Equal("deck", result.Changes[0].EntityType);
        Assert.Equal("upsert", result.Changes[0].Action);

        Assert.NotNull(result.Changes[0].Deck);
        Assert.Equal("Deck 1", result.Changes[0].Deck!.Title);
        Assert.Contains("test", result.Changes[0].Deck!.Tags);
    }

    [Fact]
    public async Task PullAsync_NextCursor_ReturnsNextPage()
    {
        var userId = Guid.NewGuid();

        var now = new DateTime(
            2026, 11, 3, 11, 0, 0,
            DateTimeKind.Utc);

        var deck1 = CreateDeck(userId, "Deck 1", now);
        var deck2 = CreateDeck(userId, "Deck 2", now);

        var repositoryMock = new Mock<ISyncPullRepository>();

        repositoryMock
            .Setup(repo => repo.GetChangedDecksAsync(
                userId,
                It.IsAny<DateTime>(),
                Guid.Empty,
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Deck>
            {
                deck1,
                deck2
            });

        repositoryMock
            .Setup(repo => repo.GetChangedDecksAsync(
                userId,
                now,
                deck1.Id,
                2,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Deck>
            {
                deck2
            });

        var service = CreateService(
            repositoryMock, userId);

        var page1 = await service.PullAsync(null, 1);

        var page2 = await service.PullAsync(
            page1.NextCursor, 1);

        Assert.Single(page1.Changes);
        Assert.Single(page2.Changes);

        Assert.Equal(deck1.Id, page1.Changes[0].EntityId);
        Assert.Equal(deck2.Id, page2.Changes[0].EntityId);

        Assert.False(page2.HasMore);
        Assert.NotEqual(
            page1.NextCursor,
            page2.NextCursor);

        repositoryMock.Verify(
            repo => repo.GetChangedDecksAsync(
                userId,
                now,
                deck1.Id,
                2,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task PullAsync_DeletedDeck_ReturnsTombstone()
    {
        var userId = Guid.NewGuid();

        var now = new DateTime(
            2026, 11, 3, 12, 0, 0,
            DateTimeKind.Utc);

        var deletedDeck = CreateDeck(
            userId,
            "Deleted Deck",
            now,
            version: 2,
            isDeleted: true);

        var repositoryMock = new Mock<ISyncPullRepository>();

        repositoryMock
            .Setup(repo => repo.GetChangedDecksAsync(
                userId,
                It.IsAny<DateTime>(),
                Guid.Empty,
                51,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Deck>
            {
                deletedDeck
            });

        var service = CreateService(
            repositoryMock, userId);

        var result = await service.PullAsync(null);

        var change = Assert.Single(result.Changes);

        Assert.Equal(deletedDeck.Id, change.EntityId);
        Assert.Equal("deck", change.EntityType);
        Assert.Equal("delete", change.Action);
        Assert.Equal(2, change.Version);
        Assert.Null(change.Deck);
        Assert.False(result.HasMore);
    }

    [Fact]
    public async Task PullAsync_NoNewChanges_ReturnsEmptyList()
    {
        var userId = Guid.NewGuid();

        var now = new DateTime(
            2026, 11, 3, 13, 0, 0,
            DateTimeKind.Utc);

        var deck = CreateDeck(userId, "Deck", now);

        var repositoryMock = new Mock<ISyncPullRepository>();

        repositoryMock
            .Setup(repo => repo.GetChangedDecksAsync(
                userId,
                It.IsAny<DateTime>(),
                Guid.Empty,
                51,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Deck>
            {
                deck
            });

        repositoryMock
            .Setup(repo => repo.GetChangedDecksAsync(
                userId,
                now,
                deck.Id,
                51,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Deck>());

        var service = CreateService(
            repositoryMock, userId);

        var first = await service.PullAsync(null);

        var second = await service.PullAsync(
            first.NextCursor);

        Assert.Single(first.Changes);
        Assert.Empty(second.Changes);
        Assert.False(second.HasMore);
        Assert.Equal(
            first.NextCursor,
            second.NextCursor);
    }

    [Fact]
    public async Task PullAsync_InvalidCursor_ThrowsArgumentException()
    {
        var userId = Guid.NewGuid();
        var repositoryMock = new Mock<ISyncPullRepository>();

        var service = CreateService(
            repositoryMock, userId);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.PullAsync("invalid-cursor"));

        repositoryMock.Verify(
            repo => repo.GetChangedDecksAsync(
                It.IsAny<Guid>(),
                It.IsAny<DateTime>(),
                It.IsAny<Guid>(),
                It.IsAny<int>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public async Task PullAsync_InvalidLimit_ThrowsArgumentException(
        int limit)
    {
        var userId = Guid.NewGuid();
        var repositoryMock = new Mock<ISyncPullRepository>();

        var service = CreateService(
            repositoryMock, userId);

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.PullAsync(null, limit));
    }
}


using LanguageLearning.Application.Common;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Decks;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using Moq;
using Xunit;

namespace LanguageLearning.Tests;

public class DeckServiceTests
{
    private static DeckService CreateService(
        Mock<IDeckRepository> repositoryMock,
        Guid currentUserId)
    {
        var currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock
            .Setup(service => service.GetRequiredUserId())
            .Returns(currentUserId);

        return new DeckService(
            repositoryMock.Object,
            currentUserMock.Object);
    }

    [Fact]
    public async Task UpdateDeck_OwnerCanUpdate_Success()
    {
        var ownerId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "Old Title",
            Description = "Old Description",
            IsPublic = false,
            Version = 1
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        repositoryMock
            .Setup(repo => repo.GetDeckStatisticsAsync(
                deckId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0));

        var service = CreateService(repositoryMock, ownerId);

        var request = new UpdateDeckRequest
        {
            BaseVersion = 1,
            Title = "New Title"
        };

        var result = await service.UpdateDeckAsync(
            deckId,
            request);

        Assert.NotNull(result);
        Assert.Equal("New Title", result.Title);
        Assert.Equal("New Title", deck.Title);
        Assert.Equal(2, deck.Version);
        Assert.Equal(2, result.Version);

        repositoryMock.Verify(
            repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateDeck_NonOwnerCannotUpdate_TC03()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "Owner Deck",
            Description = "Private Deck",
            IsPublic = false,
            Version = 1
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        var service = CreateService(
            repositoryMock,
            otherUserId);

        var request = new UpdateDeckRequest
        {
            BaseVersion = 1,
            Title = "Unauthorized Change"
        };

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.UpdateDeckAsync(
                deckId,
                request));

        Assert.Equal("Owner Deck", deck.Title);
        Assert.Equal(1, deck.Version);

        repositoryMock.Verify(
            repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateDeck_MissingBaseVersion_ThrowsArgumentException()
    {
        var ownerId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "Original Title",
            Version = 1
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        var service = CreateService(repositoryMock, ownerId);

        var request = new UpdateDeckRequest
        {
            Title = "Changed Title"
        };

        var exception = await Assert.ThrowsAsync<ArgumentException>(
            () => service.UpdateDeckAsync(deckId, request));

        Assert.Contains("BaseVersion", exception.Message);
        Assert.Equal("Original Title", deck.Title);
        Assert.Equal(1, deck.Version);

        repositoryMock.Verify(
            repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task UpdateDeck_StaleBaseVersion_ThrowsConflict_TC17()
    {
        var ownerId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "Device A Updated",
            Version = 2
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        var service = CreateService(repositoryMock, ownerId);

        var request = new UpdateDeckRequest
        {
            BaseVersion = 1,
            Title = "Device B Updated"
        };

        var exception = await Assert.ThrowsAsync<ConflictException>(
            () => service.UpdateDeckAsync(deckId, request));

        Assert.Contains("Deck version conflict", exception.Message);
        Assert.Equal("Device A Updated", deck.Title);
        Assert.Equal(2, deck.Version);

        repositoryMock.Verify(
            repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task DeleteDeck_OwnerCanDelete_Success()
    {
        var ownerId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "Owner Deck"
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        var service = CreateService(
            repositoryMock,
            ownerId);

        await service.DeleteDeckAsync(deckId);

        repositoryMock.Verify(
            repo => repo.Delete(deck),
            Times.Once);

        repositoryMock.Verify(
            repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task DeleteDeck_NonOwnerCannotDelete_TC03()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "Owner Deck"
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        var service = CreateService(
            repositoryMock,
            otherUserId);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.DeleteDeckAsync(deckId));

        repositoryMock.Verify(
            repo => repo.Delete(It.IsAny<Deck>()),
            Times.Never);

        repositoryMock.Verify(
            repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateDeck_NormalizesAndRemovesDuplicateTags()
    {
        var userId = Guid.NewGuid();
        var languageId = Guid.NewGuid();
        Deck? savedDeck = null;

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.LanguageExistsAsync(
                languageId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        repositoryMock
            .Setup(repo => repo.AddAsync(
                It.IsAny<Deck>(),
                It.IsAny<CancellationToken>()))
            .Callback<Deck, CancellationToken>(
                (deck, _) => savedDeck = deck)
            .Returns(Task.CompletedTask);

        var service = CreateService(
            repositoryMock,
            userId);

        var request = new CreateDeckRequest
        {
            Title = "English Vocabulary",
            LanguageId = languageId,
            IsPublic = true,
            Tags = new List<string>
            {
                "English",
                " English ",
                "Grammar",
                "",
                "   "
            }
        };

        var result = await service.CreateDeckAsync(request);

        Assert.NotNull(savedDeck);
        Assert.Equal(userId, result.OwnerId);
        Assert.Equal(userId, savedDeck.OwnerId);
        Assert.True(result.IsPublic);
        Assert.Equal(2, result.Tags.Count);
        Assert.Contains("English", result.Tags);
        Assert.Contains("Grammar", result.Tags);

        repositoryMock.Verify(
            repo => repo.AddAsync(
                It.IsAny<Deck>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        repositoryMock.Verify(
            repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task UpdateDeck_ReplacesTagsCorrectly()
    {
        var ownerId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "English Deck",
            Version = 1
        };

        deck.Tags.Add(new DeckTag
        {
            Id = Guid.NewGuid(),
            Tag = "English"
        });

        deck.Tags.Add(new DeckTag
        {
            Id = Guid.NewGuid(),
            Tag = "Vocabulary"
        });

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        repositoryMock
            .Setup(repo => repo.GetDeckStatisticsAsync(
                deckId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0));

        var service = CreateService(
            repositoryMock,
            ownerId);

        var request = new UpdateDeckRequest
        {
            BaseVersion = 1,
            Tags = new List<string>
            {
                "English",
                "Grammar",
                " Grammar "
            }
        };

        var result = await service.UpdateDeckAsync(
            deckId,
            request);

        Assert.NotNull(result);
        Assert.Equal(2, result.Tags.Count);
        Assert.Contains("English", result.Tags);
        Assert.Contains("Grammar", result.Tags);
        Assert.DoesNotContain("Vocabulary", result.Tags);

        repositoryMock.Verify(
            repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDeck_PublicDeckAccessibleToOtherUser()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "Public Deck",
            IsPublic = true
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        repositoryMock
            .Setup(repo => repo.GetDeckStatisticsAsync(
                deckId,
                otherUserId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((0, 0, 0));

        var service = CreateService(
            repositoryMock,
            otherUserId);

        var result = await service.GetDeckByIdAsync(deckId);

        Assert.NotNull(result);
        Assert.Equal(deckId, result.Id);
        Assert.True(result.IsPublic);

        repositoryMock.Verify(
            repo => repo.GetDeckStatisticsAsync(
                deckId,
                otherUserId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDeck_PrivateDeckNotAccessibleToOtherUser()
    {
        var ownerId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "Private Deck",
            IsPublic = false
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        var service = CreateService(
            repositoryMock,
            otherUserId);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.GetDeckByIdAsync(deckId));

        repositoryMock.Verify(
            repo => repo.GetDeckStatisticsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetDeckById_ReturnsCorrectCardStatistics()
    {
        var ownerId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            LanguageId = Guid.NewGuid(),
            Title = "English Vocabulary",
            IsPublic = false
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        repositoryMock
            .Setup(repo => repo.GetDeckStatisticsAsync(
                deckId,
                ownerId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((10, 4, 3));

        var service = CreateService(
            repositoryMock,
            ownerId);

        var result = await service.GetDeckByIdAsync(deckId);

        Assert.NotNull(result);
        Assert.Equal(10, result.TotalCards);
        Assert.Equal(4, result.NewCards);
        Assert.Equal(3, result.DueCards);

        repositoryMock.Verify(
            repo => repo.GetDeckStatisticsAsync(
                deckId,
                ownerId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDecks_ReturnsPaginationAndBatchStatistics()
    {
        var userId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = userId,
            LanguageId = Guid.NewGuid(),
            Title = "English Deck",
            IsPublic = false
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetAccessibleDecksAsync(
                userId,
                2,
                10,
                false,
                It.IsAny<CancellationToken>(),
                null))
            .ReturnsAsync((
                new List<Deck> { deck },
                25));

        repositoryMock
            .Setup(repo => repo.GetDeckStatisticsBatchAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<Guid,
                    (int TotalCards, int NewCards, int DueCards)>
                {
                    [deckId] = (10, 4, 3)
                });

        var service = CreateService(
            repositoryMock,
            userId);

        var pagination = new PaginationParams
        {
            Page = 2,
            PageSize = 10
        };

        var result = await service.GetDecksAsync(pagination);

        Assert.Equal(2, result.Page);
        Assert.Equal(10, result.PageSize);
        Assert.Equal(25, result.TotalItems);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasNextPage);
        Assert.True(result.HasPreviousPage);

        Assert.Single(result.Items);
        Assert.Equal(deckId, result.Items[0].Id);
        Assert.Equal(10, result.Items[0].TotalCards);
        Assert.Equal(4, result.Items[0].NewCards);
        Assert.Equal(3, result.Items[0].DueCards);

        repositoryMock.Verify(
            repo => repo.GetDeckStatisticsBatchAsync(
                It.Is<IReadOnlyCollection<Guid>>(
                    ids => ids.Contains(deckId)),
                userId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        repositoryMock.Verify(
            repo => repo.GetDeckStatisticsAsync(
                It.IsAny<Guid>(),
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetDecks_OnlyMineTrue_PassesFilterToRepository()
    {
        var userId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = userId,
            LanguageId = Guid.NewGuid(),
            Title = "My English Deck",
            IsPublic = false
        };

        var repositoryMock = new Mock<IDeckRepository>();

        repositoryMock
            .Setup(repo => repo.GetAccessibleDecksAsync(
                userId,
                1,
                10,
                true,
                It.IsAny<CancellationToken>(),
                null))
            .ReturnsAsync((
                new List<Deck> { deck },
                1));

        repositoryMock
            .Setup(repo => repo.GetDeckStatisticsBatchAsync(
                It.IsAny<IReadOnlyCollection<Guid>>(),
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(
                new Dictionary<Guid,
                    (int TotalCards, int NewCards, int DueCards)>
                {
                    [deckId] = (20, 5, 8)
                });

        var service = CreateService(
            repositoryMock,
            userId);

        var pagination = new PaginationParams
        {
            Page = 1,
            PageSize = 10
        };

        var result = await service.GetDecksAsync(
            pagination,
            onlyMine: true);

        Assert.Single(result.Items);
        Assert.Equal(deckId, result.Items[0].Id);
        Assert.Equal(userId, result.Items[0].OwnerId);
        Assert.Equal(1, result.TotalItems);
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(8, result.Items[0].DueCards);

        repositoryMock.Verify(
            repo => repo.GetAccessibleDecksAsync(
                userId,
                1,
                10,
                true,
                It.IsAny<CancellationToken>(),
                null),
            Times.Once);
    }
}

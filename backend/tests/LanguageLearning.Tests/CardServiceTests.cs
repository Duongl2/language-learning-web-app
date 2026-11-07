using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Cards;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using Moq;
using Xunit;

namespace LanguageLearning.Tests;

public class CardServiceTests
{
    private static CardService CreateService(
        Mock<ICardRepository> cardRepositoryMock,
        Mock<IDeckRepository> deckRepositoryMock,
        Guid currentUserId)
    {
        var currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock
            .Setup(service => service.GetRequiredUserId())
            .Returns(currentUserId);

        return new CardService(
            cardRepositoryMock.Object,
            deckRepositoryMock.Object,
            currentUserMock.Object);
    }

    [Theory]
    [InlineData("", "Back content")]
    [InlineData("   ", "Back content")]
    [InlineData("Front content", "")]
    [InlineData("Front content", "   ")]
    public async Task CreateCard_EmptyFrontOrBack_ThrowsArgumentException(
        string front,
        string back)
    {
        var ownerId = Guid.NewGuid();
        var deckId = Guid.NewGuid();

        var deck = new Deck
        {
            Id = deckId,
            OwnerId = ownerId,
            Title = "Test Deck"
        };

        var cardRepositoryMock = new Mock<ICardRepository>();
        var deckRepositoryMock = new Mock<IDeckRepository>();

        deckRepositoryMock
            .Setup(repo => repo.GetByIdAsync(
                deckId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(deck);

        var service = CreateService(
            cardRepositoryMock,
            deckRepositoryMock,
            ownerId);

        var request = new CreateCardRequest
        {
            Front = front,
            Back = back
        };

        await Assert.ThrowsAsync<ArgumentException>(
            () => service.CreateCardAsync(deckId, request));

        cardRepositoryMock.Verify(
            repo => repo.AddAsync(
                It.IsAny<Card>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        cardRepositoryMock.Verify(
            repo => repo.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

[Fact]
public async Task CreateCard_ValidData_CreatesCardSuccessfully()
{
    var ownerId = Guid.NewGuid();
    var deckId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "English Vocabulary"
    };

    Card? savedCard = null;

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    cardRepositoryMock
        .Setup(repo => repo.AddAsync(
            It.IsAny<Card>(),
            It.IsAny<CancellationToken>()))
        .Callback<Card, CancellationToken>(
            (card, _) => savedCard = card)
        .Returns(Task.CompletedTask);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        ownerId);

    var request = new CreateCardRequest
    {
        Front = "  What is C#?  ",
        Back = "  A programming language.  "
    };

    var result = await service.CreateCardAsync(
        deckId,
        request);

    Assert.NotNull(savedCard);
    Assert.Equal(deckId, result.DeckId);
    Assert.Equal(deckId, savedCard.DeckId);
    Assert.Equal("What is C#?", result.Front);
    Assert.Equal("A programming language.", result.Back);
    Assert.Equal(1, result.Version);

    cardRepositoryMock.Verify(
        repo => repo.AddAsync(
            It.IsAny<Card>(),
            It.IsAny<CancellationToken>()),
        Times.Once);

    cardRepositoryMock.Verify(
        repo => repo.SaveChangesAsync(
            It.IsAny<CancellationToken>()),
        Times.Once);
}

[Theory]
[InlineData("", null)]
[InlineData("   ", null)]
[InlineData(null, "")]
[InlineData(null, "   ")]
public async Task UpdateCard_EmptyFrontOrBack_ThrowsArgumentException(
    string? front,
    string? back)
{
    var ownerId = Guid.NewGuid();
    var deckId = Guid.NewGuid();
    var cardId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "English Vocabulary"
    };

    var card = new Card
    {
        Id = cardId,
        DeckId = deckId,
        Front = "Old Front",
        Back = "Old Back",
        Version = 1
    };

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    cardRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            cardId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(card);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        ownerId);

    var request = new UpdateCardRequest
    {
    BaseVersion = 1,
    Front = front,
    Back = back
    };

    await Assert.ThrowsAsync<ArgumentException>(
        () => service.UpdateCardAsync(
            deckId,
            cardId,
            request));

    Assert.Equal("Old Front", card.Front);
    Assert.Equal("Old Back", card.Back);
    Assert.Equal(1, card.Version);

    cardRepositoryMock.Verify(
        repo => repo.SaveChangesAsync(
            It.IsAny<CancellationToken>()),
        Times.Never);
}

[Fact]
public async Task UpdateCard_ValidData_UpdatesSuccessfully()
{
    var ownerId = Guid.NewGuid();
    var deckId = Guid.NewGuid();
    var cardId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "English Vocabulary"
    };

    var card = new Card
    {
        Id = cardId,
        DeckId = deckId,
        Front = "Old Front",
        Back = "Old Back",
        Version = 1
    };

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    cardRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            cardId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(card);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        ownerId);

    var request = new UpdateCardRequest
    {
        BaseVersion = 1,
        Front = "  New Front  ",
        Back = "  New Back  "
    };

    var result = await service.UpdateCardAsync(
        deckId,
        cardId,
        request);

    Assert.Equal("New Front", result.Front);
    Assert.Equal("New Back", result.Back);
    Assert.Equal(deckId, result.DeckId);
    Assert.Equal(cardId, result.Id);
    Assert.Equal(2, result.Version);

    Assert.Equal("New Front", card.Front);
    Assert.Equal("New Back", card.Back);

    cardRepositoryMock.Verify(
        repo => repo.SaveChangesAsync(
            It.IsAny<CancellationToken>()),
        Times.Once);
}

[Fact]
public async Task DeleteCard_NonOwnerCannotDelete()
{
    var ownerId = Guid.NewGuid();
    var otherUserId = Guid.NewGuid();
    var deckId = Guid.NewGuid();
    var cardId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "Private Deck",
        IsPublic = false
    };

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        otherUserId);

    await Assert.ThrowsAsync<NotFoundException>(
        () => service.DeleteCardAsync(deckId, cardId));

    cardRepositoryMock.Verify(
        repo => repo.Delete(It.IsAny<Card>()),
        Times.Never);

    cardRepositoryMock.Verify(
        repo => repo.SaveChangesAsync(
            It.IsAny<CancellationToken>()),
        Times.Never);
}

[Fact]
public async Task DeleteCard_HasReviewEvents_ThrowsConflictException()
{
    var ownerId = Guid.NewGuid();
    var deckId = Guid.NewGuid();
    var cardId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "English Vocabulary"
    };

    var card = new Card
    {
        Id = cardId,
        DeckId = deckId,
        Front = "Hello",
        Back = "Xin chào"
    };

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    cardRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            cardId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(card);

    cardRepositoryMock
        .Setup(repo => repo.HasReviewEventsAsync(
            cardId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        ownerId);

    await Assert.ThrowsAsync<ConflictException>(
        () => service.DeleteCardAsync(deckId, cardId));

    cardRepositoryMock.Verify(
        repo => repo.Delete(It.IsAny<Card>()),
        Times.Never);

    cardRepositoryMock.Verify(
        repo => repo.SaveChangesAsync(
            It.IsAny<CancellationToken>()),
        Times.Never);
}

[Fact]
public async Task DeleteCard_OwnerCanDeleteSuccessfully()
{
    var ownerId = Guid.NewGuid();
    var deckId = Guid.NewGuid();
    var cardId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "English Vocabulary"
    };

    var card = new Card
    {
        Id = cardId,
        DeckId = deckId,
        Front = "What is C#?",
        Back = "A programming language."
    };

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    cardRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            cardId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(card);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        ownerId);

    await service.DeleteCardAsync(deckId, cardId);

    cardRepositoryMock.Verify(
        repo => repo.Delete(card),
        Times.Once);

    cardRepositoryMock.Verify(
        repo => repo.SaveChangesAsync(
            It.IsAny<CancellationToken>()),
        Times.Once);
}

[Fact]
public async Task UpdateCard_NonOwnerCannotUpdate()
{
    var ownerId = Guid.NewGuid();
    var otherUserId = Guid.NewGuid();
    var deckId = Guid.NewGuid();
    var cardId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "Private Deck",
        IsPublic = false
    };

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        otherUserId);

    var request = new UpdateCardRequest
    {
        Front = "Unauthorized Change"
    };

    await Assert.ThrowsAsync<NotFoundException>(
        () => service.UpdateCardAsync(
            deckId,
            cardId,
            request));

    cardRepositoryMock.Verify(
        repo => repo.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()),
        Times.Never);

    cardRepositoryMock.Verify(
        repo => repo.SaveChangesAsync(
            It.IsAny<CancellationToken>()),
        Times.Never);
}

[Fact]
public async Task UpdateCard_CardNotInDeck_ThrowsNotFoundException()
{
    var ownerId = Guid.NewGuid();
    var deckId = Guid.NewGuid();
    var cardId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "English Vocabulary"
    };

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    cardRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            cardId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync((Card?)null);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        ownerId);

    var request = new UpdateCardRequest
    {
        Front = "New Front"
    };

    await Assert.ThrowsAsync<NotFoundException>(
        () => service.UpdateCardAsync(
            deckId,
            cardId,
            request));

    cardRepositoryMock.Verify(
        repo => repo.SaveChangesAsync(
            It.IsAny<CancellationToken>()),
        Times.Never);
}

[Fact]
public async Task GetCards_ValidDeck_ReturnsCards()
{
    var ownerId = Guid.NewGuid();
    var deckId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "English Vocabulary"
    };

    var cards = new List<Card>
    {
        new Card
        {
            Id = Guid.NewGuid(),
            DeckId = deckId,
            Front = "Hello",
            Back = "Xin chào",
            Version = 1
        },
        new Card
        {
            Id = Guid.NewGuid(),
            DeckId = deckId,
            Front = "Goodbye",
            Back = "Tạm biệt",
            Version = 1
        }
    };

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    cardRepositoryMock
        .Setup(repo => repo.GetByDeckIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(cards);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        ownerId);

    var result = await service.GetCardsAsync(deckId);

    Assert.Equal(2, result.Count);
    Assert.All(result, card =>
        Assert.Equal(deckId, card.DeckId));

    Assert.Equal("Hello", result[0].Front);
    Assert.Equal("Xin chào", result[0].Back);
    Assert.Equal("Goodbye", result[1].Front);
    Assert.Equal("Tạm biệt", result[1].Back);

    cardRepositoryMock.Verify(
        repo => repo.GetByDeckIdAsync(
            deckId,
            It.IsAny<CancellationToken>()),
        Times.Once);
}

[Fact]
public async Task GetCardById_ValidCard_ReturnsCorrectCard()
{
    var ownerId = Guid.NewGuid();
    var deckId = Guid.NewGuid();
    var cardId = Guid.NewGuid();

    var deck = new Deck
    {
        Id = deckId,
        OwnerId = ownerId,
        Title = "English Vocabulary"
    };

    var card = new Card
    {
        Id = cardId,
        DeckId = deckId,
        Front = "What is C#?",
        Back = "A programming language.",
        Version = 1
    };

    var cardRepositoryMock = new Mock<ICardRepository>();
    var deckRepositoryMock = new Mock<IDeckRepository>();

    deckRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(deck);

    cardRepositoryMock
        .Setup(repo => repo.GetByIdAsync(
            deckId,
            cardId,
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(card);

    var service = CreateService(
        cardRepositoryMock,
        deckRepositoryMock,
        ownerId);

    var result = await service.GetCardByIdAsync(
        deckId,
        cardId);

    Assert.Equal(cardId, result.Id);
    Assert.Equal(deckId, result.DeckId);
    Assert.Equal("What is C#?", result.Front);
    Assert.Equal("A programming language.", result.Back);
    Assert.Equal(1, result.Version);

    cardRepositoryMock.Verify(
        repo => repo.GetByIdAsync(
            deckId,
            cardId,
            It.IsAny<CancellationToken>()),
        Times.Once);
}

}

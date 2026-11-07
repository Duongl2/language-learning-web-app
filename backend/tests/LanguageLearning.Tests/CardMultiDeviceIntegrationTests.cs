
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Cards;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using LanguageLearning.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using MySqlConnector;
using Xunit;

namespace LanguageLearning.Tests;

public sealed class CardMultiDeviceIntegrationTests
{
    [Fact]
    public async Task TwoDevices_UpdateSameCard_StaleUpdateRejected()
    {
        var connectionString =
            Environment.GetEnvironmentVariable("ISSUE50_TEST_CONNECTION")
            ?? throw new InvalidOperationException(
                "ISSUE50_TEST_CONNECTION is not configured.");

        var builder = new MySqlConnectionStringBuilder(
            connectionString);

        if (!new[]
        {
            "language_learning_test_db",
            "language_learning_issue50_test"
        }.Contains(
            builder.Database,
            StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A dedicated test database is required.");
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
        Guid deckId;
        Guid cardId;

        await using (var seed = new AppDbContext(options))
        {
            await seed.Database.EnsureCreatedAsync();

            var user = new User
            {
                Email = $"issue50-card-{Guid.NewGuid():N}@test.local"
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
                Title = "Multi Device Card Test",
                Version = 1
            };

            var card = new Card
            {
                Deck = deck,
                Front = "Original Front",
                Back = "Original Back",
                Version = 1
            };

            seed.Cards.Add(card);
            await seed.SaveChangesAsync();

            userId = user.Id;
            deckId = deck.Id;
            cardId = card.Id;
        }

        var currentUser = new Mock<ICurrentUserService>();

        currentUser
            .Setup(x => x.GetRequiredUserId())
            .Returns(userId);

        async Task<CardResponse> UpdateAsync(
            string front,
            int baseVersion)
        {
            await using var context = new AppDbContext(options);

            var service = new CardService(
                new CardRepository(context),
                new DeckRepository(context),
                currentUser.Object);

            return await service.UpdateCardAsync(
                deckId,
                cardId,
                new UpdateCardRequest
                {
                    Front = front,
                    BaseVersion = baseVersion
                });
        }

        var versionA = 1;
        var versionB = 1;

        var resultA = await UpdateAsync(
            "Updated by Device A",
            versionA);

        Assert.Equal(2, resultA.Version);

        await Assert.ThrowsAsync<ConflictException>(
            () => UpdateAsync(
                "Updated by Device B",
                versionB));

        await using var verify = new AppDbContext(options);

        var finalCard = await verify.Cards
            .AsNoTracking()
            .SingleAsync(c => c.Id == cardId);

        Assert.Equal("Updated by Device A", finalCard.Front);
        Assert.Equal(2, finalCard.Version);
    }
}

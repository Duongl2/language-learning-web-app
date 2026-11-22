using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LanguageLearning.Tests;

public sealed class BackupRestoreDrillTests
{
    [Fact]
    public async Task DatabaseBackupAndRestoreDrill_PreservesUsersDecksCardsAndReviewHistoryWithoutDataLoss()
    {
        await using var sourceConnection = new SqliteConnection("Data Source=:memory:");
        await sourceConnection.OpenAsync();
        await using var backupConnection = new SqliteConnection("Data Source=:memory:");
        await backupConnection.OpenAsync();

        var sourceOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(sourceConnection).Options;

        Guid userId;
        Guid deckId;
        Guid cardId;
        Guid eventId = Guid.NewGuid();

        await using (var sourceDb = new AppDbContext(sourceOptions))
        {
            await sourceDb.Database.EnsureCreatedAsync();

            var user = new User { Email = "drill@backup.local", DisplayName = "Backup Learner", Role = UserRole.Learner };
            var language = new Language { Code = "en", Name = "English", IsActive = true };
            var card = new Card { Front = "Backup", Back = "Sao lưu", Version = 2 };
            var deck = new Deck
            {
                Owner = user,
                Language = language,
                Title = "Production Drill Deck",
                IsPublic = true,
                Version = 3,
                Cards = new[] { card }
            };
            var reviewEvent = new ReviewEvent
            {
                User = user,
                Card = card,
                ClientEventId = eventId,
                Rating = SrsRating.Good,
                ReviewedAt = DateTime.UtcNow,
                AlgorithmVersion = "srs-v1"
            };

            sourceDb.AddRange(user, language, deck, reviewEvent);
            await sourceDb.SaveChangesAsync();

            userId = user.Id;
            deckId = deck.Id;
            cardId = card.Id;

            sourceConnection.BackupDatabase(backupConnection);

            await sourceDb.Database.EnsureDeletedAsync();
            await sourceDb.Database.EnsureCreatedAsync();
            Assert.Equal(0, await sourceDb.Decks.CountAsync());
        }

        await using var targetConnection = new SqliteConnection("Data Source=:memory:");
        await targetConnection.OpenAsync();
        backupConnection.BackupDatabase(targetConnection);

        var targetOptions = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(targetConnection).Options;
        await using var restoredDb = new AppDbContext(targetOptions);

        var restoredUser = await restoredDb.Users.AsNoTracking().SingleAsync(u => u.Id == userId);
        var restoredDeck = await restoredDb.Decks.AsNoTracking().SingleAsync(d => d.Id == deckId);
        var restoredCard = await restoredDb.Cards.AsNoTracking().SingleAsync(c => c.Id == cardId);
        var restoredEvent = await restoredDb.ReviewEvents.AsNoTracking().SingleAsync(e => e.ClientEventId == eventId);

        Assert.Equal("drill@backup.local", restoredUser.Email);
        Assert.Equal("Production Drill Deck", restoredDeck.Title);
        Assert.Equal(3, restoredDeck.Version);
        Assert.Equal("Backup", restoredCard.Front);
        Assert.Equal(2, restoredCard.Version);
        Assert.Equal(SrsRating.Good, restoredEvent.Rating);
    }
}
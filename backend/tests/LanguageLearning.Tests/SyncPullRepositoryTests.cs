
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

public class SyncPullRepositoryTests
{
    [Fact]
    public async Task PullAsync_SameTimestamp_NoMissingOrDuplicateDecks()
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ISSUE27_TEST_CONNECTION")
            ?? throw new InvalidOperationException(
                "ISSUE27_TEST_CONNECTION is not configured.");

        var connectionBuilder =
            new MySqlConnectionStringBuilder(connectionString);

        if (!string.Equals(
            connectionBuilder.Database,
            "language_learning_test_db",
            StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "A dedicated test database is required.");
        }

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(
                connectionString,
                new MySqlServerVersion(new Version(8, 0, 36)))
            .Options;

        await using var context = new AppDbContext(options);

        await context.Database.EnsureCreatedAsync();

        await using var transaction =
            await context.Database.BeginTransactionAsync();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = $"issue48-{Guid.NewGuid():N}@test.local"
        };

        var language = new Language
        {
            Id = Guid.NewGuid(),
            Code = $"t{Guid.NewGuid():N}"[..8],
            Name = "Test Language"
        };

        var time = DateTime.UtcNow.AddMinutes(1);

        var sameTimestamp = new DateTime(
            time.Ticks - time.Ticks % 10,
            DateTimeKind.Utc);

        var decks = Enumerable.Range(1, 5)
            .Select(number => new Deck
            {
                Id = Guid.NewGuid(),
                Owner = user,
                Language = language,
                Title = $"Same Timestamp Deck {number}",
                Description = "Cursor test",
                Version = 1,
                UpdatedAt = sameTimestamp
            })
            .ToList();

        context.Decks.AddRange(decks);

        await context.SaveChangesAsync();

        var currentUserMock = new Mock<ICurrentUserService>();

        currentUserMock
            .Setup(service => service.GetRequiredUserId())
            .Returns(user.Id);

        var service = new SyncPullService(
            new SyncPullRepository(context),
            currentUserMock.Object);

        var receivedIds = new List<Guid>();

        string? cursor = null;
        var reachedLastPage = false;

        for (var page = 0; page < 10; page++)
        {
            var response = await service.PullAsync(
                cursor,
                limit: 2);

            receivedIds.AddRange(
                response.Changes.Select(change => change.EntityId));

            cursor = response.NextCursor;

            if (!response.HasMore)
            {
                reachedLastPage = true;
                break;
            }
        }

        Assert.True(reachedLastPage);
        Assert.Equal(5, receivedIds.Count);
        Assert.Equal(5, receivedIds.Distinct().Count());

        var expectedIds = decks
            .Select(deck => deck.Id)
            .OrderBy(
                id => id.ToString("D"),
                StringComparer.Ordinal)
            .ToList();

        Assert.Equal(expectedIds, receivedIds);

        await transaction.RollbackAsync();
    }
}

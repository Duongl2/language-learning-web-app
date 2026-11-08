using System.Text.Json;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using LanguageLearning.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Moq;
using MySqlConnector;

namespace LanguageLearning.Tests;

public sealed class LearningMySqlTests
{
    [Fact]
    public async Task LearningFlow_PersistsAllQuestionTypesProgressCsvAndCloneOnMySql()
    {
        var connection = Environment.GetEnvironmentVariable("ISSUE27_TEST_CONNECTION")
            ?? throw new InvalidOperationException("ISSUE27_TEST_CONNECTION must point to a migrated dedicated test database.");
        if (new MySqlConnectionStringBuilder(connection).Database != "language_learning_test_db")
            throw new InvalidOperationException("A dedicated language_learning_test_db is required.");
        await using var db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connection, new MySqlServerVersion(new Version(8, 0, 36))).Options);
        await using var transaction = await db.Database.BeginTransactionAsync();
        var user = new User { Email = $"learning-{Guid.NewGuid():N}@example.com" };
        var language = new Language { Code = "en", Name = "English" };
        var course = new Course { Title = "Learning integration", Language = language };
        var unit = new Unit { Course = course, Title = "Unit" };
        var lesson = new Lesson { Unit = unit, Title = "Greetings" };
        var multiple = new Question { Lesson = lesson, Prompt = "Greeting?", Options = new[] { new QuestionOption { Content = "Hello", IsCorrect = true } } };
        var fill = new Question { Lesson = lesson, Type = QuestionType.FillInBlank, Prompt = "Say hello", ConfigJson = "{\"answers\":[\"hello\"]}" };
        var matching = new Question { Lesson = lesson, Type = QuestionType.Matching, Prompt = "Match", ConfigJson = "{\"left\":[{\"id\":\"a\",\"text\":\"Hello\"}],\"right\":[{\"id\":\"b\",\"text\":\"Xin chào\"}],\"pairs\":{\"a\":\"b\"}}" };
        var deck = new Deck { Owner = user, Language = language, Title = "CSV source", IsPublic = true };
        db.AddRange(user, language, course, unit, lesson, multiple, fill, matching, deck);
        await db.SaveChangesAsync();
        db.ChangeTracker.Clear();
        var identity = new Mock<ICurrentUserService>();
        identity.Setup(x => x.GetRequiredUserId()).Returns(user.Id);
        var repository = new QuizRepository(db);
        var quiz = new QuizService(repository, identity.Object);
        var request = new QuizSubmission
        {
            AttemptId = Guid.NewGuid(),
            Answers = new()
        {
            new(multiple.Id, JsonSerializer.SerializeToElement(multiple.Options.Single().Id)),
            new(fill.Id, JsonSerializer.SerializeToElement("HELLO")),
            new(matching.Id, JsonSerializer.SerializeToElement(new Dictionary<string, string> { ["a"] = "b" }))
        }
        };
        Assert.Equal(3, (await quiz.SubmitAsync(lesson.Id, request, default)).Score);
        Assert.Equal(3, (await quiz.SubmitAsync(lesson.Id, request, default)).Score);
        Assert.Equal(1, await db.QuizAttempts.CountAsync(x => x.UserId == user.Id));
        var progress = new ProgressService(new ProgressRepository(db), identity.Object);
        await progress.UpdateAsync(lesson.Id, "Completed", default);
        await progress.UpdateAsync(lesson.Id, "InProgress", default);
        Assert.Equal(100, (await progress.GetAsync(default)).Courses.Single().Percent);
        var csv = new CsvService(new DeckRepository(db), new CardRepository(db), identity.Object);
        Assert.Equal(2, await csv.ImportAsync(deck.Id, "front,back\nHello,Xin chào\n\"Comma, inside\",\"=1+1\"", default));
        Assert.Contains("\"'=1+1\"", await csv.ExportAsync(deck.Id, default));
        var sharing = new SharingReadRepository(db);
        var sharingWrite = new SharingWriteRepository(db);
        var cloneId = await sharingWrite.CloneAsync(deck.Id, user.Id, default);
        Assert.Equal(2, await db.Cards.CountAsync(x => x.DeckId == cloneId));
        Assert.Null(await sharing.PreviewAsync(cloneId, default));
        var statistics = new StatisticsService(new StatisticsRepository(db), identity.Object, TimeProvider.System);
        Assert.Equal(4, (await statistics.OverviewAsync(null, null, default)).NewCards);
        await transaction.RollbackAsync();
    }
}

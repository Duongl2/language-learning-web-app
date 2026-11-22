using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using LanguageLearning.Infrastructure.Repositories;
using LanguageLearning.Infrastructure.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace LanguageLearning.Tests;

public sealed class LearningIntegrationTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly AppDbContext _db;
    private readonly User _owner = new() { Email = "owner@example.com" };
    private readonly User _other = new() { Email = "other@example.com" };
    private readonly Language _language = new() { Name = "English", Code = "en" };
    private readonly Course _course;
    private readonly Lesson _lesson;
    private readonly Deck _deck;
    private readonly Question _question;
    private readonly QuizRepository _repository;
    private readonly ProgressRepository _progressRepository;
    private readonly StatisticsRepository _statisticsRepository;
    private readonly StreakRepository _streakRepository;

    public LearningIntegrationTests()
    {
        _connection.Open();
        _db = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        _db.Database.EnsureCreated();
        _course = new Course { Language = _language, Title = "A1" };
        var unit = new Unit { Course = _course, Title = "Unit" };
        _lesson = new Lesson { Unit = unit, Title = "Lesson" };
        _question = new Question
        {
            Lesson = _lesson,
            Prompt = "Hello?",
            Explanation = "A greeting.",
            Options = new[] { new QuestionOption { Content = "Xin chào", IsCorrect = true }, new QuestionOption { Content = "Tạm biệt" } }
        };
        _deck = new Deck
        {
            Owner = _owner,
            Language = _language,
            Title = "Source",
            IsPublic = true,
            Cards = new[] { new Card { Front = "Hello", Back = "Xin chào" } },
            Tags = new[] { new DeckTag { Tag = "A1" } }
        };
        _db.AddRange(_owner, _other, _course, unit, _lesson, _question, _deck);
        _db.SaveChanges();
        _db.ChangeTracker.Clear();
        _repository = new QuizRepository(_db);
        _progressRepository = new ProgressRepository(_db);
        _statisticsRepository = new StatisticsRepository(_db);
        _streakRepository = new StreakRepository(_db);
    }

    private ICurrentUserService Identity(Guid id)
    {
        var current = new Mock<ICurrentUserService>();
        current.Setup(x => x.GetRequiredUserId()).Returns(id);
        return current.Object;
    }

    [Fact]
    public async Task Quiz_HidesAnswers_GradesPersistsAndReplaysWithoutDuplicates()
    {
        var quiz = new QuizService(_repository, Identity(_owner.Id));
        var questions = await new QuizReadService(new QuizReadRepository(_db)).GetAsync(_lesson.Id, default);
        var json = JsonSerializer.Serialize(questions);
        Assert.DoesNotContain("IsCorrect", json);
        Assert.DoesNotContain("A greeting", json);
        Assert.DoesNotContain("ConfigJson", json);
        var submission = new QuizSubmission
        {
            AttemptId = Guid.NewGuid(),
            Answers = new()
        {
            new(_question.Id, JsonSerializer.SerializeToElement(_question.Options.First(x => x.IsCorrect).Id))
        }
        };
        var result = await quiz.SubmitAsync(_lesson.Id, submission, default);
        Assert.Equal(1, result.Score);
        Assert.Equal("Xin chào", result.Answers.Single().CorrectAnswer);
        Assert.Equal(result.Id, (await quiz.SubmitAsync(_lesson.Id, submission, default)).Id);
        Assert.Equal(1, await _db.QuizAttempts.CountAsync());
        Assert.Equal(1, await _db.QuizAnswers.CountAsync());
        Assert.Equal(1, await _db.Questions.CountAsync());
        Assert.Single(await quiz.HistoryAsync(_lesson.Id, default));
        submission.Answers[0] = new(_question.Id, JsonSerializer.SerializeToElement(Guid.NewGuid()));
        await Assert.ThrowsAsync<ConflictException>(() => quiz.SubmitAsync(_lesson.Id, submission, default));
        Assert.Empty(await new QuizService(_repository, Identity(_other.Id)).HistoryAsync(_lesson.Id, default));
    }

    [Fact]
    public async Task Quiz_RejectsForeignOrMissingAnswersAndUnpublishedLessons()
    {
        var quiz = new QuizService(_repository, Identity(_owner.Id));
        await Assert.ThrowsAsync<ArgumentException>(() => quiz.SubmitAsync(_lesson.Id,
            new QuizSubmission { AttemptId = Guid.NewGuid() }, default));
        var lesson = await _db.Lessons.FindAsync(_lesson.Id);
        lesson!.Status = ContentStatus.Draft;
        await _db.SaveChangesAsync();
        await Assert.ThrowsAsync<NotFoundException>(() => new QuizReadService(new QuizReadRepository(_db)).GetAsync(_lesson.Id, default));
        Assert.Equal(0, await _db.QuizAttempts.CountAsync());
    }

    [Fact]
    public async Task Progress_RetryDoesNotDuplicateCompletionOrEnrollment_AndResumeIsUserScoped()
    {
        var service = new ProgressService(_progressRepository, Identity(_owner.Id));
        await service.UpdateAsync(_lesson.Id, "InProgress", default);
        Assert.Equal(_lesson.Id, (await service.GetAsync(default)).Resume!.LessonId);
        await service.UpdateAsync(_lesson.Id, "Completed", default);
        var completedAt = (await _db.LessonProgresses.SingleAsync()).CompletedAt;
        await service.UpdateAsync(_lesson.Id, "Completed", default);
        await service.UpdateAsync(_lesson.Id, "InProgress", default);
        await service.EnrollAsync(_course.Id, default);
        Assert.Equal(1, await _db.CourseEnrollments.CountAsync());
        Assert.Equal(1, await _db.LessonProgresses.CountAsync());
        Assert.Equal(completedAt, (await _db.LessonProgresses.SingleAsync()).CompletedAt);
        var response = await service.GetAsync(default);
        Assert.Null(response.Resume);
        Assert.Equal(100, response.Courses.Single().Percent);
        Assert.Empty((await new ProgressService(_progressRepository, Identity(_other.Id)).GetAsync(default)).Courses);
        await Assert.ThrowsAsync<ArgumentException>(() => service.UpdateAsync(_lesson.Id, "Arbitrary", default));
    }

    [Fact]
    public async Task Progress_ConcurrentStaleStatusCannotOverwriteCompletion()
    {
        await _progressRepository.SaveProgressAsync(_owner.Id, _lesson.Id, "InProgress", default);
        await _progressRepository.SaveChangesAsync(default);
        _db.ChangeTracker.Clear();
        await using var second = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options);
        var stale = new ProgressRepository(second);
        await stale.SaveProgressAsync(_owner.Id, _lesson.Id, "NotStarted", default);
        await _progressRepository.SaveProgressAsync(_owner.Id, _lesson.Id, "Completed", default);
        await _progressRepository.SaveChangesAsync(default);
        await Assert.ThrowsAsync<ConflictException>(() => stale.SaveChangesAsync(default));
        Assert.Equal("Completed", (await _db.LessonProgresses.AsNoTracking().SingleAsync()).Status);
    }

    [Fact]
    public async Task Sharing_PrivateDecksAreHidden_ClonesAreIndependent_AndModerationIsAudited()
    {
        var sharing = new SharingReadRepository(_db);
        var sharingWrite = new SharingWriteRepository(_db);
        var initial = await sharing.ListAsync("Source", _language.Id, 1, 10, default);
        Assert.Equal(1, initial.Total);
        var cloneId = await sharingWrite.CloneAsync(_deck.Id, _other.Id, default);
        var clone = await _db.Decks.Include(x => x.Cards).Include(x => x.Tags).SingleAsync(x => x.Id == cloneId);
        Assert.Equal(_other.Id, clone.OwnerId);
        Assert.Equal(_deck.Id, clone.SourceDeckId);
        Assert.False(clone.IsPublic);
        Assert.NotEqual(_deck.Cards.Single().Id, clone.Cards.Single().Id);
        clone.Cards.Single().Front = "Changed";
        await _db.SaveChangesAsync();
        Assert.Equal("Hello", (await sharing.PreviewAsync(_deck.Id, default))!.Cards.Single().Front);
        Assert.Null(await sharing.PreviewAsync(cloneId, default));
        await Assert.ThrowsAsync<NotFoundException>(() => sharingWrite.CloneAsync(cloneId, _owner.Id, default));
        await sharingWrite.HideAsync(_deck.Id, _owner.Id, default);
        Assert.Null(await sharing.PreviewAsync(_deck.Id, default));
        Assert.Equal(0, (await sharing.ListAsync(null, null, 1, 10, default)).Total);
        Assert.Equal("HidePublicDeck", (await _db.AuditLogs.SingleAsync()).Action);
    }

    [Fact]
    public async Task Csv_InvalidBatchLeavesDeckUnchanged_AndOnlyOwnerCanImportOrExport()
    {
        var service = new CsvService(new DeckRepository(_db), new CardRepository(_db), Identity(_owner.Id));
        await Assert.ThrowsAsync<ArgumentException>(() => service.ImportAsync(_deck.Id, "front,back\nvalid,ok\n,invalid", default));
        Assert.Equal(1, await _db.Cards.CountAsync());
        Assert.Equal(1, (await _db.Decks.FindAsync(_deck.Id))!.Version);
        Assert.Equal(2, await service.ImportAsync(_deck.Id, "front,back\nnew,word\n=SUM(A1),formula", default));
        Assert.Equal(3, await _db.Cards.CountAsync());
        Assert.Contains("\"'=SUM(A1)\"", await service.ExportAsync(_deck.Id, default));
        var foreign = new CsvService(new DeckRepository(_db), new CardRepository(_db), Identity(_other.Id));
        await Assert.ThrowsAsync<NotFoundException>(() => foreign.ImportAsync(_deck.Id, "front,back\na,b", default));
        await Assert.ThrowsAsync<NotFoundException>(() => foreign.ExportAsync(_deck.Id, default));
    }

    [Fact]
    public async Task Statistics_GoalsAndReviewsAreScopedToCurrentUser()
    {
        var service = new StreakService(_streakRepository, Identity(_owner.Id), TimeProvider.System);
        await service.SetGoalAsync(5, default);
        await service.SetGoalAsync(7, default);
        Assert.Equal(1, await _db.UserGoals.CountAsync());
        Assert.Equal(7, (await service.StreakAsync(default)).DailyTarget);
        Assert.Equal(10, (await new StreakService(_streakRepository, Identity(_other.Id), TimeProvider.System).StreakAsync(default)).DailyTarget);
        Assert.Equal(1, (await new StatisticsService(_statisticsRepository, Identity(_owner.Id), TimeProvider.System).OverviewAsync(null, null, default)).NewCards);
        await Assert.ThrowsAsync<ArgumentException>(() => service.SetGoalAsync(0, default));
        await Assert.ThrowsAsync<ArgumentException>(() => new StatisticsService(_statisticsRepository, Identity(_owner.Id), TimeProvider.System).OverviewAsync(new DateOnly(2026, 2, 1), new DateOnly(2026, 1, 1), default));
    }

    [Fact]
    public async Task Api_AuthenticatesRequests_RejectsLearnerModeration_AndReturnsTypedQuiz()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("Jwt:SecretKey", "learning-test-secret-key-at-least-32-characters");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                foreach (var descriptor in services.Where(x => x.ServiceType.Name.Contains("DbContextOptionsConfiguration")).ToList()) services.Remove(descriptor);
                services.AddDbContext<AppDbContext>(options => options.UseSqlite(_connection));
            });
        });
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync($"/api/v1/lessons/{_lesson.Id}/quiz")).StatusCode);
        var tokens = factory.Services.GetRequiredService<ITokenService>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.CreateAccessToken(_owner));
        Assert.Equal(HttpStatusCode.Forbidden, (await client.PostAsync($"/api/v1/admin/decks/{_deck.Id}/hide", null)).StatusCode);
        var quiz = await client.GetAsync($"/api/v1/lessons/{_lesson.Id}/quiz");
        Assert.Equal(HttpStatusCode.OK, quiz.StatusCode);
        var json = await quiz.Content.ReadAsStringAsync();
        Assert.DoesNotContain("isCorrect", json);
        Assert.DoesNotContain("correctAnswer", json);
        var page = await client.GetFromJsonAsync<JsonElement>("/api/v1/public/decks?page=1&pageSize=10");
        Assert.Equal(1, page.GetProperty("totalItems").GetInt32());
        Assert.Equal(1, page.GetProperty("page").GetInt32());
        _owner.Role = UserRole.Admin;
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.CreateAccessToken(_owner));
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync($"/api/v1/admin/decks/{_deck.Id}/hide", null)).StatusCode);
    }

    public void Dispose() { _db.Dispose(); _connection.Dispose(); }
}

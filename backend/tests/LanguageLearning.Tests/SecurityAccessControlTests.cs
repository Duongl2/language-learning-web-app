using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using LanguageLearning.Application.DTOs.Auth;
using LanguageLearning.Application.DTOs.Cards;
using LanguageLearning.Application.DTOs.Decks;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xunit;

namespace LanguageLearning.Tests;

public sealed class SecurityAccessControlTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly AppDbContext _db;
    private readonly WebApplicationFactory<Program> _factory;
    private readonly User _owner;
    private readonly User _otherLearner;
    private readonly User _admin;
    private readonly Language _language;
    private readonly Course _course;
    private readonly Unit _unit;
    private readonly Lesson _lesson;
    private readonly Deck _privateDeck;
    private readonly Deck _publicDeck;
    private readonly Card _ownerCard;

    public SecurityAccessControlTests()
    {
        _connection.Open();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();

        _owner = new User
        {
            Email = "owner@security.local",
            PasswordHash = "PBKDF2$SUPER_SECRET_HASH_VALUE_NEVER_LEAK",
            DisplayName = "Deck Owner",
            Role = UserRole.Learner
        };

        _otherLearner = new User
        {
            Email = "intruder@security.local",
            PasswordHash = "PBKDF2$INTRUDER_SECRET_HASH_VALUE",
            DisplayName = "Other Learner",
            Role = UserRole.Learner
        };

        _admin = new User
        {
            Email = "admin@security.local",
            PasswordHash = "PBKDF2$ADMIN_SECRET_HASH_VALUE",
            DisplayName = "System Admin",
            Role = UserRole.Admin
        };

        _language = new Language { Code = "en", Name = "English", IsActive = true };
        _course = new Course { Language = _language, Title = "English Security A1", Level = "A1", Status = ContentStatus.Published };
        _unit = new Unit { Course = _course, Title = "Unit 1", SortOrder = 1 };
        _lesson = new Lesson { Unit = _unit, Title = "Lesson 1", Content = "Security Basics", SortOrder = 1, Status = ContentStatus.Published };

        var question = new Question
        {
            Lesson = _lesson,
            Prompt = "What is Least Privilege?",
            Explanation = "Hidden explanation before quiz submission",
            Options = new[]
            {
                new QuestionOption { Content = "Grant only required access", IsCorrect = true },
                new QuestionOption { Content = "Grant admin to all users", IsCorrect = false }
            }
        };

        _ownerCard = new Card { Front = "Private Front", Back = "Private Back", Version = 1 };
        _privateDeck = new Deck
        {
            Owner = _owner,
            Language = _language,
            Title = "Owner Private Deck",
            Description = "Confidential study notes",
            IsPublic = false,
            Version = 1,
            Cards = new[] { _ownerCard }
        };

        _publicDeck = new Deck
        {
            Owner = _owner,
            Language = _language,
            Title = "Owner Public Deck",
            Description = "Shared community deck",
            IsPublic = true,
            Version = 1,
            Cards = new[] { new Card { Front = "Public Front", Back = "Public Back", Version = 1 } }
        };

        _db.AddRange(_owner, _otherLearner, _admin, _language, _course, _unit, _lesson, question, _privateDeck, _publicDeck);
        _db.SaveChanges();
        _db.ChangeTracker.Clear();

        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("Jwt:SecretKey", "security-test-secret-key-at-least-32-chars-long");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<AppDbContext>();
                services.RemoveAll<DbContextOptions<AppDbContext>>();
                foreach (var descriptor in services.Where(d => d.ServiceType.Name.Contains("DbContextOptionsConfiguration")).ToList())
                {
                    services.Remove(descriptor);
                }

                services.AddDbContext<AppDbContext>(dbOptions => dbOptions.UseSqlite(_connection));
            });
        });
    }

    private HttpClient CreateAuthenticatedClient(User user)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
        var tokenService = _factory.Services.GetRequiredService<ITokenService>();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokenService.CreateAccessToken(user));
        return client;
    }

    [Fact]
    public async Task AdminEndpoints_RejectAnonymousWith401_AndRejectLearnerWith403()
    {
        using var anonymousClient = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        using var learnerClient = CreateAuthenticatedClient(_otherLearner);
        using var adminClient = CreateAuthenticatedClient(_admin);

        var adminGetRoutes = new[]
        {
            "/api/v1/admin/access",
            "/api/v1/admin/languages",
            "/api/v1/admin/courses",
            "/api/v1/admin/units",
            "/api/v1/admin/lessons"
        };

        foreach (var route in adminGetRoutes)
        {
            var anonResponse = await anonymousClient.GetAsync(route);
            Assert.Equal(HttpStatusCode.Unauthorized, anonResponse.StatusCode);

            var learnerResponse = await learnerClient.GetAsync(route);
            Assert.Equal(HttpStatusCode.Forbidden, learnerResponse.StatusCode);
        }

        var learnerModerateResponse = await learnerClient.PostAsync($"/api/v1/admin/decks/{_publicDeck.Id}/hide", null);
        Assert.Equal(HttpStatusCode.Forbidden, learnerModerateResponse.StatusCode);

        var adminAccessResponse = await adminClient.GetAsync("/api/v1/admin/access");
        Assert.Equal(HttpStatusCode.NoContent, adminAccessResponse.StatusCode);
    }

    [Fact]
    public async Task ResourceOwnership_LearnerCannotReadModifyOrDeleteAnotherUsersPrivateDeckOrCards()
    {
        using var intruderClient = CreateAuthenticatedClient(_otherLearner);

        var getPrivateDeck = await intruderClient.GetAsync($"/api/v1/decks/{_privateDeck.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getPrivateDeck.StatusCode);

        var updatePrivateDeck = await intruderClient.PatchAsJsonAsync(
            $"/api/v1/decks/{_privateDeck.Id}",
            new UpdateDeckRequest { BaseVersion = 1, Title = "Hacked Title" });
        Assert.Equal(HttpStatusCode.NotFound, updatePrivateDeck.StatusCode);

        var deletePrivateDeck = await intruderClient.DeleteAsync($"/api/v1/decks/{_privateDeck.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deletePrivateDeck.StatusCode);

        var updatePublicDeckNotOwned = await intruderClient.PatchAsJsonAsync(
            $"/api/v1/decks/{_publicDeck.Id}",
            new UpdateDeckRequest { BaseVersion = 1, Title = "Hijacked Public Deck" });
        Assert.Equal(HttpStatusCode.NotFound, updatePublicDeckNotOwned.StatusCode);

        var createCardInForeignDeck = await intruderClient.PostAsJsonAsync(
            $"/api/v1/decks/{_publicDeck.Id}/cards",
            new CreateCardRequest { Front = "Injected", Back = "Card" });
        Assert.Equal(HttpStatusCode.NotFound, createCardInForeignDeck.StatusCode);

        var updateForeignCard = await intruderClient.PatchAsJsonAsync(
            $"/api/v1/decks/{_privateDeck.Id}/cards/{_ownerCard.Id}",
            new UpdateCardRequest { BaseVersion = 1, Front = "Tampered Front" });
        Assert.Equal(HttpStatusCode.NotFound, updateForeignCard.StatusCode);

        var deleteForeignCard = await intruderClient.DeleteAsync(
            $"/api/v1/decks/{_privateDeck.Id}/cards/{_ownerCard.Id}");
        Assert.Equal(HttpStatusCode.NotFound, deleteForeignCard.StatusCode);

        var clonePrivateDeck = await intruderClient.PostAsync($"/api/v1/public/decks/{_privateDeck.Id}/clone", null);
        Assert.Equal(HttpStatusCode.NotFound, clonePrivateDeck.StatusCode);

        var unchangedDeck = await _db.Decks.AsNoTracking().SingleAsync(d => d.Id == _privateDeck.Id);
        Assert.Equal("Owner Private Deck", unchangedDeck.Title);
    }

    [Fact]
    public async Task ApiResponses_NeverLeakPasswordHashTokenHashOrQuizAnswers()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var registerResponse = await client.PostAsJsonAsync("/api/v1/auth/register", new RegisterRequest
        {
            Email = "newlearner@security.local",
            Password = "SafePassword123!",
            DisplayName = "Safe Learner"
        });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        var registerBody = await registerResponse.Content.ReadAsStringAsync();

        Assert.DoesNotContain("SafePassword123!", registerBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("passwordHash", registerBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tokenHash", registerBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("PBKDF2", registerBody, StringComparison.OrdinalIgnoreCase);

        var auth = JsonSerializer.Deserialize<AuthResponse>(registerBody, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.NotNull(auth);

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.AccessToken);

        var profileResponse = await client.GetAsync("/api/v1/me");
        Assert.Equal(HttpStatusCode.OK, profileResponse.StatusCode);
        var profileBody = await profileResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("password", profileBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("tokenHash", profileBody, StringComparison.OrdinalIgnoreCase);

        var quizResponse = await client.GetAsync($"/api/v1/lessons/{_lesson.Id}/quiz");
        Assert.Equal(HttpStatusCode.OK, quizResponse.StatusCode);
        var quizBody = await quizResponse.Content.ReadAsStringAsync();
        Assert.DoesNotContain("isCorrect", quizBody, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Hidden explanation", quizBody, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        _factory.Dispose();
        _db.Dispose();
        _connection.Dispose();
    }
}
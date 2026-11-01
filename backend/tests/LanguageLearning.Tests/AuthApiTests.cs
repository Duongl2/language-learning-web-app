using System.Net;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LanguageLearning.Application.DTOs.Auth;
using LanguageLearning.Application.DTOs.Decks;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace LanguageLearning.Tests;

public class AuthApiTests
{
    [Fact]
    public async Task Register_DuplicateEmail_ReturnsBadRequestProblemDetails()
    {
        var repository = new InMemoryUserRepository();
        using var factory = CreateFactory(repository);
        using var client = factory.CreateClient();
        var request = new RegisterRequest
        {
            Email = "learner@example.com",
            Password = "secure-password",
            DisplayName = "Learner"
        };

        var firstResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            request);
        var duplicateResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            request);

        Assert.Equal(HttpStatusCode.Created, firstResponse.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, duplicateResponse.StatusCode);
        Assert.StartsWith("application/problem+json",
            duplicateResponse.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsJwtAndRefreshToken()
    {
        var repository = new InMemoryUserRepository();
        using var factory = CreateFactory(repository);
        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest
            {
                Email = "learner@example.com",
                Password = "secure-password"
            });
        var loginResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new LoginRequest
            {
                Email = "learner@example.com",
                Password = "secure-password"
            });
        var response = await loginResponse.Content.ReadFromJsonAsync<AuthResponse>();

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        Assert.NotNull(response);
        Assert.Equal(3, response.AccessToken.Split('.').Length);
        Assert.False(string.IsNullOrWhiteSpace(response.RefreshToken));
        Assert.NotEqual(response.RefreshToken,
            repository.GetUser("learner@example.com")!
                .RefreshTokens.Last().TokenHash);
    }

    [Fact]
    public async Task RefreshThenLogout_ManagesRefreshTokenLifecycle()
    {
        var repository = new InMemoryUserRepository();
        using var factory = CreateFactory(repository);
        using var client = factory.CreateClient();

        var registerResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/register",
            new RegisterRequest
            {
                Email = "learner@example.com",
                Password = "secure-password"
            });
        var registered = await registerResponse.Content
            .ReadFromJsonAsync<AuthResponse>();

        var refreshResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshTokenRequest
            {
                RefreshToken = registered!.RefreshToken
            });
        var refreshed = await refreshResponse.Content
            .ReadFromJsonAsync<AuthResponse>();

        var logoutResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/logout",
            new RefreshTokenRequest
            {
                RefreshToken = refreshed!.RefreshToken
            });
        var revokedRefreshResponse = await client.PostAsJsonAsync(
            "/api/v1/auth/refresh",
            new RefreshTokenRequest
            {
                RefreshToken = refreshed.RefreshToken
            });

        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, refreshResponse.StatusCode);
        Assert.NotNull(refreshed);
        Assert.NotEqual(registered.RefreshToken, refreshed.RefreshToken);
        Assert.Equal(HttpStatusCode.NoContent, logoutResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized,
            revokedRefreshResponse.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_UnknownRole_ReturnsForbidden()
    {
        var repository = new InMemoryUserRepository();
        var languageService = new Mock<ILanguageService>();
        languageService
            .Setup(service => service.GetActiveLanguagesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LanguageOptionResponse>());
        using var factory = CreateFactory(repository, languageService.Object);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                CreateAccessToken("Guest"));

        var response = await client.GetAsync("/api/v1/languages");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Theory]
    [InlineData("Learner")]
    [InlineData("Admin")]
    public async Task ProtectedEndpoint_AllowedRole_ReturnsSuccess(string role)
    {
        var repository = new InMemoryUserRepository();
        var languageService = new Mock<ILanguageService>();
        languageService
            .Setup(service => service.GetActiveLanguagesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LanguageOptionResponse>());
        using var factory = CreateFactory(repository, languageService.Object);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                CreateAccessToken(role));

        var response = await client.GetAsync("/api/v1/languages");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Theory]
    [InlineData("Learner", HttpStatusCode.Forbidden)]
    [InlineData("Admin", HttpStatusCode.NoContent)]
    public async Task AdminEndpoint_RequiresAdminRole(
        string role,
        HttpStatusCode expectedStatusCode)
    {
        var repository = new InMemoryUserRepository();
        using var factory = CreateFactory(repository);
        using var client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new System.Net.Http.Headers.AuthenticationHeaderValue(
                "Bearer",
                CreateAccessToken(role));

        var response = await client.GetAsync("/api/v1/admin/access");

        Assert.Equal(expectedStatusCode, response.StatusCode);
    }

    private static WebApplicationFactory<Program> CreateFactory(
        InMemoryUserRepository repository,
        ILanguageService? languageService = null)
    {
        return new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Development");
                builder.UseSetting(
                    "Jwt:SecretKey",
                    "test-secret-key-that-is-long-enough-for-hmac-sha256");
                builder.ConfigureTestServices(services =>
                {
                    services.RemoveAll<IUserRepository>();
                    services.AddSingleton<IUserRepository>(repository);

                    if (languageService != null)
                    {
                        services.RemoveAll<ILanguageService>();
                        services.AddSingleton(languageService);
                    }
                });
            });
    }

    private sealed class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<string, User> _users =
            new(StringComparer.OrdinalIgnoreCase);
        private User? _pendingUser;

        public Task<User?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_users.Values.FirstOrDefault(user => user.Id == id));
        }

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            _users.TryGetValue(email, out var user);

            return Task.FromResult(user);
        }

        public Task<RefreshToken?> GetRefreshTokenByHashAsync(
            string tokenHash,
            CancellationToken cancellationToken = default)
        {
            var refreshToken = _users.Values
                .SelectMany(user => user.RefreshTokens)
                .FirstOrDefault(token => token.TokenHash == tokenHash);

            return Task.FromResult(refreshToken);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            _pendingUser = user;

            return Task.CompletedTask;
        }

        public Task AddRefreshTokenAsync(
            RefreshToken refreshToken,
            CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            if (_pendingUser != null)
            {
                _users.Add(_pendingUser.Email, _pendingUser);
                _pendingUser = null;
            }

            return Task.CompletedTask;
        }

        public User? GetUser(string email)
        {
            _users.TryGetValue(email, out var user);

            return user;
        }
    }

    private static string CreateAccessToken(string role)
    {
        var header = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            alg = "HS256",
            typ = "JWT"
        }));
        var payload = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(new
        {
            sub = Guid.NewGuid().ToString(),
            email = "learner@example.com",
            role,
            iss = "LanguageLearningApp",
            aud = "LanguageLearningAppClient",
            iat = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            exp = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()
        }));
        var unsignedToken = $"{header}.{payload}";
        var signature = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(
                "test-secret-key-that-is-long-enough-for-hmac-sha256"),
            Encoding.UTF8.GetBytes(unsignedToken));

        return $"{unsignedToken}.{Base64UrlEncode(signature)}";
    }

    private static string Base64UrlEncode(byte[] value)
    {
        return Convert.ToBase64String(value)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}

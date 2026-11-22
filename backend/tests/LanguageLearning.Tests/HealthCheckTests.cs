using System.Net;
using System.Net.Http.Json;
using LanguageLearning.Application.DTOs.Decks;
using LanguageLearning.Application.DTOs.Health;
using LanguageLearning.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using Xunit;

namespace LanguageLearning.Tests;

public sealed class HealthCheckTests
{
    [Theory]
    [InlineData("/health")]
    [InlineData("/api/v1/health")]
    public async Task GetHealth_DatabaseReachable_Returns200Healthy(string route)
    {
        var languageService = new Mock<ILanguageService>();
        languageService
            .Setup(s => s.GetActiveLanguagesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<LanguageOptionResponse>());

        using var factory = CreateFactory(languageService.Object);
        using var client = factory.CreateClient();

        var response = await client.GetAsync(route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<HealthStatusResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Healthy", payload!.Status);
        Assert.Equal("Healthy", payload.Api);
        Assert.Equal("Healthy", payload.Database);
    }

    [Fact]
    public async Task GetHealth_DatabaseUnreachable_Returns503Unhealthy()
    {
        var languageService = new Mock<ILanguageService>();
        languageService
            .Setup(s => s.GetActiveLanguagesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed."));

        using var factory = CreateFactory(languageService.Object);
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.ServiceUnavailable, response.StatusCode);
        var payload = await response.Content.ReadFromJsonAsync<HealthStatusResponse>();
        Assert.NotNull(payload);
        Assert.Equal("Unhealthy", payload!.Status);
        Assert.Equal("Healthy", payload.Api);
        Assert.Equal("Unhealthy", payload.Database);
    }

    private static WebApplicationFactory<Program> CreateFactory(ILanguageService languageService)
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.UseSetting("Jwt:SecretKey", "health-check-test-secret-key-at-least-32-chars");
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ILanguageService>();
                services.AddSingleton(languageService);
            });
        });
    }
}
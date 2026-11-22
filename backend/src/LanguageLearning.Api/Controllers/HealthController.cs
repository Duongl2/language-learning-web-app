using LanguageLearning.Application.DTOs.Health;
using LanguageLearning.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLearning.Api.Controllers;

[ApiController]
[AllowAnonymous]
public sealed class HealthController : ControllerBase
{
    private readonly ILanguageService _languageService;

    public HealthController(ILanguageService languageService)
    {
        _languageService = languageService;
    }

    [HttpGet("/health")]
    [HttpGet("/api/v1/health")]
    public async Task<ActionResult<HealthStatusResponse>> GetHealth(
        CancellationToken cancellationToken)
    {
        try
        {
            await _languageService.GetActiveLanguagesAsync(cancellationToken);

            return Ok(new HealthStatusResponse(
                Status: "Healthy",
                Api: "Healthy",
                Database: "Healthy",
                TimestampUtc: DateTime.UtcNow));
        }
        catch
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                new HealthStatusResponse(
                    Status: "Unhealthy",
                    Api: "Healthy",
                    Database: "Unhealthy",
                    TimestampUtc: DateTime.UtcNow));
        }
    }
}
using System.Text;
using LanguageLearning.Api.Authorization;
using LanguageLearning.Application.Common;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLearning.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1")]
public sealed class LearningStatisticsController(StatisticsService statistics) : ControllerBase
{
    [HttpGet("statistics/overview")]
    public async Task<IActionResult> Overview(DateOnly? from, DateOnly? to, CancellationToken ct) => Ok(await statistics.OverviewAsync(from, to, ct));

    [HttpGet("statistics/heatmap")]
    public async Task<IActionResult> Heatmap(DateOnly? from, DateOnly? to, CancellationToken ct) => Ok(await statistics.HeatmapAsync(from, to, ct));
}

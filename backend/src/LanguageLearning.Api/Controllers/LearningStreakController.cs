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
public sealed class LearningStreakController(StreakService streak) : ControllerBase
{
    [HttpGet("me/streak")]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await streak.StreakAsync(ct));

    [HttpPatch("me/goal")]
    public async Task<IActionResult> Goal(GoalRequest request, CancellationToken ct)
    {
        await streak.SetGoalAsync(request.DailyTarget, ct);
        return NoContent();
    }
}

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
public sealed class LearningProgressController(ProgressService progress) : ControllerBase
{
    [HttpPost("courses/{id:guid}/enroll")]
    public async Task<IActionResult> Enroll(Guid id, CancellationToken ct)
    {
        await progress.EnrollAsync(id, ct);
        return NoContent();
    }

    [HttpGet("me/progress")]
    public async Task<IActionResult> Get(CancellationToken ct) => Ok(await progress.GetAsync(ct));

    [HttpPatch("lessons/{id:guid}/progress")]
    public async Task<IActionResult> Update(Guid id, UpdateProgressRequest request, CancellationToken ct)
    {
        await progress.UpdateAsync(id, request.Status, ct);
        return NoContent();
    }
}

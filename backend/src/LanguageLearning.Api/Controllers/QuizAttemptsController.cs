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
public sealed class QuizAttemptsController(QuizService quiz) : ControllerBase
{
    [HttpPost("lessons/{id:guid}/quiz/attempts")]
    public async Task<IActionResult> Submit(Guid id, QuizSubmission request, CancellationToken ct) => Ok(await quiz.SubmitAsync(id, request, ct));

    [HttpGet("lessons/{id:guid}/quiz/attempts")]
    public async Task<IActionResult> History(Guid id, CancellationToken ct) => Ok(await quiz.HistoryAsync(id, ct));
}

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
public sealed class QuizQuestionsController(QuizReadService quiz) : ControllerBase
{
    [HttpGet("lessons/{id:guid}/quiz")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct) => Ok(await quiz.GetAsync(id, ct));
}

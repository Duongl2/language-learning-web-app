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
public sealed class LearningController(QuizService quiz, ProgressService progress, StatisticsService statistics,
    CsvService csv, SharingService sharing) : ControllerBase
{
    [HttpGet("lessons/{id:guid}/quiz")]
    public async Task<IActionResult> Quiz(Guid id, CancellationToken ct) => Ok(await quiz.GetAsync(id, ct));

    [HttpPost("lessons/{id:guid}/quiz/attempts")]
    public async Task<IActionResult> Submit(Guid id, QuizSubmission request, CancellationToken ct) => Ok(await quiz.SubmitAsync(id, request, ct));

    [HttpGet("lessons/{id:guid}/quiz/attempts")]
    public async Task<IActionResult> History(Guid id, CancellationToken ct) => Ok(await quiz.HistoryAsync(id, ct));

    [HttpPost("courses/{id:guid}/enroll")]
    public async Task<IActionResult> Enroll(Guid id, CancellationToken ct)
    {
        await progress.EnrollAsync(id, ct);
        return NoContent();
    }

    [HttpGet("me/progress")]
    public async Task<IActionResult> Progress(CancellationToken ct) => Ok(await progress.GetAsync(ct));

    [HttpPatch("lessons/{id:guid}/progress")]
    public async Task<IActionResult> UpdateProgress(Guid id, UpdateProgressRequest request, CancellationToken ct)
    {
        await progress.UpdateAsync(id, request.Status, ct);
        return NoContent();
    }

    [HttpGet("statistics/overview")]
    public async Task<IActionResult> Overview(DateOnly? from, DateOnly? to, CancellationToken ct) => Ok(await statistics.OverviewAsync(from, to, ct));

    [HttpGet("statistics/heatmap")]
    public async Task<IActionResult> Heatmap(DateOnly? from, DateOnly? to, CancellationToken ct) => Ok(await statistics.HeatmapAsync(from, to, ct));

    [HttpGet("me/streak")]
    public async Task<IActionResult> Streak(CancellationToken ct) => Ok(await statistics.StreakAsync(ct));

    [HttpPatch("me/goal")]
    public async Task<IActionResult> Goal(GoalRequest request, CancellationToken ct)
    {
        await statistics.SetGoalAsync(request.DailyTarget, ct);
        return NoContent();
    }

    [HttpGet("csv/template")]
    public IActionResult Template() => File(Encoding.UTF8.GetBytes(CsvService.Template), "text/csv; charset=utf-8", "cards-template.csv");

    [HttpPost("csv/preview")]
    [RequestSizeLimit(4_000_000)]
    public IActionResult PreviewCsv(CsvRequest request) => Ok(CsvService.Preview(request.Content));

    [HttpPost("decks/{id:guid}/csv/import")]
    [RequestSizeLimit(4_000_000)]
    public async Task<IActionResult> Import(Guid id, CsvRequest request, CancellationToken ct) => Ok(new { importedCount = await csv.ImportAsync(id, request.Content, ct) });

    [HttpGet("decks/{id:guid}/csv/export")]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct) => File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(await csv.ExportAsync(id, ct))).ToArray(), "text/csv; charset=utf-8", "cards.csv");

    [HttpGet("public/decks")]
    public async Task<IActionResult> PublicDecks([FromQuery] PaginationParams pagination, string? search, Guid? languageId, CancellationToken ct) =>
        Ok(await sharing.ListAsync(search, languageId, pagination, ct));

    [HttpGet("public/decks/{id:guid}")]
    public async Task<IActionResult> PublicDeck(Guid id, CancellationToken ct) => Ok(await sharing.PreviewAsync(id, ct));

    [HttpPost("public/decks/{id:guid}/clone")]
    public async Task<IActionResult> Clone(Guid id, CancellationToken ct) => Ok(new { id = await sharing.CloneAsync(id, ct) });

    [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
    [HttpPost("admin/decks/{id:guid}/hide")]
    public async Task<IActionResult> Hide(Guid id, CancellationToken ct)
    {
        await sharing.HideAsync(id, ct);
        return NoContent();
    }
}

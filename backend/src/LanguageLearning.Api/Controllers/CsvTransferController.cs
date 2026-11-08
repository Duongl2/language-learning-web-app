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
public sealed class CsvTransferController(CsvService csv) : ControllerBase
{
    [HttpPost("decks/{id:guid}/csv/import")]
    [RequestSizeLimit(4_000_000)]
    public async Task<IActionResult> Import(Guid id, CsvRequest request, CancellationToken ct) => Ok(new { importedCount = await csv.ImportAsync(id, request.Content, ct) });

    [HttpGet("decks/{id:guid}/csv/export")]
    public async Task<IActionResult> Export(Guid id, CancellationToken ct) => File(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(await csv.ExportAsync(id, ct))).ToArray(), "text/csv; charset=utf-8", "cards.csv");
}

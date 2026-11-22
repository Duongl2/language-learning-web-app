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
public sealed class CsvPreviewController() : ControllerBase
{
    [HttpGet("csv/template")]
    public IActionResult Template() => File(Encoding.UTF8.GetBytes(CsvPreviewService.Template), "text/csv; charset=utf-8", "cards-template.csv");

    [HttpPost("csv/preview")]
    [RequestSizeLimit(4_000_000)]
    public IActionResult Preview(CsvRequest request) => Ok(CsvPreviewService.Preview(request.Content));
}

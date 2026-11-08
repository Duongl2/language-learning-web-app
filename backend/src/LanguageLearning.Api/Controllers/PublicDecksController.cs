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
public sealed class PublicDecksController(SharingService sharing) : ControllerBase
{
    [HttpGet("public/decks")]
    public async Task<IActionResult> List([FromQuery] PaginationParams pagination, string? search, Guid? languageId, CancellationToken ct) => Ok(await sharing.ListAsync(search, languageId, pagination, ct));

    [HttpGet("public/decks/{id:guid}")]
    public async Task<IActionResult> Preview(Guid id, CancellationToken ct) => Ok(await sharing.PreviewAsync(id, ct));
}

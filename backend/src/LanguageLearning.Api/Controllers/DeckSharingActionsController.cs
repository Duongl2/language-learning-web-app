using System.Text;
using LanguageLearning.Api.Authorization;
using LanguageLearning.Application.Common;
using LanguageLearning.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLearning.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1")]
public sealed class DeckSharingActionsController(SharingWriteService sharing) : ControllerBase
{
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

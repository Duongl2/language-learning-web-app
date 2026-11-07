
using LanguageLearning.Application.DTOs.Sync;
using LanguageLearning.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLearning.Api.Controllers;

[ApiController]
[Route("api/v1/sync")]
[Authorize]
public sealed class SyncController : ControllerBase
{
    private readonly ISyncService _syncService;
    private readonly ISyncPullService _syncPullService;

    public SyncController(
        ISyncService syncService,
        ISyncPullService syncPullService)
    {
        _syncService = syncService;
        _syncPullService = syncPullService;
    }

    [HttpPost("push")]
    public async Task<ActionResult<SyncPushResponse>> Push(
        [FromBody] SyncPushRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _syncService.PushAsync(
            request,
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("pull")]
    public async Task<ActionResult<SyncPullResponse>> Pull(
        [FromQuery] string? cursor,
        [FromQuery] int limit = 50,
        CancellationToken cancellationToken = default)
    {
        var result = await _syncPullService.PullAsync(
            cursor,
            limit,
            cancellationToken);

        return Ok(result);
    }
}

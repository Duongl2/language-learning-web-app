using LanguageLearning.Application.DTOs.Auth;
using LanguageLearning.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLearning.Api.Controllers;

[ApiController]
[Route("api/v1/me")]
[Authorize]
public class MeController : ControllerBase
{
    private readonly IUserProfileService _profiles;

    public MeController(IUserProfileService profiles)
    {
        _profiles = profiles;
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileResponse>> Get(CancellationToken cancellationToken)
    {
        return Ok(await _profiles.GetAsync(cancellationToken));
    }

    [HttpPatch]
    public async Task<ActionResult<UserProfileResponse>> Update(
        [FromBody] UpdateUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        return Ok(await _profiles.UpdateAsync(request, cancellationToken));
    }
}

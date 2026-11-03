using LanguageLearning.Application.Common;
using LanguageLearning.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLearning.Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class PublishedContentController : ControllerBase
{
    private readonly PublishedContentService _content;

    public PublishedContentController(PublishedContentService content) => _content = content;

    [HttpGet("courses")]
    public async Task<IActionResult> ListCourses([FromQuery] PaginationParams pagination, [FromQuery] Guid? languageId, [FromQuery] string? level, CancellationToken ct) =>
        Ok(await _content.ListCoursesAsync(pagination, languageId, level, ct));

    [HttpGet("courses/{id:guid}")]
    public async Task<IActionResult> GetCourse(Guid id, CancellationToken ct) =>
        Ok(await _content.GetCourseAsync(id, ct));

    [HttpGet("courses/{id:guid}/units")]
    public async Task<IActionResult> ListUnits(Guid id, [FromQuery] PaginationParams pagination, CancellationToken ct) =>
        Ok(await _content.ListUnitsAsync(id, pagination, ct));

    [HttpGet("units/{id:guid}/lessons")]
    public async Task<IActionResult> ListLessons(Guid id, [FromQuery] PaginationParams pagination, CancellationToken ct) =>
        Ok(await _content.ListLessonsAsync(id, pagination, ct));

    [HttpGet("lessons/{id:guid}")]
    public async Task<IActionResult> GetLesson(Guid id, CancellationToken ct) =>
        Ok(await _content.GetLessonAsync(id, ct));
}

using LanguageLearning.Api.Authorization;
using LanguageLearning.Application.DTOs.AdminContent;
using LanguageLearning.Application.Common;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LanguageLearning.Api.Controllers;

[ApiController]
[Route("api/v1/admin")]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
public class AdminContentController : ControllerBase
{
    private readonly AdminContentService _content;

    public AdminContentController(AdminContentService content) => _content = content;

    [HttpGet("languages")]
    public async Task<IActionResult> ListLanguages([FromQuery] PaginationParams pagination, CancellationToken ct) => Ok(await _content.ListLanguagesAsync(pagination, ct));
    [HttpGet("languages/{id:guid}")]
    public async Task<IActionResult> GetLanguage(Guid id, CancellationToken ct) => Ok(await _content.GetLanguageAsync(id, ct));
    [HttpPost("languages")]
    public async Task<IActionResult> CreateLanguage(LanguageInput input, CancellationToken ct)
    {
        var value = await _content.CreateLanguageAsync(input, ct);
        return CreatedAtAction(nameof(GetLanguage), new { id = value.Id }, value);
    }
    [HttpPut("languages/{id:guid}")]
    public async Task<IActionResult> UpdateLanguage(Guid id, LanguageInput input, CancellationToken ct) => Ok(await _content.UpdateLanguageAsync(id, input, ct));
    [HttpDelete("languages/{id:guid}")]
    public async Task<IActionResult> DeleteLanguage(Guid id, CancellationToken ct)
    {
        await _content.DeleteAsync<Language>(id, ct);
        return NoContent();
    }

    [HttpGet("courses")]
    public async Task<IActionResult> ListCourses([FromQuery] PaginationParams pagination, CancellationToken ct) => Ok(await _content.ListCoursesAsync(pagination, ct));
    [HttpGet("courses/{id:guid}")]
    public async Task<IActionResult> GetCourse(Guid id, CancellationToken ct) => Ok(await _content.GetCourseAsync(id, ct));
    [HttpPost("courses")]
    public async Task<IActionResult> CreateCourse(CourseInput input, CancellationToken ct)
    {
        var value = await _content.CreateCourseAsync(input, ct);
        return CreatedAtAction(nameof(GetCourse), new { id = value.Id }, value);
    }
    [HttpPut("courses/{id:guid}")]
    public async Task<IActionResult> UpdateCourse(Guid id, CourseInput input, CancellationToken ct) => Ok(await _content.UpdateCourseAsync(id, input, ct));
    [HttpDelete("courses/{id:guid}")]
    public async Task<IActionResult> DeleteCourse(Guid id, CancellationToken ct)
    {
        await _content.DeleteAsync<Course>(id, ct);
        return NoContent();
    }

    [HttpGet("units")]
    public async Task<IActionResult> ListUnits([FromQuery] PaginationParams pagination, CancellationToken ct) => Ok(await _content.ListUnitsAsync(pagination, ct));
    [HttpGet("units/{id:guid}")]
    public async Task<IActionResult> GetUnit(Guid id, CancellationToken ct) => Ok(await _content.GetUnitAsync(id, ct));
    [HttpPost("units")]
    public async Task<IActionResult> CreateUnit(UnitInput input, CancellationToken ct)
    {
        var value = await _content.CreateUnitAsync(input, ct);
        return CreatedAtAction(nameof(GetUnit), new { id = value.Id }, value);
    }
    [HttpPut("units/{id:guid}")]
    public async Task<IActionResult> UpdateUnit(Guid id, UnitInput input, CancellationToken ct) => Ok(await _content.UpdateUnitAsync(id, input, ct));
    [HttpDelete("units/{id:guid}")]
    public async Task<IActionResult> DeleteUnit(Guid id, CancellationToken ct)
    {
        await _content.DeleteAsync<Unit>(id, ct);
        return NoContent();
    }

    [HttpGet("lessons")]
    public async Task<IActionResult> ListLessons([FromQuery] PaginationParams pagination, CancellationToken ct) => Ok(await _content.ListLessonsAsync(pagination, ct));
    [HttpGet("lessons/{id:guid}")]
    public async Task<IActionResult> GetLesson(Guid id, CancellationToken ct) => Ok(await _content.GetLessonAsync(id, ct));
    [HttpPost("lessons")]
    public async Task<IActionResult> CreateLesson(LessonInput input, CancellationToken ct)
    {
        var value = await _content.CreateLessonAsync(input, ct);
        return CreatedAtAction(nameof(GetLesson), new { id = value.Id }, value);
    }
    [HttpPut("lessons/{id:guid}")]
    public async Task<IActionResult> UpdateLesson(Guid id, LessonInput input, CancellationToken ct) => Ok(await _content.UpdateLessonAsync(id, input, ct));
    [HttpDelete("lessons/{id:guid}")]
    public async Task<IActionResult> DeleteLesson(Guid id, CancellationToken ct)
    {
        await _content.DeleteAsync<Lesson>(id, ct);
        return NoContent();
    }
}

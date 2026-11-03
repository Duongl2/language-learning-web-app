using LanguageLearning.Application.Common;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.AdminContent;
using LanguageLearning.Application.Interfaces;

namespace LanguageLearning.Application.Services;

public class PublishedContentService
{
    private readonly IPublishedContentRepository _repository;

    public PublishedContentService(IPublishedContentRepository repository) => _repository = repository;

    public async Task<PagedResponse<CourseResponse>> ListCoursesAsync(PaginationParams pagination, Guid? languageId, string? level, CancellationToken ct)
    {
        var page = await _repository.ListCoursesAsync(pagination, languageId, level, ct);
        return new PagedResponse<CourseResponse>(page.Items.Select(course => new CourseResponse(
            course.Id, course.LanguageId, course.Title, course.Level, course.Description, course.Status)).ToList(),
            page.Page, page.PageSize, page.TotalItems);
    }

    public async Task<CourseResponse> GetCourseAsync(Guid id, CancellationToken ct)
    {
        var course = await _repository.GetCourseAsync(id, ct) ?? throw new NotFoundException("Course not found.");
        return new(course.Id, course.LanguageId, course.Title, course.Level, course.Description, course.Status);
    }

    public async Task<PagedResponse<UnitResponse>> ListUnitsAsync(Guid courseId, PaginationParams pagination, CancellationToken ct)
    {
        await GetCourseAsync(courseId, ct);
        var page = await _repository.ListUnitsAsync(courseId, pagination, ct);
        return new(page.Items.Select(unit => new UnitResponse(unit.Id, unit.CourseId, unit.Title, unit.SortOrder)).ToList(),
            page.Page, page.PageSize, page.TotalItems);
    }

    public async Task<PagedResponse<LessonResponse>> ListLessonsAsync(Guid unitId, PaginationParams pagination, CancellationToken ct)
    {
        if (await _repository.GetUnitAsync(unitId, ct) is null)
            throw new NotFoundException("Unit not found.");
        var page = await _repository.ListLessonsAsync(unitId, pagination, ct);
        return new(page.Items.Select(MapLesson).ToList(), page.Page, page.PageSize, page.TotalItems);
    }

    public async Task<LessonResponse> GetLessonAsync(Guid id, CancellationToken ct)
    {
        var lesson = await _repository.GetLessonAsync(id, ct) ?? throw new NotFoundException("Lesson not found.");
        return MapLesson(lesson);
    }

    private static LessonResponse MapLesson(LanguageLearning.Domain.Entities.Lesson lesson) =>
        new(lesson.Id, lesson.UnitId, lesson.Title, lesson.Content, lesson.SortOrder, lesson.Status);
}

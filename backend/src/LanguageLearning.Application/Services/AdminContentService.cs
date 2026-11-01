using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Common;
using LanguageLearning.Application.DTOs.AdminContent;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Common;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.Services;

public class AdminContentService
{
    private readonly IAdminContentRepository _repository;

    public AdminContentService(IAdminContentRepository repository) => _repository = repository;

    public async Task<PagedResponse<LanguageResponse>> ListLanguagesAsync(PaginationParams pagination, CancellationToken ct)
    {
        var page = await _repository.ListAsync<Language>(pagination, ct);
        return new(page.Items.Select(Map).ToList(), page.Page, page.PageSize, page.TotalItems);
    }
    public async Task<LanguageResponse> GetLanguageAsync(Guid id, CancellationToken ct) =>
        Map(await RequiredAsync<Language>(id, ct));
    public async Task<LanguageResponse> CreateLanguageAsync(LanguageInput input, CancellationToken ct)
    {
        ValidateLanguage(input);
        var language = new Language { Code = input.Code.Trim().ToLowerInvariant(), Name = input.Name.Trim(), IsActive = input.IsActive };
        await _repository.AddAsync(language, ct);
        await _repository.SaveAsync(ct);
        return Map(language);
    }
    public async Task<LanguageResponse> UpdateLanguageAsync(Guid id, LanguageInput input, CancellationToken ct)
    {
        ValidateLanguage(input);
        var language = await RequiredAsync<Language>(id, ct);
        language.Code = input.Code.Trim().ToLowerInvariant();
        language.Name = input.Name.Trim();
        language.IsActive = input.IsActive;
        language.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveAsync(ct);
        return Map(language);
    }

    public async Task<PagedResponse<CourseResponse>> ListCoursesAsync(PaginationParams pagination, CancellationToken ct)
    {
        var page = await _repository.ListAsync<Course>(pagination, ct);
        return new(page.Items.Select(Map).ToList(), page.Page, page.PageSize, page.TotalItems);
    }
    public async Task<CourseResponse> GetCourseAsync(Guid id, CancellationToken ct) =>
        Map(await RequiredAsync<Course>(id, ct));
    public async Task<CourseResponse> CreateCourseAsync(CourseInput input, CancellationToken ct)
    {
        await ValidateCourseAsync(input, ct);
        var course = new Course { LanguageId = input.LanguageId, Title = input.Title.Trim(), Level = input.Level.Trim(), Description = input.Description?.Trim() ?? string.Empty, Status = input.Status };
        await _repository.AddAsync(course, ct);
        await _repository.SaveAsync(ct);
        return Map(course);
    }
    public async Task<CourseResponse> UpdateCourseAsync(Guid id, CourseInput input, CancellationToken ct)
    {
        await ValidateCourseAsync(input, ct);
        var course = await RequiredAsync<Course>(id, ct);
        course.LanguageId = input.LanguageId;
        course.Title = input.Title.Trim();
        course.Level = input.Level.Trim();
        course.Description = input.Description?.Trim() ?? string.Empty;
        course.Status = input.Status;
        course.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveAsync(ct);
        return Map(course);
    }

    public async Task<PagedResponse<UnitResponse>> ListUnitsAsync(PaginationParams pagination, CancellationToken ct)
    {
        var page = await _repository.ListAsync<Unit>(pagination, ct);
        return new(page.Items.Select(Map).ToList(), page.Page, page.PageSize, page.TotalItems);
    }
    public async Task<UnitResponse> GetUnitAsync(Guid id, CancellationToken ct) =>
        Map(await RequiredAsync<Unit>(id, ct));
    public async Task<UnitResponse> CreateUnitAsync(UnitInput input, CancellationToken ct)
    {
        await ValidateUnitAsync(input, ct);
        var unit = new Unit { CourseId = input.CourseId, Title = input.Title.Trim(), SortOrder = input.SortOrder };
        await _repository.AddAsync(unit, ct);
        await _repository.SaveAsync(ct);
        return Map(unit);
    }
    public async Task<UnitResponse> UpdateUnitAsync(Guid id, UnitInput input, CancellationToken ct)
    {
        await ValidateUnitAsync(input, ct);
        var unit = await RequiredAsync<Unit>(id, ct);
        unit.CourseId = input.CourseId;
        unit.Title = input.Title.Trim();
        unit.SortOrder = input.SortOrder;
        unit.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveAsync(ct);
        return Map(unit);
    }

    public async Task<PagedResponse<LessonResponse>> ListLessonsAsync(PaginationParams pagination, CancellationToken ct)
    {
        var page = await _repository.ListAsync<Lesson>(pagination, ct);
        return new(page.Items.Select(Map).ToList(), page.Page, page.PageSize, page.TotalItems);
    }
    public async Task<LessonResponse> GetLessonAsync(Guid id, CancellationToken ct) =>
        Map(await RequiredAsync<Lesson>(id, ct));
    public async Task<LessonResponse> CreateLessonAsync(LessonInput input, CancellationToken ct)
    {
        await ValidateLessonAsync(input, ct);
        var lesson = new Lesson { UnitId = input.UnitId, Title = input.Title.Trim(), Content = input.Content?.Trim() ?? string.Empty, SortOrder = input.SortOrder, Status = input.Status };
        await _repository.AddAsync(lesson, ct);
        await _repository.SaveAsync(ct);
        return Map(lesson);
    }
    public async Task<LessonResponse> UpdateLessonAsync(Guid id, LessonInput input, CancellationToken ct)
    {
        await ValidateLessonAsync(input, ct);
        var lesson = await RequiredAsync<Lesson>(id, ct);
        lesson.UnitId = input.UnitId;
        lesson.Title = input.Title.Trim();
        lesson.Content = input.Content?.Trim() ?? string.Empty;
        lesson.SortOrder = input.SortOrder;
        lesson.Status = input.Status;
        lesson.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveAsync(ct);
        return Map(lesson);
    }

    public async Task DeleteAsync<T>(Guid id, CancellationToken ct) where T : BaseEntity
    {
        var entity = await RequiredAsync<T>(id, ct);
        if (await _repository.HasDependentsAsync<T>(id, ct))
            throw new ConflictException("Content is referenced by other records.");
        _repository.Remove(entity);
        await _repository.SaveAsync(ct);
    }

    private async Task<T> RequiredAsync<T>(Guid id, CancellationToken ct) where T : BaseEntity =>
        await _repository.FindAsync<T>(id, ct) ?? throw new NotFoundException($"{typeof(T).Name} not found.");

    private static void RequiredText(string? value, string field, int maxLength = 200)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Trim().Length > maxLength)
            throw new ArgumentException($"{field} must contain 1 to {maxLength} characters.");
    }
    private static void ValidateLanguage(LanguageInput input)
    {
        RequiredText(input.Code, "Language code", 16);
        RequiredText(input.Name, "Language name");
    }
    private async Task ValidateCourseAsync(CourseInput input, CancellationToken ct)
    {
        RequiredText(input.Title, "Course title");
        RequiredText(input.Level, "Course level", 20);
        ValidateStatus(input.Status);
        await RequiredAsync<Language>(input.LanguageId, ct);
    }
    private async Task ValidateUnitAsync(UnitInput input, CancellationToken ct)
    {
        RequiredText(input.Title, "Unit title");
        if (input.SortOrder < 0) throw new ArgumentException("Sort order cannot be negative.");
        await RequiredAsync<Course>(input.CourseId, ct);
    }
    private async Task ValidateLessonAsync(LessonInput input, CancellationToken ct)
    {
        RequiredText(input.Title, "Lesson title");
        if (input.SortOrder < 0) throw new ArgumentException("Sort order cannot be negative.");
        ValidateStatus(input.Status);
        await RequiredAsync<Unit>(input.UnitId, ct);
    }
    private static void ValidateStatus(ContentStatus status)
    {
        if (!Enum.IsDefined(status)) throw new ArgumentException("Invalid content status.");
    }
    private static LanguageResponse Map(Language value) => new(value.Id, value.Code, value.Name, value.IsActive);
    private static CourseResponse Map(Course value) => new(value.Id, value.LanguageId, value.Title, value.Level, value.Description, value.Status);
    private static UnitResponse Map(Unit value) => new(value.Id, value.CourseId, value.Title, value.SortOrder);
    private static LessonResponse Map(Lesson value) => new(value.Id, value.UnitId, value.Title, value.Content, value.SortOrder, value.Status);
}

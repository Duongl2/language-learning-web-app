using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Common;
using LanguageLearning.Application.DTOs.AdminContent;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using Moq;

namespace LanguageLearning.Tests;

public class AdminContentServiceTests
{
    private readonly Mock<IAdminContentRepository> _repository = new();
    private readonly AdminContentService _service;

    public AdminContentServiceTests()
    {
        _service = new AdminContentService(_repository.Object);
    }

    [Fact]
    public async Task LanguageCrudCreatesUpdatesAndDeletes()
    {
        var created = await _service.CreateLanguageAsync(new LanguageInput(" JA ", " Japanese "), default);
        Assert.Equal("ja", created.Code);
        Assert.Equal("Japanese", created.Name);

        var entity = new Language { Id = created.Id, Code = created.Code, Name = created.Name };
        _repository.Setup(repository => repository.FindAsync<Language>(created.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(entity);
        var updated = await _service.UpdateLanguageAsync(created.Id, new LanguageInput("ko", "Korean", false), default);
        Assert.Equal("ko", updated.Code);
        Assert.False(updated.IsActive);

        await _service.DeleteAsync<Language>(created.Id, default);
        _repository.Verify(repository => repository.Remove(entity), Times.Once);
        _repository.Verify(repository => repository.SaveAsync(It.IsAny<CancellationToken>()), Times.Exactly(3));
    }

    [Fact]
    public async Task CourseRequiresExistingLanguage()
    {
        var input = new CourseInput(Guid.NewGuid(), "Basics", "A1", "", ContentStatus.Draft);
        await Assert.ThrowsAsync<NotFoundException>(() => _service.CreateCourseAsync(input, default));
        _repository.Verify(repository => repository.AddAsync(It.IsAny<Course>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UnitAndLessonRequireTheirParent()
    {
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateUnitAsync(new UnitInput(Guid.NewGuid(), "Unit", 1), default));
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _service.CreateLessonAsync(new LessonInput(Guid.NewGuid(), "Lesson", "Content", 1, ContentStatus.Draft), default));
    }

    [Fact]
    public async Task RejectsInvalidContentStatus()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.CreateCourseAsync(new CourseInput(Guid.NewGuid(), "Basics", "A1", "", (ContentStatus)99), default));
    }

    [Fact]
    public async Task DeleteRejectsContentWithDependents()
    {
        var language = new Language();
        _repository.Setup(repository => repository.FindAsync<Language>(language.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(language);
        _repository.Setup(repository => repository.HasDependentsAsync<Language>(language.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        await Assert.ThrowsAsync<ConflictException>(() => _service.DeleteAsync<Language>(language.Id, default));

        _repository.Verify(repository => repository.Remove(It.IsAny<Language>()), Times.Never);
    }

    [Fact]
    public async Task LanguageListPreservesPaginationMetadata()
    {
        var pagination = new PaginationParams { Page = 2, PageSize = 10 };
        _repository.Setup(repository => repository.ListAsync<Language>(pagination, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PagedResponse<Language>(new[] { new Language { Code = "ja", Name = "Japanese" } }, 2, 10, 11));

        var page = await _service.ListLanguagesAsync(pagination, default);

        Assert.Equal(2, page.Page);
        Assert.Equal(11, page.TotalItems);
        Assert.Equal("ja", Assert.Single(page.Items).Code);
    }
}

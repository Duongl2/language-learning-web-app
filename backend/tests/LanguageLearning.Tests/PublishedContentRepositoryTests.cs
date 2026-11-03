using LanguageLearning.Application.Common;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;
using LanguageLearning.Infrastructure.Data;
using LanguageLearning.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Tests;

public class PublishedContentRepositoryTests
{
    [Fact]
    public async Task LearnerSeesOnlyPublishedContentInSortOrderAndSeesNewPublishImmediately()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var language = new Language { Code = "ja", Name = "Japanese" };
        var course = new Course { Language = language, Title = "Basics", Status = ContentStatus.Draft };
        var firstUnit = new Unit { Course = course, Title = "First", SortOrder = 1 };
        var secondUnit = new Unit { Course = course, Title = "Second", SortOrder = 2 };
        var publishedLesson = new Lesson { Unit = firstUnit, Title = "Visible", SortOrder = 2, Status = ContentStatus.Published };
        var draftLesson = new Lesson { Unit = firstUnit, Title = "Hidden", SortOrder = 1, Status = ContentStatus.Draft };
        context.Lessons.AddRange(publishedLesson, draftLesson);
        context.Units.Add(secondUnit);
        await context.SaveChangesAsync();

        var repository = new PublishedContentRepository(context);
        var pagination = new PaginationParams();
        Assert.Empty((await repository.ListCoursesAsync(pagination, null, null, default)).Items);
        Assert.Null(await repository.GetLessonAsync(publishedLesson.Id, default));

        var admin = new AdminContentService(new AdminContentRepository(context));
        await admin.SetCoursePublishedAsync(course.Id, true, default);

        Assert.Single((await repository.ListCoursesAsync(pagination, null, null, default)).Items);
        Assert.Single((await repository.ListCoursesAsync(pagination, language.Id, "a1", default)).Items);
        Assert.Empty((await repository.ListCoursesAsync(pagination, language.Id, "A2", default)).Items);
        Assert.Equal(new[] { firstUnit.Id, secondUnit.Id },
            (await repository.ListUnitsAsync(course.Id, pagination, default)).Items.Select(unit => unit.Id));
        Assert.Equal(publishedLesson.Id,
            Assert.Single((await repository.ListLessonsAsync(firstUnit.Id, pagination, default)).Items).Id);
        Assert.Null(await repository.GetLessonAsync(draftLesson.Id, default));

        publishedLesson.Status = ContentStatus.Draft;
        await context.SaveChangesAsync();
        Assert.Empty((await repository.ListLessonsAsync(firstUnit.Id, pagination, default)).Items);
    }
}

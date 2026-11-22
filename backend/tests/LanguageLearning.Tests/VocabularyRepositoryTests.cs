using LanguageLearning.Application.Common;
using LanguageLearning.Application.DTOs.AdminContent;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Infrastructure.Data;
using LanguageLearning.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LanguageLearning.Tests;

public class VocabularyRepositoryTests
{
    [Fact]
    public async Task CrudSearchAndPaginationStayWithinLesson()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(connection).Options;
        await using var context = new AppDbContext(options);
        await context.Database.EnsureCreatedAsync();

        var language = new Language { Code = "ja", Name = "Japanese" };
        var course = new Course { Language = language, Title = "Basics" };
        var unit = new Unit { Course = course, Title = "Unit" };
        var lesson = new Lesson { Unit = unit, Title = "Lesson" };
        var otherLesson = new Lesson { Unit = unit, Title = "Other" };
        context.Lessons.AddRange(lesson, otherLesson);
        await context.SaveChangesAsync();

        var service = new AdminContentService(new AdminContentRepository(context));
        var first = await service.CreateVocabularyAsync(new(lesson.Id, "猫", "neko", "cat", "a cat", null, null), default);
        await service.CreateVocabularyAsync(new(lesson.Id, "犬", "inu", "dog", "a dog", null, null), default);
        await service.CreateVocabularyAsync(new(otherLesson.Id, "猫", "neko", "cat", "a cat", null, null), default);

        var page = await service.ListVocabulariesAsync(lesson.Id, "cat", new PaginationParams { PageSize = 1 }, default);
        Assert.Equal(1, page.TotalItems);
        Assert.Equal(first.Id, Assert.Single(page.Items).Id);
        Assert.Equal(language.Id, first.LanguageId);
        Assert.Equal("neko", first.Phonetic);
        Assert.Equal("a cat", first.Example);
        Assert.Equal(0, (await service.ListVocabulariesAsync(lesson.Id, "%", new PaginationParams(), default)).TotalItems);

        var all = await service.ListVocabulariesAsync(lesson.Id, null, new PaginationParams { PageSize = 1 }, default);
        Assert.Equal(2, all.TotalItems);
        Assert.True(all.HasNextPage);
        Assert.Single((await service.ListVocabulariesAsync(lesson.Id, null,
            new PaginationParams { Page = 2, PageSize = 1 }, default)).Items);

        var updated = await service.UpdateVocabularyAsync(first.Id, new(otherLesson.Id, "鳥", "tori", "bird", "a bird", null, null), default);
        Assert.Equal(otherLesson.Id, updated.LessonId);
        Assert.Equal("bird", (await service.GetVocabularyAsync(first.Id, default)).Meaning);
        await service.DeleteAsync<Vocabulary>(first.Id, default);
        Assert.Equal(1, (await service.ListVocabulariesAsync(otherLesson.Id, null, new PaginationParams(), default)).TotalItems);
    }
}

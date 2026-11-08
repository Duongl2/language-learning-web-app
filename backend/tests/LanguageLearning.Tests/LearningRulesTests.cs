using System.Text.Json;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Tests;

public sealed class LearningRulesTests
{
    [Fact]
    public void MultipleChoice_OnlyAcceptsAnExistingCorrectOption()
    {
        var correct = new QuestionOption { IsCorrect = true };
        var incorrect = new QuestionOption { IsCorrect = false };
        var question = new Question { Options = new[] { correct, incorrect } };
        Assert.True(QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(correct.Id)));
        Assert.False(QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(incorrect.Id)));
        Assert.False(QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(Guid.NewGuid())));
        Assert.False(QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(1)));
    }

    [Theory]
    [InlineData(" hello ", true)]
    [InlineData("HI", true)]
    [InlineData("bye", false)]
    public void FillInBlank_UsesConfiguredAlternatives(string answer, bool expected)
    {
        var question = new Question { Type = QuestionType.FillInBlank, ConfigJson = "{\"answers\":[\"Hello\",\"Hi\"]}" };
        Assert.Equal(expected, QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(answer)));
    }

    [Theory]
    [InlineData("{\"a\":\"2\",\"b\":\"1\"}", true)]
    [InlineData("{\"a\":\"1\",\"b\":\"2\"}", false)]
    [InlineData("{\"a\":\"2\"}", false)]
    [InlineData("{\"a\":\"2\",\"a\":\"2\"}", false)]
    public void Matching_RequiresTheCompleteExactMapping(string answer, bool expected)
    {
        var question = new Question { Type = QuestionType.Matching, ConfigJson = "{\"pairs\":{\"a\":\"2\",\"b\":\"1\"}}" };
        Assert.Equal(expected, QuizService.IsCorrect(question, JsonSerializer.Deserialize<JsonElement>(answer)));
    }

    [Fact]
    public void Csv_ParsesEscapesMultilineUnicodeAndReportsPhysicalLineNumbers()
    {
        var preview = CsvService.Preview("\uFEFFfront,back\r\n\"Xin, chào\",\"Hello \"\"friend\"\"\"\r\n\"two\nlines\",ok\r\n,missing\r\n");
        Assert.Equal(2, preview.ValidCount);
        Assert.Equal(1, preview.InvalidCount);
        Assert.Equal("Hello \"friend\"", preview.Rows[0].Back);
        Assert.Equal("two\nlines", preview.Rows[1].Front);
        Assert.Equal(5, preview.Rows[2].Line);
    }

    [Theory]
    [InlineData("front,back\na,b\na,b", 1)]
    [InlineData("front,back\na,b,c", 1)]
    public void Csv_ReportsInvalidAndDuplicateRows(string content, int invalid) => Assert.Equal(invalid, CsvService.Preview(content).InvalidCount);

    [Theory]
    [InlineData("term,meaning\na,b")]
    [InlineData("front,back\n\"unterminated,b")]
    [InlineData("front,back\na\"b,c")]
    [InlineData("front,back\n\"a\"x,b")]
    [InlineData("front,back\n\uFFFD,b")]
    public void Csv_RejectsMalformedInput(string content) => Assert.Throws<ArgumentException>(() => CsvService.Preview(content));

    [Theory]
    [InlineData("=SUM(A1)")]
    [InlineData("  +cmd")]
    [InlineData("-1")]
    [InlineData("@function")]
    [InlineData("\tformula")]
    [InlineData("\rformula")]
    public void Csv_ExportNeutralizesFormulaInjection(string value) => Assert.StartsWith("\"'", CsvService.Escape(value));

    [Fact]
    public void Streak_UsesUserTimezoneAndDeduplicatesOfflineEvents()
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        var first = new ReviewEvent { ReviewedAt = new DateTime(2026, 11, 7, 16, 59, 0, DateTimeKind.Utc), ClientEventId = Guid.NewGuid() };
        var second = new ReviewEvent { ReviewedAt = new DateTime(2026, 11, 7, 17, 1, 0, DateTimeKind.Utc), ClientEventId = Guid.NewGuid() };
        var now = new DateTime(2026, 11, 8, 3, 0, 0, DateTimeKind.Utc);
        var result = StatisticsService.Calculate(new[] { first, second, second }, Array.Empty<LessonProgress>(), zone, 1, now);
        Assert.Equal(2, result.CurrentStreak);
        Assert.Equal(2, result.LongestStreak);
        Assert.Equal(1, result.TodayReviews);
        Assert.Equal(new[] { "2026-11-07", "2026-11-08" }, result.Days.Select(x => x.Date));
        var unmet = StatisticsService.Calculate(new[] { first, second, second }, Array.Empty<LessonProgress>(), zone, 2, now);
        Assert.Equal(0, unmet.CurrentStreak);
    }

    [Fact]
    public void Streak_PreservesYesterdayUntilTodayEndsAndResetsAfterMissedDay()
    {
        var review = new ReviewEvent { ReviewedAt = new DateTime(2026, 11, 7, 10, 0, 0, DateTimeKind.Utc), ClientEventId = Guid.NewGuid() };
        var now = new DateTime(2026, 11, 8, 10, 0, 0, DateTimeKind.Utc);
        Assert.Equal(1, StatisticsService.Calculate(new[] { review }, Array.Empty<LessonProgress>(), TimeZoneInfo.Utc, 1, now).CurrentStreak);
        Assert.Equal(0, StatisticsService.Calculate(new[] { review }, Array.Empty<LessonProgress>(), TimeZoneInfo.Utc, 1, now.AddDays(1)).CurrentStreak);
    }
}

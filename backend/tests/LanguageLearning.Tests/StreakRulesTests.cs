using System.Text.Json;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Tests;

public sealed class StreakRulesTests
{
    [Fact]
    public void Streak_UsesUserTimezoneAndDeduplicatesOfflineEvents()
    {
        var zone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
        var first = new ReviewEvent { ReviewedAt = new DateTime(2026, 11, 7, 16, 59, 0, DateTimeKind.Utc), ClientEventId = Guid.NewGuid() };
        var second = new ReviewEvent { ReviewedAt = new DateTime(2026, 11, 7, 17, 1, 0, DateTimeKind.Utc), ClientEventId = Guid.NewGuid() };
        var now = new DateTime(2026, 11, 8, 3, 0, 0, DateTimeKind.Utc);
        var result = StreakService.Calculate(new[] { first, second, second }, Array.Empty<LessonProgress>(), zone, 1, now);
        Assert.Equal(2, result.CurrentStreak);
        Assert.Equal(2, result.LongestStreak);
        Assert.Equal(1, result.TodayReviews);
        Assert.Equal(new[] { "2026-11-07", "2026-11-08" }, result.Days.Select(x => x.Date));
        var unmet = StreakService.Calculate(new[] { first, second, second }, Array.Empty<LessonProgress>(), zone, 2, now);
        Assert.Equal(0, unmet.CurrentStreak);
    }


    [Fact]
    public void Streak_PreservesYesterdayUntilTodayEndsAndResetsAfterMissedDay()
    {
        var review = new ReviewEvent { ReviewedAt = new DateTime(2026, 11, 7, 10, 0, 0, DateTimeKind.Utc), ClientEventId = Guid.NewGuid() };
        var now = new DateTime(2026, 11, 8, 10, 0, 0, DateTimeKind.Utc);
        Assert.Equal(1, StreakService.Calculate(new[] { review }, Array.Empty<LessonProgress>(), TimeZoneInfo.Utc, 1, now).CurrentStreak);
        Assert.Equal(0, StreakService.Calculate(new[] { review }, Array.Empty<LessonProgress>(), TimeZoneInfo.Utc, 1, now.AddDays(1)).CurrentStreak);
    }
}

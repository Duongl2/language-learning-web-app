using System.Globalization;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.Services;

public sealed class StreakService(IStreakRepository repository, ICurrentUserService currentUser, TimeProvider clock)
{
    public async Task<StreakResponse> StreakAsync(CancellationToken ct)
    {
        var userId = currentUser.GetRequiredUserId();
        var user = await repository.GetUserAsync(userId, ct);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
        var target = (await repository.GetGoalAsync(userId, ct))?.DailyTarget ?? 10;
        var reviews = await repository.GetReviewsAsync(userId, ct);
        var progress = await repository.GetProgressAsync(userId, ct);
        return Calculate(reviews, progress, zone, target, clock.GetUtcNow().UtcDateTime);
    }

    public static StreakResponse Calculate(IEnumerable<ReviewEvent> reviews, IEnumerable<LessonProgress> progress,
        TimeZoneInfo zone, int target, DateTime now)
    {
        var today = LocalDate(now, zone);
        // Deduplicate by the client event identifier, including replayed offline reviews.
        var reviewDays = reviews.DistinctBy(x => x.ClientEventId).GroupBy(x => LocalDate(x.ReviewedAt, zone))
            .ToDictionary(x => x.Key, x => x.Count());
        var lessonDays = progress.Where(x => x.CompletedAt.HasValue).DistinctBy(x => x.LessonId)
            .GroupBy(x => LocalDate(x.CompletedAt!.Value, zone)).ToDictionary(x => x.Key, x => x.Count());
        var qualified = reviewDays.Where(x => x.Key <= today && x.Value >= target).Select(x => x.Key).ToHashSet();
        var current = 0;
        var date = qualified.Contains(today) ? today : today.AddDays(-1);
        while (qualified.Contains(date)) { current++; date = date.AddDays(-1); }
        var longest = 0;
        var run = 0;
        DateOnly? previous = null;
        foreach (var day in qualified.Order())
        {
            run = previous.HasValue && day == previous.Value.AddDays(1) ? run + 1 : 1;
            longest = Math.Max(longest, run);
            previous = day;
        }
        var days = reviewDays.Keys.Union(lessonDays.Keys).Where(x => x <= today).Order().Select(day =>
            new StreakDay(day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), reviewDays.GetValueOrDefault(day),
                lessonDays.GetValueOrDefault(day), qualified.Contains(day))).ToList();
        return new StreakResponse(current, longest, target, reviewDays.GetValueOrDefault(today), zone.Id, days);
    }

    public async Task SetGoalAsync(int target, CancellationToken ct)
    {
        if (target is < 1 or > 1000) throw new ArgumentException("Daily target must be between 1 and 1000.");
        await repository.SetGoalAsync(currentUser.GetRequiredUserId(), target, ct);
        await repository.SaveChangesAsync(ct);
    }

    private static DateOnly LocalDate(DateTime value, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(value, DateTimeKind.Utc), zone));
}

using System.Globalization;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.Services;

public sealed class StatisticsService(IStatisticsRepository repository, ICurrentUserService currentUser, TimeProvider clock)
{
    public async Task<StatisticsOverview> OverviewAsync(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var userId = currentUser.GetRequiredUserId();
        var user = await repository.GetUserAsync(userId, ct);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
        var reviews = Filter(await repository.GetReviewsAsync(userId, ct), zone, from, to);
        var schedules = await repository.GetSchedulesAsync(userId, ct);
        var total = await repository.CountCardsAsync(userId, ct);
        return new StatisticsOverview(Math.Max(0, total - schedules.Count), schedules.Count,
            schedules.Count(x => x.DueAt <= clock.GetUtcNow().UtcDateTime), reviews.Count,
            reviews.Count == 0 ? 0 : Math.Round(100.0 * reviews.Count(x => x.Rating != SrsRating.Again) / reviews.Count, 1),
            Enum.GetValues<SrsRating>().Select(rating => new RatingCount(rating.ToString(), reviews.Count(x => x.Rating == rating))).ToList());
    }

    public async Task<List<ActivityDay>> HeatmapAsync(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        ValidateRange(from, to);
        var userId = currentUser.GetRequiredUserId();
        var user = await repository.GetUserAsync(userId, ct);
        var zone = TimeZoneInfo.FindSystemTimeZoneById(user.Timezone);
        var target = (await repository.GetGoalAsync(userId, ct))?.DailyTarget ?? 10;
        var reviews = Filter(await repository.GetReviewsAsync(userId, ct), zone, from, to)
            .DistinctBy(x => x.ClientEventId).GroupBy(x => LocalDate(x.ReviewedAt, zone)).ToDictionary(x => x.Key, x => x.Count());
        var progress = await repository.GetProgressAsync(userId, ct);
        var lessons = progress.Where(x => x.CompletedAt.HasValue).DistinctBy(x => x.LessonId)
            .GroupBy(x => LocalDate(x.CompletedAt!.Value, zone)).ToDictionary(x => x.Key, x => x.Count());
        var today = LocalDate(clock.GetUtcNow().UtcDateTime, zone);
        return reviews.Keys.Union(lessons.Keys).Where(day => day <= today && (!from.HasValue || day >= from) && (!to.HasValue || day <= to))
            .Order().Select(day => new ActivityDay(day.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture), reviews.GetValueOrDefault(day),
                lessons.GetValueOrDefault(day), reviews.GetValueOrDefault(day) >= target)).ToList();
    }

    private static DateOnly LocalDate(DateTime value, TimeZoneInfo zone) =>
        DateOnly.FromDateTime(TimeZoneInfo.ConvertTimeFromUtc(DateTime.SpecifyKind(value, DateTimeKind.Utc), zone));

    private static List<ReviewEvent> Filter(List<ReviewEvent> reviews, TimeZoneInfo zone, DateOnly? from, DateOnly? to)
    {
        ValidateRange(from, to);
        return reviews.Where(x => (!from.HasValue || LocalDate(x.ReviewedAt, zone) >= from) &&
            (!to.HasValue || LocalDate(x.ReviewedAt, zone) <= to)).ToList();
    }

    private static void ValidateRange(DateOnly? from, DateOnly? to)
    {
        if (from.HasValue && to.HasValue && from > to) throw new ArgumentException("From date must precede to date.");
    }
}

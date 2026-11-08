using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LanguageLearning.Application.DTOs.Learning;

public sealed class GoalRequest
{
    [Range(1, 1000)] public int DailyTarget { get; set; } = 10;
}

public sealed record StreakResponse(int CurrentStreak, int LongestStreak, int DailyTarget,
    int TodayReviews, string Timezone, IReadOnlyList<StreakDay> Days);
public sealed record StreakDay(string Date, int ReviewCount, int LessonsCompleted, bool GoalMet);

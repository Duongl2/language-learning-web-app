using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LanguageLearning.Application.DTOs.Learning;

public sealed record ActivityDay(string Date, int ReviewCount, int LessonsCompleted, bool GoalMet);

public sealed record RatingCount(string Rating, int Count);

public sealed record StatisticsOverview(int NewCards, int LearnedCards, int DueCards,
    int ReviewCount, double Retention, IReadOnlyList<RatingCount> Ratings);

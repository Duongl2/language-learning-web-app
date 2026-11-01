
using System.Text.Json;
using LanguageLearning.Application.DTOs.Reviews;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.Srs;

public static class ReviewIntervalCalculator
{
    public static ReviewIntervalsResponse Calculate(string stateJson)
    {

        SrsState state;

        if (string.IsNullOrWhiteSpace(stateJson) ||
            stateJson.Trim() == "{}")
        {
            state = new SrsState(0, 0, 2.5);
        }
        else
        {
            state = JsonSerializer.Deserialize<SrsState>(
                stateJson,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }
            ) ?? throw new InvalidOperationException(
             "Invalid SRS state."
            );
        }

        var scheduler = new SrsV1Scheduler();
        var nowUtc = DateTime.UtcNow;

        double GetMinutes(SrsRating rating)
        {
            var (_, dueAt) = scheduler.Schedule(
                state,
                rating,
                nowUtc
            );

            return (dueAt - nowUtc).TotalMinutes;
        }

        return new ReviewIntervalsResponse
        {
            AgainMinutes = GetMinutes(SrsRating.Again),
            HardMinutes = GetMinutes(SrsRating.Hard),
            GoodMinutes = GetMinutes(SrsRating.Good),
            EasyMinutes = GetMinutes(SrsRating.Easy)
        };
    }
}

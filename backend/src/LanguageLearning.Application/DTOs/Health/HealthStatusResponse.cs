namespace LanguageLearning.Application.DTOs.Health;

public sealed record HealthStatusResponse(
    string Status,
    string Api,
    string Database,
    DateTime TimestampUtc);
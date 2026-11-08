using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LanguageLearning.Application.DTOs.Learning;

public sealed record CsvRow(int Line, string Front, string Back, string? Error);

public sealed record CsvPreview(IReadOnlyList<CsvRow> Rows, int ValidCount, int InvalidCount);

public sealed class CsvRequest
{
    [Required, MaxLength(2_000_000)] public string Content { get; set; } = string.Empty;
}

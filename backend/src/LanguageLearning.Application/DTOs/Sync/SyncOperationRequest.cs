
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LanguageLearning.Application.DTOs.Sync;

public sealed class SyncOperationRequest
{
    public Guid OperationId { get; set; }

    [Required]
    public string Type { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public JsonElement Payload { get; set; }

    public DateTimeOffset OccurredAt { get; set; }
}

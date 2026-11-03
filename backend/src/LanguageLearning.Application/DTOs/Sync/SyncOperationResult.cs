
namespace LanguageLearning.Application.DTOs.Sync;

public sealed class SyncOperationResult
{
    public Guid OperationId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string? Message { get; set; }
}

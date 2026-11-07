
namespace LanguageLearning.Application.DTOs.Sync;

public sealed class SyncChangeResponse
{
    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public int Version { get; set; }

    public string Action { get; set; } = string.Empty;

    public DateTime ChangedAt { get; set; }

    public SyncDeckSnapshot? Deck { get; set; }
}


namespace LanguageLearning.Application.DTOs.Sync;

public sealed class SyncPullResponse
{
    public List<SyncChangeResponse> Changes { get; set; } = [];

    public string? NextCursor { get; set; }

    public bool HasMore { get; set; }
}


namespace LanguageLearning.Application.DTOs.Sync;

public sealed class SyncDeckSnapshot
{
    public Guid Id { get; set; }

    public Guid LanguageId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public bool IsPublic { get; set; }

    public int Version { get; set; }

    public List<string> Tags { get; set; } = [];
}

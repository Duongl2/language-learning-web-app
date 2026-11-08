using LanguageLearning.Application.DTOs.Learning;

namespace LanguageLearning.Application.Interfaces;

public interface ISharingRepository
{
    Task<(List<PublicDeckDto> Items, int Total)> ListAsync(string? search, Guid? languageId, int page, int pageSize, CancellationToken ct);
    Task<PublicDeckPreview?> PreviewAsync(Guid id, CancellationToken ct);
    Task<Guid> CloneAsync(Guid id, Guid ownerId, CancellationToken ct);
    Task HideAsync(Guid id, Guid actorId, CancellationToken ct);
}

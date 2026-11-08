using LanguageLearning.Application.DTOs.Learning;

namespace LanguageLearning.Application.Interfaces;

public interface ISharingReadRepository
{
    Task<(List<PublicDeckDto> Items, int Total)> ListAsync(string? search, Guid? languageId, int page, int pageSize, CancellationToken ct);
    Task<PublicDeckPreview?> PreviewAsync(Guid id, CancellationToken ct);
}

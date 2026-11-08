using LanguageLearning.Application.Common;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;

namespace LanguageLearning.Application.Services;

public sealed class SharingService(ISharingRepository repository, ICurrentUserService currentUser)
{
    public async Task<PagedResponse<PublicDeckDto>> ListAsync(string? search, Guid? languageId, PaginationParams pagination, CancellationToken ct)
    {
        var (items, total) = await repository.ListAsync(search, languageId, pagination.Page, pagination.PageSize, ct);
        return new PagedResponse<PublicDeckDto>(items, pagination.Page, pagination.PageSize, total);
    }

    public async Task<PublicDeckPreview> PreviewAsync(Guid id, CancellationToken ct) =>
        await repository.PreviewAsync(id, ct) ?? throw new NotFoundException("Public deck not found.");

    public Task<Guid> CloneAsync(Guid id, CancellationToken ct) => repository.CloneAsync(id, currentUser.GetRequiredUserId(), ct);
    public Task HideAsync(Guid id, CancellationToken ct) => repository.HideAsync(id, currentUser.GetRequiredUserId(), ct);
}

using LanguageLearning.Application.Common;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.Interfaces;

namespace LanguageLearning.Application.Services;

public sealed class SharingWriteService(ISharingWriteRepository repository, ICurrentUserService currentUser)
{
    public Task<Guid> CloneAsync(Guid id, CancellationToken ct) => repository.CloneAsync(id, currentUser.GetRequiredUserId(), ct);

    public Task HideAsync(Guid id, CancellationToken ct) => repository.HideAsync(id, currentUser.GetRequiredUserId(), ct);
}

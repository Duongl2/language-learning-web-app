
using LanguageLearning.Application.DTOs.Sync;

namespace LanguageLearning.Application.Interfaces;

public interface ISyncPullService
{
    Task<SyncPullResponse> PullAsync(
        string? cursor,
        int limit = 50,
        CancellationToken cancellationToken = default);
}

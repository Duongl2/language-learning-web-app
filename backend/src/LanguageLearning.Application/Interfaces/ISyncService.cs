
using LanguageLearning.Application.DTOs.Sync;

namespace LanguageLearning.Application.Interfaces;

public interface ISyncService
{
    Task<SyncPushResponse> PushAsync(
        SyncPushRequest request,
        CancellationToken cancellationToken = default);
}

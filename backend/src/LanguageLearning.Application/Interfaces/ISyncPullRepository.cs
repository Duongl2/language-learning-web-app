
using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Interfaces;

public interface ISyncPullRepository
{
    Task<List<Deck>> GetChangedDecksAsync(
        Guid userId,
        DateTime afterChangedAtUtc,
        Guid afterId,
        int take,
        CancellationToken cancellationToken = default);
}

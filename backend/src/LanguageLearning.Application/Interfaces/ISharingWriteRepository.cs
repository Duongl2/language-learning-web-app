namespace LanguageLearning.Application.Interfaces;

public interface ISharingWriteRepository
{
    Task<Guid> CloneAsync(Guid id, Guid ownerId, CancellationToken ct);
    Task HideAsync(Guid id, Guid actorId, CancellationToken ct);
}

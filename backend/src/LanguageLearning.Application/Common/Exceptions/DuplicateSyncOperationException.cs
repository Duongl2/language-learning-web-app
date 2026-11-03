
namespace LanguageLearning.Application.Common.Exceptions;

public sealed class DuplicateSyncOperationException : Exception
{
    public DuplicateSyncOperationException()
        : base("Sync operation has already been processed.")
    {
    }
}

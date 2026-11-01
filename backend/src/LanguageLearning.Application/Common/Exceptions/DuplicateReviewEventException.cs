
namespace LanguageLearning.Application.Common.Exceptions;

public sealed class DuplicateReviewEventException : Exception
{
    public DuplicateReviewEventException()
        : base("Review event has already been processed.")
    {
    }
}

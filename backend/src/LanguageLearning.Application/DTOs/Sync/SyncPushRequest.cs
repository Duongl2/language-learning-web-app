
using System.ComponentModel.DataAnnotations;

namespace LanguageLearning.Application.DTOs.Sync;

public sealed class SyncPushRequest
{
    [Required]
    [MinLength(1)]
    public List<SyncOperationRequest> Operations { get; set; } = [];
}

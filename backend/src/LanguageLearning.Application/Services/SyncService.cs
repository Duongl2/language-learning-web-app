
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Reviews;
using LanguageLearning.Application.DTOs.Sync;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Application.Services;

public sealed class SyncService : ISyncService
{
    private readonly ISyncRepository _syncRepository;
    private readonly IReviewService _reviewService;
    private readonly ICurrentUserService _currentUserService;

    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    public SyncService(
        ISyncRepository syncRepository,
        IReviewService reviewService,
        ICurrentUserService currentUserService)
    {
        _syncRepository = syncRepository;
        _reviewService = reviewService;
        _currentUserService = currentUserService;
    }

    public async Task<SyncPushResponse> PushAsync(
        SyncPushRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request?.Operations == null ||
            request.Operations.Count == 0 ||
            request.Operations.Count > 100)
        {
            throw new ArgumentException(
                "Operations must contain between 1 and 100 items.");
        }

        var userId = _currentUserService.GetRequiredUserId();
        var response = new SyncPushResponse();

        foreach (var operation in request.Operations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var result = await ProcessAsync(
                userId,
                operation,
                cancellationToken);

            response.Results.Add(result);
        }

        return response;
    }

    private async Task<SyncOperationResult> ProcessAsync(
        Guid userId,
        SyncOperationRequest operation,
        CancellationToken cancellationToken)
    {
        if (operation.OperationId == Guid.Empty ||
            operation.EntityId == Guid.Empty ||
            operation.OccurredAt == default)
        {
            return CreateResult(
                operation.OperationId,
                "rejected",
                "Invalid operation metadata.");
        }

        if (!string.Equals(
            operation.Type,
            "Review",
            StringComparison.OrdinalIgnoreCase))
        {
            return CreateResult(
                operation.OperationId,
                "rejected",
                "Unsupported operation type.");
        }

        if (operation.Payload.ValueKind != JsonValueKind.Object)
        {
            return CreateResult(
                operation.OperationId,
                "rejected",
                "Invalid review payload.");
        }

        ReviewPayload? payload;

        try
        {
            payload = JsonSerializer.Deserialize<ReviewPayload>(
                operation.Payload.GetRawText(),
                JsonOptions);
        }
        catch (JsonException)
        {
            return CreateResult(
                operation.OperationId,
                "rejected",
                "Invalid review payload.");
        }

        if (payload == null ||
            payload.ClientEventId == Guid.Empty ||
            payload.CardId != operation.EntityId ||
            payload.ReviewedAt == default ||
            string.IsNullOrWhiteSpace(payload.Rating) ||
            !Enum.TryParse<SrsRating>(
                payload.Rating,
                true,
                out var rating) ||
            !Enum.IsDefined(rating))
        {
            return CreateResult(
                operation.OperationId,
                "rejected",
                "Invalid review data.");
        }

        if (payload.AlgorithmVersion != "srs-v1")
        {
            return CreateResult(
                operation.OperationId,
                "conflict",
                "Unsupported SRS algorithm version.");
        }

        var canonicalPayload = JsonSerializer.Serialize(new
        {
            Type = "Review",
            operation.EntityId,
            payload.ClientEventId,
            payload.CardId,
            Rating = rating.ToString(),
            ReviewedAt = payload.ReviewedAt.ToUniversalTime(),
            payload.AlgorithmVersion,
            OccurredAt = operation.OccurredAt.ToUniversalTime()
        });

        var hash = Convert.ToHexString(
            SHA256.HashData(
                Encoding.UTF8.GetBytes(canonicalPayload)));

        try
        {
            return await _syncRepository.ExecuteInTransactionAsync(
                async token =>
                {
                    var existing =
                        await _syncRepository.GetByClientOperationIdAsync(
                            userId,
                            operation.OperationId,
                            token);

                    if (existing != null)
                    {
                        return CompareExisting(
                            existing,
                            operation,
                            hash);
                    }

                    var ledger = new SyncOperation
                    {
                        UserId = userId,
                        ClientOperationId = operation.OperationId,
                        OperationType = "Review",
                        EntityId = operation.EntityId,
                        PayloadHash = hash,
                        Status = "Accepted"
                    };

                    await _syncRepository.AddAsync(ledger, token);

                    await _syncRepository.SaveChangesAsync(token);

                    var review =
                        await _reviewService.SubmitReviewAsync(
                            new SubmitReviewRequest
                            {
                                CardId = payload.CardId,
                                ClientEventId = payload.ClientEventId,
                                Rating = rating
                            },
                            token);

                    return CreateResult(
                        operation.OperationId,
                        review.AlreadyProcessed
                            ? "duplicate"
                            : "accepted");
                },
                cancellationToken);
        }
        catch (DuplicateSyncOperationException)
        {
            var existing =
                await _syncRepository.GetByClientOperationIdAsync(
                    userId,
                    operation.OperationId,
                    cancellationToken);

            if (existing == null)
            {
                throw;
            }

            return CompareExisting(
                existing,
                operation,
                hash);
        }
        catch (ConflictException exception)
        {
            return CreateResult(
                operation.OperationId,
                "conflict",
                exception.Message);
        }
        catch (NotFoundException exception)
        {
            return CreateResult(
                operation.OperationId,
                "rejected",
                exception.Message);
        }
        catch (ArgumentException exception)
        {
            return CreateResult(
                operation.OperationId,
                "rejected",
                exception.Message);
        }
    }

    private static SyncOperationResult CompareExisting(
        SyncOperation existing,
        SyncOperationRequest operation,
        string hash)
    {
        var isSameOperation =
            existing.OperationType == "Review" &&
            existing.EntityId == operation.EntityId &&
            existing.PayloadHash == hash;

        return CreateResult(
            operation.OperationId,
            isSameOperation ? "duplicate" : "conflict",
            isSameOperation
                ? "Operation already processed."
                : "Operation ID was used with different data.");
    }

    private static SyncOperationResult CreateResult(
        Guid operationId,
        string status,
        string? message = null)
    {
        return new SyncOperationResult
        {
            OperationId = operationId,
            Status = status,
            Message = message
        };
    }

    private sealed class ReviewPayload
    {
        public Guid ClientEventId { get; set; }

        public Guid CardId { get; set; }

        public string Rating { get; set; } = string.Empty;

        public DateTimeOffset ReviewedAt { get; set; }

        public string AlgorithmVersion { get; set; } = string.Empty;
    }
}

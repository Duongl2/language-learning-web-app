using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace LanguageLearning.Application.DTOs.Learning;

public sealed record PublicDeckDto(Guid Id, Guid LanguageId, string Title, string Description,
    int CardCount, IReadOnlyList<string> Tags);

public sealed record CardPreview(string Front, string Back);

public sealed record PublicDeckPreview(PublicDeckDto Deck, IReadOnlyList<CardPreview> Cards);

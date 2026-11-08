using System.Text.Json;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Tests;

public sealed class QuizScoringTests
{
    [Fact]
    public void MultipleChoice_OnlyAcceptsAnExistingCorrectOption()
    {
        var correct = new QuestionOption { IsCorrect = true };
        var incorrect = new QuestionOption { IsCorrect = false };
        var question = new Question { Options = new[] { correct, incorrect } };
        Assert.True(QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(correct.Id)));
        Assert.False(QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(incorrect.Id)));
        Assert.False(QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(Guid.NewGuid())));
        Assert.False(QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(1)));
    }


    [Theory]
    [InlineData(" hello ", true)]
    [InlineData("HI", true)]
    [InlineData("bye", false)]
    public void FillInBlank_UsesConfiguredAlternatives(string answer, bool expected)
    {
        var question = new Question { Type = QuestionType.FillInBlank, ConfigJson = "{\"answers\":[\"Hello\",\"Hi\"]}" };
        Assert.Equal(expected, QuizService.IsCorrect(question, JsonSerializer.SerializeToElement(answer)));
    }


    [Theory]
    [InlineData("{\"a\":\"2\",\"b\":\"1\"}", true)]
    [InlineData("{\"a\":\"1\",\"b\":\"2\"}", false)]
    [InlineData("{\"a\":\"2\"}", false)]
    [InlineData("{\"a\":\"2\",\"a\":\"2\"}", false)]
    public void Matching_RequiresTheCompleteExactMapping(string answer, bool expected)
    {
        var question = new Question { Type = QuestionType.Matching, ConfigJson = "{\"pairs\":{\"a\":\"2\",\"b\":\"1\"}}" };
        Assert.Equal(expected, QuizService.IsCorrect(question, JsonSerializer.Deserialize<JsonElement>(answer)));
    }


}

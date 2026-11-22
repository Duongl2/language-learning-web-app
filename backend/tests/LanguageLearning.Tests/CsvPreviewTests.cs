using System.Text.Json;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Tests;

public sealed class CsvPreviewTests
{
    [Fact]
    public void Csv_ParsesEscapesMultilineUnicodeAndReportsPhysicalLineNumbers()
    {
        var preview = CsvPreviewService.Preview("\uFEFFfront,back\r\n\"Xin, chào\",\"Hello \"\"friend\"\"\"\r\n\"two\nlines\",ok\r\n,missing\r\n");
        Assert.Equal(2, preview.ValidCount);
        Assert.Equal(1, preview.InvalidCount);
        Assert.Equal("Hello \"friend\"", preview.Rows[0].Back);
        Assert.Equal("two\nlines", preview.Rows[1].Front);
        Assert.Equal(5, preview.Rows[2].Line);
    }


    [Theory]
    [InlineData("front,back\na,b\na,b", 1)]
    [InlineData("front,back\na,b,c", 1)]
    public void Csv_ReportsInvalidAndDuplicateRows(string content, int invalid) => Assert.Equal(invalid, CsvPreviewService.Preview(content).InvalidCount);


    [Theory]
    [InlineData("term,meaning\na,b")]
    [InlineData("front,back\n\"unterminated,b")]
    [InlineData("front,back\na\"b,c")]
    [InlineData("front,back\n\"a\"x,b")]
    [InlineData("front,back\n\uFFFD,b")]
    public void Csv_RejectsMalformedInput(string content) => Assert.Throws<ArgumentException>(() => CsvPreviewService.Preview(content));


}

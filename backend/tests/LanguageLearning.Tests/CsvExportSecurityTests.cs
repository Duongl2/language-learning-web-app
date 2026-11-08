using System.Text.Json;
using LanguageLearning.Application.Services;
using LanguageLearning.Domain.Entities;
using LanguageLearning.Domain.Enums;

namespace LanguageLearning.Tests;

public sealed class CsvExportSecurityTests
{
    [Theory]
    [InlineData("=SUM(A1)")]
    [InlineData("  +cmd")]
    [InlineData("-1")]
    [InlineData("@function")]
    [InlineData("\tformula")]
    [InlineData("\rformula")]
    public void Csv_ExportNeutralizesFormulaInjection(string value) => Assert.StartsWith("\"'", CsvService.Escape(value));


}

using System.Text;
using LanguageLearning.Application.Common.Exceptions;
using LanguageLearning.Application.DTOs.Learning;
using LanguageLearning.Application.Interfaces;
using LanguageLearning.Domain.Entities;

namespace LanguageLearning.Application.Services;

public static class CsvPreviewService
{
    public const string Template = "front,back\r\nHello,Xin chào\r\n";

    public static CsvPreview Preview(string content)
    {
        if (string.IsNullOrWhiteSpace(content) || Encoding.UTF8.GetByteCount(content) > 2_000_000 || content.Contains('\uFFFD') || content.Contains('\0'))
            throw new ArgumentException("CSV must contain valid UTF-8 text under 2 MB.");
        var records = Parse(content.TrimStart('\uFEFF'));
        if (records.Count == 0 || !records[0].Values.SequenceEqual(new[] { "front", "back" }, StringComparer.OrdinalIgnoreCase))
            throw new ArgumentException("CSV header must be front,back.");
        if (records.Count > 5001) throw new ArgumentException("Import is limited to 5000 cards.");
        var seen = new HashSet<(string, string)>();
        var rows = records.Skip(1).Select(record =>
        {
            var front = record.Values.ElementAtOrDefault(0)?.Trim() ?? "";
            var back = record.Values.ElementAtOrDefault(1)?.Trim() ?? "";
            var error = record.Values.Count != 2 ? "Expected exactly two columns." :
                front.Length == 0 || back.Length == 0 ? "Front and back are required." :
                front.Length > 10000 || back.Length > 10000 ? "Card text exceeds 10000 characters." :
                !seen.Add((front, back)) ? "Duplicate card in CSV." : null;
            return new CsvRow(record.Line, front, back, error);
        }).ToList();
        return new CsvPreview(rows, rows.Count(x => x.Error == null), rows.Count(x => x.Error != null));
    }

    private static List<(int Line, List<string> Values)> Parse(string content)
    {
        var records = new List<(int, List<string>)>();
        var fields = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        var closed = false;
        var line = 1;
        var start = 1;
        for (var i = 0; i < content.Length; i++)
        {
            var ch = content[i];
            if (quoted)
            {
                if (ch == '"')
                {
                    if (i + 1 < content.Length && content[i + 1] == '"') { field.Append('"'); i++; }
                    else { quoted = false; closed = true; }
                }
                else { field.Append(ch); if (ch == '\n') line++; }
            }
            else if (ch == '"')
            {
                if (field.Length > 0 || closed) throw new ArgumentException($"Invalid CSV quoting at line {line}.");
                quoted = true;
            }
            else if (ch == ',' || ch == '\r' || ch == '\n')
            {
                fields.Add(field.ToString()); field.Clear(); closed = false;
                if (ch != ',')
                {
                    records.Add((start, fields)); fields = new();
                    if (ch == '\r' && i + 1 < content.Length && content[i + 1] == '\n') i++;
                    line++; start = line;
                }
            }
            else
            {
                if (closed) throw new ArgumentException($"Unexpected text after closing quote at line {line}.");
                field.Append(ch);
            }
        }
        if (quoted) throw new ArgumentException($"Unclosed CSV quote at line {start}.");
        if (field.Length > 0 || fields.Count > 0 || closed)
        {
            fields.Add(field.ToString()); records.Add((start, fields));
        }
        return records;
    }
}

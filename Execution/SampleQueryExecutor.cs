using System.Text.RegularExpressions;
using KazmonQueryBuilder.Core;

namespace KazmonQueryBuilder.Execution;

public class SampleQueryExecutor : IQueryExecutor
{
    public Task<QueryResult> ExecuteAsync(string kql)
    {
        var tableName = ExtractTableName(kql);

        if (tableName is null || !SampleData.Tables.TryGetValue(tableName, out var data))
        {
            var known = string.Join(", ", SampleData.Tables.Keys);
            return Task.FromResult(new QueryResult(
                ["Info"],
                [[$"No sample data for table '{tableName}'. Available: {known}"]]));
        }

        var (columns, rows) = data;
        var result = rows.Select(r => r.ToArray()).ToList();

        result = ApplyTop(kql, result);
        result = ApplyWhereContains(kql, columns, result);
        (columns, result) = ApplyProject(kql, columns, result);

        return Task.FromResult(new QueryResult(columns, result));
    }

    public static string? ExtractTableName(string kql)
    {
        var match = Regex.Match(kql.TrimStart(), @"^(\w+)");
        return match.Success ? match.Groups[1].Value : null;
    }

    public static List<string[]> ApplyTop(string kql, List<string[]> rows)
    {
        var match = Regex.Match(kql, @"\b(?:top|limit)\s+(\d+)", RegexOptions.IgnoreCase);
        if (match.Success && int.TryParse(match.Groups[1].Value, out int n))
            return rows.Take(n).ToList();
        return rows;
    }

    public static List<string[]> ApplyWhereContains(string kql, string[] columns, List<string[]> rows)
    {
        var matches = Regex.Matches(kql,
            @"\bwhere\b.+?(\w+)\s*(?:contains|==|=~|has)\s*[""']([^""']+)[""']",
            RegexOptions.IgnoreCase);

        foreach (Match m in matches)
        {
            var col = m.Groups[1].Value;
            var val = m.Groups[2].Value;
            int idx = Array.FindIndex(columns, c => c.Equals(col, StringComparison.OrdinalIgnoreCase));
            if (idx >= 0)
                rows = rows.Where(r => r[idx].Contains(val, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return rows;
    }

    public static (string[] columns, List<string[]> rows) ApplyProject(string kql, string[] columns, List<string[]> rows)
    {
        var match = Regex.Match(kql, @"\bproject\b\s+([^\r\n|]+)", RegexOptions.IgnoreCase);
        if (!match.Success) return (columns, rows);

        var selected = match.Groups[1].Value
            .Split(',')
            .Select(c => c.Trim())
            .Where(c => !string.IsNullOrEmpty(c))
            .ToArray();

        var indices = selected
            .Select(s => Array.FindIndex(columns, c => c.Equals(s, StringComparison.OrdinalIgnoreCase)))
            .Where(i => i >= 0)
            .ToArray();

        if (indices.Length == 0) return (columns, rows);

        var newColumns = indices.Select(i => columns[i]).ToArray();
        var newRows = rows.Select(r => indices.Select(i => r[i]).ToArray()).ToList();
        return (newColumns, newRows);
    }
}

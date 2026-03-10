namespace KazmonQueryBuilder.Core;

public record QueryResult(string[] Columns, List<string[]> Rows);

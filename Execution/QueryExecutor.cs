using Azure.Monitor.Query;
using KazmonQueryBuilder.Core;

namespace KazmonQueryBuilder.Execution;

public class QueryExecutor(LogsQueryClient client, string workspaceId) : IQueryExecutor
{
    public async Task<QueryResult> ExecuteAsync(string kql)
    {
        var response = await client.QueryWorkspaceAsync(
            workspaceId,
            kql,
            new QueryTimeRange(TimeSpan.FromDays(1)));

        var table = response.Value.Table;
        var columns = table.Columns.Select(c => c.Name).ToArray();
        var rows = table.Rows
            .Select(r => Enumerable.Range(0, columns.Length)
                .Select(i => r[i]?.ToString() ?? "")
                .ToArray())
            .ToList();

        return new QueryResult(columns, rows);
    }
}

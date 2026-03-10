using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;

namespace KazmonQueryBuilder;

class QueryExecutor(LogsQueryClient client, string workspaceId)
{
    public async Task<LogsTable> ExecuteAsync(string kql) =>
        (await client.QueryWorkspaceAsync(
            workspaceId,
            kql,
            new QueryTimeRange(TimeSpan.FromDays(1)))).Value.Table;
}

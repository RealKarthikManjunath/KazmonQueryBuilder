namespace KazmonQueryBuilder;

interface IQueryExecutor
{
    Task<QueryResult> ExecuteAsync(string kql);
}

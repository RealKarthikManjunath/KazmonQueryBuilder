namespace KazmonQueryBuilder.Core;

public interface IQueryExecutor
{
    Task<QueryResult> ExecuteAsync(string kql);
}

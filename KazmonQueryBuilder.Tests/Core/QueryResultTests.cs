using KazmonQueryBuilder.Core;

namespace KazmonQueryBuilder.Tests.Core;

public class QueryResultTests
{
    [Fact]
    public void QueryResult_StoresColumnsAndRows()
    {
        var columns = new[] { "Col1", "Col2" };
        var rows = new List<string[]> { new[] { "A", "B" }, new[] { "C", "D" } };

        var result = new QueryResult(columns, rows);

        Assert.Equal(columns, result.Columns);
        Assert.Equal(rows, result.Rows);
    }

    [Fact]
    public void QueryResult_EmptyRows_HasZeroCount()
    {
        var result = new QueryResult(["Col1"], []);

        Assert.Empty(result.Rows);
    }

    [Fact]
    public void QueryResult_ColumnsMatchRowWidth()
    {
        var result = new QueryResult(
            ["A", "B", "C"],
            [["1", "2", "3"], ["4", "5", "6"]]);

        foreach (var row in result.Rows)
            Assert.Equal(result.Columns.Length, row.Length);
    }
}

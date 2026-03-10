using KazmonQueryBuilder.Execution;

namespace KazmonQueryBuilder.Tests.Execution;

public class SampleQueryExecutorTests
{
    private readonly SampleQueryExecutor _executor = new();

    // ── ExecuteAsync ────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_KnownTable_ReturnsRows()
    {
        var result = await _executor.ExecuteAsync("SecurityEvent");

        Assert.NotEmpty(result.Rows);
        Assert.NotEmpty(result.Columns);
    }

    [Fact]
    public async Task ExecuteAsync_UnknownTable_ReturnsInfoRow()
    {
        var result = await _executor.ExecuteAsync("NonExistentTable");

        Assert.Single(result.Columns);
        Assert.Equal("Info", result.Columns[0]);
        Assert.Single(result.Rows);
        Assert.Contains("NonExistentTable", result.Rows[0][0]);
    }

    [Fact]
    public async Task ExecuteAsync_AllSampleTables_ReturnRows()
    {
        foreach (var tableName in SampleData.Tables.Keys)
        {
            var result = await _executor.ExecuteAsync(tableName);
            Assert.True(result.Rows.Count > 0, $"Table '{tableName}' returned no rows");
        }
    }

    // ── ApplyTop ────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("SecurityEvent | top 3",  3)]
    [InlineData("SecurityEvent | top 1",  1)]
    [InlineData("SecurityEvent | limit 5", 5)]
    public async Task ExecuteAsync_WithTopOrLimit_RowsAreCapped(string kql, int expected)
    {
        var result = await _executor.ExecuteAsync(kql);

        Assert.Equal(expected, result.Rows.Count);
    }

    [Fact]
    public async Task ExecuteAsync_TopLargerThanData_ReturnsAllRows()
    {
        var all = await _executor.ExecuteAsync("Heartbeat");
        var capped = await _executor.ExecuteAsync("Heartbeat | top 10000");

        Assert.Equal(all.Rows.Count, capped.Rows.Count);
    }

    // ── ApplyWhereContains ──────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_WhereContains_FiltersRows()
    {
        var result = await _executor.ExecuteAsync(
            """SecurityEvent | where Activity contains "failed" """);

        Assert.All(result.Rows, row =>
        {
            var activityIdx = Array.IndexOf(result.Columns, "Activity");
            Assert.Contains("failed", row[activityIdx], StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public async Task ExecuteAsync_WhereEquals_FiltersRows()
    {
        var result = await _executor.ExecuteAsync(
            """SecurityEvent | where EventID == "4624" """);

        var eventIdIdx = Array.IndexOf(result.Columns, "EventID");
        Assert.All(result.Rows, row => Assert.Equal("4624", row[eventIdIdx]));
    }

    [Fact]
    public async Task ExecuteAsync_WhereNoMatch_ReturnsEmpty()
    {
        var result = await _executor.ExecuteAsync(
            """SecurityEvent | where Computer == "DOES-NOT-EXIST" """);

        Assert.Empty(result.Rows);
    }

    // ── ApplyProject ────────────────────────────────────────────────────────

    [Fact]
    public async Task ExecuteAsync_Project_ReducesColumns()
    {
        var result = await _executor.ExecuteAsync(
            "SecurityEvent | project Computer, EventID");

        Assert.Equal(2, result.Columns.Length);
        Assert.Contains("Computer", result.Columns);
        Assert.Contains("EventID", result.Columns);
    }

    [Fact]
    public async Task ExecuteAsync_Project_EachRowHasCorrectWidth()
    {
        var result = await _executor.ExecuteAsync(
            "SecurityEvent | project Computer, Account, EventID");

        Assert.All(result.Rows, row => Assert.Equal(3, row.Length));
    }

    [Fact]
    public async Task ExecuteAsync_ProjectUnknownColumn_IgnoresSilently()
    {
        var result = await _executor.ExecuteAsync(
            "SecurityEvent | project Computer, ThisColumnDoesNotExist");

        // Only the valid column survives
        Assert.Equal(["Computer"], result.Columns);
    }

    // ── ExtractTableName ────────────────────────────────────────────────────

    [Theory]
    [InlineData("SecurityEvent | where EventID == '4624'", "SecurityEvent")]
    [InlineData("SigninLogs | top 10",                      "SigninLogs")]
    [InlineData("Heartbeat",                                "Heartbeat")]
    [InlineData("  Perf | project Computer",               "Perf")]
    public void ExtractTableName_ReturnsFirstToken(string kql, string expected)
    {
        Assert.Equal(expected, SampleQueryExecutor.ExtractTableName(kql));
    }

    [Fact]
    public void ExtractTableName_EmptyString_ReturnsNull()
    {
        Assert.Null(SampleQueryExecutor.ExtractTableName(""));
    }
}

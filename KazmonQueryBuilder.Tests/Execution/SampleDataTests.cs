using KazmonQueryBuilder.Execution;

namespace KazmonQueryBuilder.Tests.Execution;

public class SampleDataTests
{
    [Theory]
    [InlineData("SecurityEvent")]
    [InlineData("SigninLogs")]
    [InlineData("Heartbeat")]
    [InlineData("Perf")]
    [InlineData("AzureActivity")]
    [InlineData("DnsEvents")]
    public void Tables_EachKnownTable_HasRowsAndColumns(string tableName)
    {
        Assert.True(SampleData.Tables.ContainsKey(tableName));

        var (columns, rows) = SampleData.Tables[tableName];

        Assert.NotEmpty(columns);
        Assert.NotEmpty(rows);
    }

    [Fact]
    public void Tables_AllRows_MatchColumnCount()
    {
        foreach (var (tableName, (columns, rows)) in SampleData.Tables)
        {
            foreach (var row in rows)
                Assert.True(row.Length == columns.Length,
                    $"Table '{tableName}': row has {row.Length} values but {columns.Length} columns");
        }
    }

    [Fact]
    public void Tables_SecurityEvent_ContainsExpectedColumns()
    {
        var (columns, _) = SampleData.Tables["SecurityEvent"];
        Assert.Contains("EventID",   columns);
        Assert.Contains("Computer",  columns);
        Assert.Contains("Account",   columns);
        Assert.Contains("Activity",  columns);
    }

    [Fact]
    public void Tables_SigninLogs_ContainsExpectedColumns()
    {
        var (columns, _) = SampleData.Tables["SigninLogs"];
        Assert.Contains("UserPrincipalName", columns);
        Assert.Contains("ResultType",        columns);
        Assert.Contains("RiskLevel",         columns);
        Assert.Contains("IPAddress",         columns);
    }

    [Fact]
    public void Tables_SecurityEvent_HasBothSuccessAndFailureEvents()
    {
        var (columns, rows) = SampleData.Tables["SecurityEvent"];
        int eventIdIdx = Array.IndexOf(columns, "EventID");

        var eventIds = rows.Select(r => r[eventIdIdx]).ToHashSet();

        Assert.Contains("4624", eventIds); // successful logon
        Assert.Contains("4625", eventIds); // failed logon
    }

    [Fact]
    public void Tables_SigninLogs_HasHighRiskEntries()
    {
        var (columns, rows) = SampleData.Tables["SigninLogs"];
        int riskIdx = Array.IndexOf(columns, "RiskLevel");

        Assert.Contains(rows, r => r[riskIdx].Equals("high", StringComparison.OrdinalIgnoreCase));
    }
}

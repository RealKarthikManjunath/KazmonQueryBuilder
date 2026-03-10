using KazmonQueryBuilder.Display;

namespace KazmonQueryBuilder.Tests.Display;

public class TableCatalogTests
{
    [Fact]
    public void Sections_ContainsAllExpectedCategories()
    {
        var expected = new[]
        {
            "Azure Monitor / Log Analytics",
            "Microsoft Entra ID / Sign-in",
            "Microsoft Sentinel / Security",
            "Network",
            "Containers & Apps",
            "Azure Resources",
        };

        foreach (var category in expected)
            Assert.True(TableCatalog.Sections.ContainsKey(category),
                $"Missing category: '{category}'");
    }

    [Fact]
    public void Sections_EachCategory_HasAtLeastOneTable()
    {
        foreach (var (category, tables) in TableCatalog.Sections)
            Assert.True(tables.Length > 0, $"Category '{category}' has no tables");
    }

    [Theory]
    [InlineData("SecurityEvent")]
    [InlineData("SigninLogs")]
    [InlineData("Heartbeat")]
    [InlineData("Perf")]
    [InlineData("AzureActivity")]
    [InlineData("DnsEvents")]
    [InlineData("AuditLogs")]
    [InlineData("KeyVaultLogs")]
    public void Sections_ContainsExpectedTable(string tableName)
    {
        var allTables = TableCatalog.Sections.Values.SelectMany(t => t);
        Assert.Contains(tableName, allTables);
    }

    [Fact]
    public void Sections_NoTableAppearsInMultipleCategories()
    {
        var allTables = TableCatalog.Sections.Values.SelectMany(t => t).ToList();
        var duplicates = allTables.GroupBy(t => t).Where(g => g.Count() > 1).Select(g => g.Key);

        Assert.Empty(duplicates);
    }

    [Fact]
    public void SystemPromptSection_IsNotEmpty()
    {
        Assert.False(string.IsNullOrWhiteSpace(TableCatalog.SystemPromptSection));
    }

    [Theory]
    [InlineData("SecurityEvent")]
    [InlineData("SigninLogs")]
    [InlineData("Heartbeat")]
    [InlineData("TimeGenerated")]
    [InlineData("UserPrincipalName")]
    [InlineData("EventID")]
    public void SystemPromptSection_ContainsKeyTerms(string term)
    {
        Assert.Contains(term, TableCatalog.SystemPromptSection, StringComparison.OrdinalIgnoreCase);
    }
}

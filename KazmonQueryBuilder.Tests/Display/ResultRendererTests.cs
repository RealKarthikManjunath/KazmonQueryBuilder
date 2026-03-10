using KazmonQueryBuilder.Core;
using KazmonQueryBuilder.Display;

namespace KazmonQueryBuilder.Tests.Display;

public class ResultRendererTests
{
    // Redirect stdout so we can assert on rendered output
    private static string Capture(Action action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        action();
        Console.SetOut(original);
        return writer.ToString();
    }

    [Fact]
    public void Render_EmptyRows_PrintsNoResultsMessage()
    {
        var result = new QueryResult(["Col1"], []);

        var output = Capture(() => ResultRenderer.Render(result));

        Assert.Contains("No results returned", output);
    }

    [Fact]
    public void Render_WithRows_PrintsColumnHeaders()
    {
        var result = new QueryResult(
            ["Computer", "EventID"],
            [["WIN-DC01", "4624"]]);

        var output = Capture(() => ResultRenderer.Render(result));

        Assert.Contains("Computer", output);
        Assert.Contains("EventID", output);
    }

    [Fact]
    public void Render_WithRows_PrintsRowValues()
    {
        var result = new QueryResult(
            ["Computer", "EventID"],
            [["WIN-DC01", "4624"]]);

        var output = Capture(() => ResultRenderer.Render(result));

        Assert.Contains("WIN-DC01", output);
        Assert.Contains("4624", output);
    }

    [Fact]
    public void Render_WithRows_PrintsRowCount()
    {
        var result = new QueryResult(
            ["Computer"],
            [["A"], ["B"], ["C"]]);

        var output = Capture(() => ResultRenderer.Render(result));

        Assert.Contains("3 row(s) returned", output);
    }

    [Fact]
    public void Truncate_ShortString_ReturnsUnchanged()
    {
        Assert.Equal("hello", ResultRenderer.Truncate("hello", 10));
    }

    [Fact]
    public void Truncate_LongString_TruncatesWithEllipsis()
    {
        var result = ResultRenderer.Truncate("abcdefgh", 5);

        Assert.Equal(5, result.Length);
        Assert.EndsWith("…", result);
    }

    [Fact]
    public void Truncate_ExactLength_ReturnsUnchanged()
    {
        Assert.Equal("abcde", ResultRenderer.Truncate("abcde", 5));
    }
}

namespace KazmonQueryBuilder;

static class ResultRenderer
{
    public static void Render(QueryResult result)
    {
        if (result.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  No results returned.");
            Console.ResetColor();
            Console.WriteLine();
            return;
        }

        var widths = CalculateWidths(result);

        PrintHeader(result.Columns, widths);

        int rowCount = 0;
        foreach (var row in result.Rows)
        {
            Console.ForegroundColor = rowCount % 2 == 0 ? ConsoleColor.White : ConsoleColor.Gray;
            PrintRow(row, widths);
            rowCount++;
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\n  {rowCount} row(s) returned.");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static int[] CalculateWidths(QueryResult result)
    {
        var widths = result.Columns
            .Select(c => Math.Min(50, Math.Max(c.Length, 10)))
            .ToArray();

        foreach (var row in result.Rows.Take(20))
            for (int i = 0; i < result.Columns.Length; i++)
                widths[i] = Math.Min(50, Math.Max(widths[i], (row[i] ?? "").Length));

        return widths;
    }

    private static void PrintHeader(string[] columns, int[] widths)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        PrintRow(columns, widths);
        Console.WriteLine(string.Join("─┼─", widths.Select(w => new string('─', w))));
        Console.ResetColor();
    }

    private static void PrintRow(string[] values, int[] widths)
    {
        var cells = values.Select((v, i) => Truncate(v ?? "", widths[i]).PadRight(widths[i]));
        Console.WriteLine(string.Join(" │ ", cells));
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..(max - 1)] + "…";
}

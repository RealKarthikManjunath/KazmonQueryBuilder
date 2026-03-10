using Azure.Monitor.Query.Models;

namespace KazmonQueryBuilder;

static class ResultRenderer
{
    public static void Render(LogsTable table)
    {
        if (table.Rows.Count == 0)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("  No results returned.");
            Console.ResetColor();
            Console.WriteLine();
            return;
        }

        var columns = table.Columns;
        var widths = CalculateWidths(table);

        PrintHeader(columns.Select(c => c.Name).ToArray(), widths);

        int rowCount = 0;
        foreach (var row in table.Rows)
        {
            var values = Enumerable.Range(0, columns.Count)
                .Select(i => Truncate(row[i]?.ToString() ?? "", widths[i]))
                .ToArray();

            Console.ForegroundColor = rowCount % 2 == 0 ? ConsoleColor.White : ConsoleColor.Gray;
            PrintRow(values, widths);
            rowCount++;
        }

        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"\n  {rowCount} row(s) returned.");
        Console.ResetColor();
        Console.WriteLine();
    }

    private static int[] CalculateWidths(LogsTable table)
    {
        var widths = table.Columns
            .Select(c => Math.Min(50, Math.Max(c.Name.Length, 10)))
            .ToArray();

        foreach (var row in table.Rows.Take(20))
            for (int i = 0; i < table.Columns.Count; i++)
                widths[i] = Math.Min(50, Math.Max(widths[i], (row[i]?.ToString() ?? "").Length));

        return widths;
    }

    private static void PrintHeader(string[] columnNames, int[] widths)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        PrintRow(columnNames, widths);
        Console.WriteLine(string.Join("─┼─", widths.Select(w => new string('─', w))));
        Console.ResetColor();
    }

    private static void PrintRow(string[] values, int[] widths)
    {
        var cells = values.Select((v, i) => Truncate(v, widths[i]).PadRight(widths[i]));
        Console.WriteLine(string.Join(" │ ", cells));
    }

    private static string Truncate(string s, int max) =>
        s.Length <= max ? s : s[..(max - 1)] + "…";
}

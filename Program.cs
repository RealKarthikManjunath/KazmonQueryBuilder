using Anthropic;
using Azure.Identity;
using Azure.Monitor.Query;
using KazmonQueryBuilder.Core;
using KazmonQueryBuilder.Display;
using KazmonQueryBuilder.Execution;
using KazmonQueryBuilder.Translation;

// --- Validate environment ---
var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error: ANTHROPIC_API_KEY is not set.");
    Console.WriteLine("  export ANTHROPIC_API_KEY=sk-ant-...");
    Console.ResetColor();
    return;
}

var workspaceId = Environment.GetEnvironmentVariable("LOG_ANALYTICS_WORKSPACE_ID");
bool usingAzure = !string.IsNullOrEmpty(workspaceId);

// --- Services ---
var translator = new KqlTranslator(new AnthropicClient { ApiKey = apiKey });
IQueryExecutor executor = usingAzure
    ? new QueryExecutor(new LogsQueryClient(new DefaultAzureCredential()), workspaceId!)
    : new SampleQueryExecutor();

// --- Banner ---
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║        KazmonQueryBuilder — KQL Assistant           ║");
Console.WriteLine("║  Type your question in plain English.               ║");
Console.WriteLine("║  Commands: /tables  /help  exit                     ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝");
Console.ForegroundColor = usingAzure ? ConsoleColor.DarkGreen : ConsoleColor.DarkYellow;
Console.WriteLine(usingAzure
    ? "  [Connected to Azure Log Analytics workspace]"
    : "  [Using built-in sample data — set LOG_ANALYTICS_WORKSPACE_ID for real data]");
Console.ResetColor();
Console.WriteLine();

// --- REPL ---
while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("You > ");
    Console.ResetColor();

    var input = Console.ReadLine();
    if (input is null) break;
    input = input.Trim();
    if (string.IsNullOrEmpty(input)) continue;

    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    if (input.Equals("/help", StringComparison.OrdinalIgnoreCase))
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("  /tables  — list all available tables");
        Console.WriteLine("  /help    — show this help");
        Console.WriteLine("  exit     — quit the app");
        Console.WriteLine("  anything else — translate to KQL and execute");
        Console.ResetColor();
        Console.WriteLine();
        continue;
    }

    if (input.Equals("/tables", StringComparison.OrdinalIgnoreCase) ||
        input.Contains("available table", StringComparison.OrdinalIgnoreCase) ||
        input.Contains("list table", StringComparison.OrdinalIgnoreCase) ||
        input.Contains("show table", StringComparison.OrdinalIgnoreCase))
    {
        TableCatalog.Print();
        continue;
    }

    // Step 1: Translate
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Translating...");
    Console.ResetColor();

    string kql;
    try
    {
        kql = await translator.TranslateAsync(input);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Translation error: {ex.Message}");
        Console.ResetColor();
        continue;
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\nGenerated KQL:");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(kql);
    Console.ResetColor();
    Console.WriteLine();

    // Step 2: Execute
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine(usingAzure ? "Executing against workspace..." : "Executing against sample data...");
    Console.ResetColor();

    try
    {
        var table = await executor.ExecuteAsync(kql);
        ResultRenderer.Render(table);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Execution error: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine();
    }
}

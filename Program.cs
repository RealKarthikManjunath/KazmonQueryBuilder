using Anthropic;
using Anthropic.Models.Messages;

const string SystemPrompt = """
You are an expert in Kusto Query Language (KQL), used in Azure Data Explorer, Azure Monitor, Microsoft Sentinel, and Log Analytics.

Your job is to translate natural language queries into precise, correct KQL.

Guidelines:
- Output ONLY the KQL query — no explanations, no markdown fences, no extra text.
- Use common KQL table names where appropriate (e.g., Heartbeat, AzureActivity, SecurityEvent, SigninLogs, Perf, Event, Syslog, ContainerLog, etc.) unless the user specifies a table.
- Use proper KQL operators: where, summarize, project, extend, join, union, render, bin(), ago(), now(), count(), avg(), sum(), max(), min(), dcount(), etc.
- Prefer readable, well-formatted multi-line KQL.
- If the intent is ambiguous, make a reasonable assumption and produce a valid query.
""";

AnthropicClient client = new();

Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔══════════════════════════════════════════════╗");
Console.WriteLine("║        KazmonQueryBuilder — KQL Assistant     ║");
Console.WriteLine("║  Type your question in plain English.         ║");
Console.WriteLine("║  Type 'exit' or 'quit' to quit.               ║");
Console.WriteLine("╚══════════════════════════════════════════════╝");
Console.ResetColor();
Console.WriteLine();

while (true)
{
    Console.ForegroundColor = ConsoleColor.Green;
    Console.Write("You > ");
    Console.ResetColor();

    var input = Console.ReadLine()?.Trim();

    if (string.IsNullOrEmpty(input)) continue;
    if (input.Equals("exit", StringComparison.OrdinalIgnoreCase) ||
        input.Equals("quit", StringComparison.OrdinalIgnoreCase))
    {
        Console.WriteLine("Goodbye!");
        break;
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.Write("KQL > ");
    Console.ResetColor();

    try
    {
        var parameters = new MessageCreateParams
        {
            Model = Model.ClaudeOpus4_6,
            MaxTokens = 2048,
            Thinking = new ThinkingConfigAdaptive(),
            System = SystemPrompt,
            Messages = [new() { Role = Role.User, Content = input }]
        };

        await foreach (var streamEvent in client.Messages.CreateStreaming(parameters))
        {
            if (streamEvent.TryPickContentBlockDelta(out var delta) &&
                delta.Delta.TryPickText(out var text))
            {
                Console.Write(text.Text);
            }
        }

        Console.WriteLine();
        Console.WriteLine();
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Error: {ex.Message}");
        Console.ResetColor();
    }
}

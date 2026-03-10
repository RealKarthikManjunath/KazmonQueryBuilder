using Anthropic;
using Anthropic.Models.Messages;
using Azure.Identity;
using Azure.Monitor.Query;
using Azure.Monitor.Query.Models;

const string SystemPrompt = """
You are an expert in Kusto Query Language (KQL), used in Azure Data Explorer, Azure Monitor, Microsoft Sentinel, and Log Analytics.

Your job is to translate natural language queries into precise, correct KQL.

Guidelines:
- Output ONLY the KQL query — no explanations, no markdown fences, no extra text.
- Use proper KQL operators: where, summarize, project, extend, join, union, render, bin(), ago(), now(), count(), avg(), sum(), max(), min(), dcount(), tostring(), toint(), datetime(), etc.
- Prefer readable, well-formatted multi-line KQL.
- If the intent is ambiguous, make a reasonable assumption and produce a valid query.
- Always pick the most relevant table from the list below based on the user's intent.
- Keep result sets reasonable — add a top 100 or limit unless the user asks for all results.

Available tables and their key columns:

## Azure Monitor / Log Analytics
- Heartbeat: Computer, OSType, OSName, Version, ComputerEnvironment, TimeGenerated, Category, ResourceGroup, SubscriptionId
- Perf: Computer, ObjectName, CounterName, InstanceName, CounterValue, TimeGenerated
- Event: Computer, EventLog, Source, EventID, Level, RenderedDescription, TimeGenerated
- Syslog: Computer, Facility, SeverityLevel, SyslogMessage, ProcessName, TimeGenerated
- AzureActivity: Caller, OperationName, ResourceGroup, ResourceId, ActivityStatus, Level, TimeGenerated, SubscriptionId
- AzureMetrics: ResourceId, MetricName, Namespace, Average, Maximum, Minimum, Count, TimeGenerated
- AzureDiagnostics: ResourceType, OperationName, ResultType, ResultDescription, TimeGenerated
- Usage: DataType, Solution, Quantity, QuantityUnit, TimeGenerated

## Microsoft Entra ID / Sign-in
- SigninLogs: UserPrincipalName, UserDisplayName, AppDisplayName, IPAddress, Location, ResultType, ResultDescription, ConditionalAccessStatus, TimeGenerated, ClientAppUsed, DeviceDetail, RiskLevel
- AADNonInteractiveUserSignInLogs: UserPrincipalName, AppDisplayName, IPAddress, ResultType, TimeGenerated
- AuditLogs: OperationName, Result, InitiatedBy, TargetResources, LoggedByService, TimeGenerated
- AADServicePrincipalSignInLogs: ServicePrincipalName, AppId, IPAddress, ResultType, TimeGenerated
- AADManagedIdentitySignInLogs: ServicePrincipalName, ResourceDisplayName, ResultType, TimeGenerated

## Microsoft Sentinel / Security
- SecurityEvent: Computer, Account, EventID, Activity, LogonType, IpAddress, SubjectUserName, TimeGenerated
- SecurityAlert: AlertName, AlertSeverity, Description, Entities, ProviderName, Tactics, TimeGenerated
- SecurityIncident: Title, Severity, Status, Owner, Classification, TimeGenerated
- ThreatIntelligenceIndicator: Type, ThreatType, IndicatorType, NetworkIP, Url, DomainName, TimeGenerated, ExpirationDateTime, Confidence
- IdentityInfo: AccountUPN, AccountDisplayName, Department, JobTitle, AccountEnabled, TimeGenerated
- BehaviorAnalytics: UserName, UserPrincipalName, ActivityType, ActionType, DevicesInsights, TimeGenerated

## Network
- AzureNetworkAnalytics_CL: FlowType_s, SrcIP_s, DestIP_s, DestPort_d, L7Protocol_s, FlowStatus_s, TimeGenerated
- DnsEvents: Computer, IPAddresses, Name, QueryType, ResultCode, TimeGenerated
- CommonSecurityLog: DeviceVendor, DeviceProduct, Activity, SourceIP, DestinationIP, DestinationPort, Protocol, TimeGenerated
- W3CIISLog: sIP, csUriStem, scStatus, csMethod, csUserAgent, TimeGenerated

## Containers & Apps
- ContainerLog: ContainerID, LogEntry, LogEntrySource, TimeGenerated
- ContainerInventory: ContainerID, Name, Image, ImageTag, Ports, State, TimeGenerated
- KubeEvents: Name, Namespace, Reason, Message, KubeEventType, TimeGenerated
- KubePodInventory: Name, Namespace, Node, ContainerName, PodStatus, TimeGenerated
- AppRequests: Name, Url, ResultCode, DurationMs, Success, TimeGenerated
- AppExceptions: ProblemId, OuterMessage, Assembly, Method, TimeGenerated
- AppDependencies: Name, Target, DependencyType, ResultCode, DurationMs, Success, TimeGenerated
- AppTraces: Message, SeverityLevel, OperationName, TimeGenerated

## Azure Resources
- ResourceManagementPublicAccessLogs: CallerIpAddress, OperationName, ResultSignature, TimeGenerated
- StorageBlobLogs: OperationName, StatusCode, CallerIpAddress, Uri, TimeGenerated
- KeyVaultLogs: OperationName, ResultType, CallerIPAddress, Id, TimeGenerated
""";

// --- Validate environment ---
var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error: ANTHROPIC_API_KEY environment variable is not set.");
    Console.WriteLine("  export ANTHROPIC_API_KEY=sk-ant-...");
    Console.ResetColor();
    return;
}

var workspaceId = Environment.GetEnvironmentVariable("LOG_ANALYTICS_WORKSPACE_ID");
bool canExecute = !string.IsNullOrEmpty(workspaceId);

// --- Clients ---
AnthropicClient anthropic = new() { ApiKey = apiKey };
LogsQueryClient? logsClient = canExecute ? new LogsQueryClient(new DefaultAzureCredential()) : null;

// --- Banner ---
Console.ForegroundColor = ConsoleColor.Cyan;
Console.WriteLine("╔════════════════════════════════════════════════════╗");
Console.WriteLine("║        KazmonQueryBuilder — KQL Assistant           ║");
Console.WriteLine("║  Type your question in plain English.               ║");
Console.WriteLine("║  Commands: /tables  /help  exit                     ║");
Console.WriteLine("╚════════════════════════════════════════════════════╝");
if (!canExecute)
{
    Console.ForegroundColor = ConsoleColor.DarkYellow;
    Console.WriteLine("  [Query execution disabled — set LOG_ANALYTICS_WORKSPACE_ID to enable]");
}
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
        Console.WriteLine("  anything else — translate natural language to KQL and execute it");
        Console.ResetColor();
        Console.WriteLine();
        continue;
    }

    if (input.Equals("/tables", StringComparison.OrdinalIgnoreCase) ||
        input.Contains("available table", StringComparison.OrdinalIgnoreCase) ||
        input.Contains("list table", StringComparison.OrdinalIgnoreCase) ||
        input.Contains("show table", StringComparison.OrdinalIgnoreCase))
    {
        PrintTableCatalog();
        continue;
    }

    // --- Step 1: Translate to KQL ---
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Translating...");
    Console.ResetColor();

    var kqlBuilder = new System.Text.StringBuilder();
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

        await foreach (var streamEvent in anthropic.Messages.CreateStreaming(parameters))
        {
            if (streamEvent.TryPickContentBlockDelta(out var delta) &&
                delta.Delta.TryPickText(out var text))
            {
                kqlBuilder.Append(text.Text);
            }
        }
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Translation error: {ex.Message}");
        Console.ResetColor();
        continue;
    }

    var kql = kqlBuilder.ToString().Trim();

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("\nGenerated KQL:");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine(kql);
    Console.ResetColor();
    Console.WriteLine();

    // --- Step 2: Execute ---
    if (!canExecute)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("  [Execution skipped — set LOG_ANALYTICS_WORKSPACE_ID to run queries]");
        Console.ResetColor();
        Console.WriteLine();
        continue;
    }

    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine("Executing...");
    Console.ResetColor();

    try
    {
        var response = await logsClient!.QueryWorkspaceAsync(
            workspaceId!,
            kql,
            new QueryTimeRange(TimeSpan.FromDays(1)));

        var table = response.Value.Table;
        PrintResults(table);
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"Execution error: {ex.Message}");
        Console.ResetColor();
        Console.WriteLine();
    }
}

// --- Helpers ---

static void PrintResults(LogsTable table)
{
    if (table.Rows.Count == 0)
    {
        Console.ForegroundColor = ConsoleColor.DarkYellow;
        Console.WriteLine("  No results returned.");
        Console.ResetColor();
        Console.WriteLine();
        return;
    }

    // Calculate column widths (cap at 50 chars)
    var columns = table.Columns;
    var widths = columns.Select(c => Math.Min(50, Math.Max(c.Name.Length, 10))).ToArray();

    // Sample first 20 rows to refine widths
    foreach (var row in table.Rows.Take(20))
        for (int i = 0; i < columns.Count; i++)
            widths[i] = Math.Min(50, Math.Max(widths[i], (row[i]?.ToString() ?? "").Length));

    // Header
    Console.ForegroundColor = ConsoleColor.Cyan;
    PrintRow(columns.Select(c => c.Name).ToArray(), widths);
    Console.WriteLine(string.Join("─┼─", widths.Select(w => new string('─', w))));
    Console.ResetColor();

    // Rows
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

static void PrintRow(string[] values, int[] widths)
{
    var cells = values.Select((v, i) => Truncate(v, widths[i]).PadRight(widths[i]));
    Console.WriteLine(string.Join(" │ ", cells));
}

static string Truncate(string s, int max) =>
    s.Length <= max ? s : s[..(max - 1)] + "…";

static void PrintTableCatalog()
{
    var sections = new Dictionary<string, string[]>
    {
        ["Azure Monitor / Log Analytics"] = ["Heartbeat", "Perf", "Event", "Syslog", "AzureActivity", "AzureMetrics", "AzureDiagnostics", "Usage"],
        ["Microsoft Entra ID / Sign-in"]  = ["SigninLogs", "AADNonInteractiveUserSignInLogs", "AuditLogs", "AADServicePrincipalSignInLogs", "AADManagedIdentitySignInLogs"],
        ["Microsoft Sentinel / Security"]  = ["SecurityEvent", "SecurityAlert", "SecurityIncident", "ThreatIntelligenceIndicator", "IdentityInfo", "BehaviorAnalytics"],
        ["Network"]                        = ["AzureNetworkAnalytics_CL", "DnsEvents", "CommonSecurityLog", "W3CIISLog"],
        ["Containers & Apps"]              = ["ContainerLog", "ContainerInventory", "KubeEvents", "KubePodInventory", "AppRequests", "AppExceptions", "AppDependencies", "AppTraces"],
        ["Azure Resources"]                = ["ResourceManagementPublicAccessLogs", "StorageBlobLogs", "KeyVaultLogs"],
    };
    foreach (var (category, tables) in sections)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"\n  {category}");
        Console.ForegroundColor = ConsoleColor.White;
        foreach (var t in tables)
            Console.WriteLine($"    - {t}");
    }
    Console.ResetColor();
    Console.WriteLine();
}

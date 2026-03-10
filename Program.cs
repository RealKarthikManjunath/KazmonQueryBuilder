using Anthropic;
using Anthropic.Models.Messages;

const string SystemPrompt = """
You are an expert in Kusto Query Language (KQL), used in Azure Data Explorer, Azure Monitor, Microsoft Sentinel, and Log Analytics.

Your job is to translate natural language queries into precise, correct KQL.

Guidelines:
- Output ONLY the KQL query — no explanations, no markdown fences, no extra text.
- Use proper KQL operators: where, summarize, project, extend, join, union, render, bin(), ago(), now(), count(), avg(), sum(), max(), min(), dcount(), tostring(), toint(), datetime(), etc.
- Prefer readable, well-formatted multi-line KQL.
- If the intent is ambiguous, make a reasonable assumption and produce a valid query.
- Always pick the most relevant table from the list below based on the user's intent.

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

var apiKey = Environment.GetEnvironmentVariable("ANTHROPIC_API_KEY");
if (string.IsNullOrEmpty(apiKey))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine("Error: ANTHROPIC_API_KEY environment variable is not set.");
    Console.WriteLine("Set it with: export ANTHROPIC_API_KEY=sk-ant-...");
    Console.ResetColor();
    return;
}

AnthropicClient client = new() { ApiKey = apiKey };

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

    var input = Console.ReadLine();
    if (input is null) break; // EOF
    input = input.Trim();

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

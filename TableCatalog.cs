namespace KazmonQueryBuilder;

static class TableCatalog
{
    public static readonly Dictionary<string, string[]> Sections = new()
    {
        ["Azure Monitor / Log Analytics"] =
        [
            "Heartbeat", "Perf", "Event", "Syslog",
            "AzureActivity", "AzureMetrics", "AzureDiagnostics", "Usage"
        ],
        ["Microsoft Entra ID / Sign-in"] =
        [
            "SigninLogs", "AADNonInteractiveUserSignInLogs", "AuditLogs",
            "AADServicePrincipalSignInLogs", "AADManagedIdentitySignInLogs"
        ],
        ["Microsoft Sentinel / Security"] =
        [
            "SecurityEvent", "SecurityAlert", "SecurityIncident",
            "ThreatIntelligenceIndicator", "IdentityInfo", "BehaviorAnalytics"
        ],
        ["Network"] =
        [
            "AzureNetworkAnalytics_CL", "DnsEvents", "CommonSecurityLog", "W3CIISLog"
        ],
        ["Containers & Apps"] =
        [
            "ContainerLog", "ContainerInventory", "KubeEvents", "KubePodInventory",
            "AppRequests", "AppExceptions", "AppDependencies", "AppTraces"
        ],
        ["Azure Resources"] =
        [
            "ResourceManagementPublicAccessLogs", "StorageBlobLogs", "KeyVaultLogs"
        ],
    };

    public static readonly string SystemPromptSection = """
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

    public static void Print()
    {
        foreach (var (category, tables) in Sections)
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
}

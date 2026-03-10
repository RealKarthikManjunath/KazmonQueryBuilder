namespace KazmonQueryBuilder;

// Each table is a list of rows; each row is a dict of column -> value.
static class SampleData
{
    private static readonly DateTime Now = DateTime.UtcNow;
    private static string T(int minutesAgo) => Now.AddMinutes(-minutesAgo).ToString("o");

    public static readonly Dictionary<string, (string[] Columns, List<string[]> Rows)> Tables = new()
    {
        ["SecurityEvent"] = Build(
            ["TimeGenerated", "Computer", "Account", "EventID", "Activity", "LogonType", "IpAddress", "SubjectUserName"],
            [
                [T(2),   "WIN-DC01",      "CORP\\alice",   "4624", "An account was successfully logged on", "2", "10.0.0.15",  "alice"],
                [T(5),   "WIN-WEB01",     "CORP\\bob",     "4625", "An account failed to log on",           "3", "185.42.11.3","bob"],
                [T(8),   "WIN-WEB01",     "CORP\\bob",     "4625", "An account failed to log on",           "3", "185.42.11.3","bob"],
                [T(12),  "WIN-WEB01",     "CORP\\bob",     "4625", "An account failed to log on",           "3", "185.42.11.3","bob"],
                [T(15),  "WIN-SQL01",     "CORP\\admin",   "4648", "A logon was attempted using explicit credentials", "2", "10.0.0.22", "svc-deploy"],
                [T(20),  "WIN-DC01",      "CORP\\charlie", "4624", "An account was successfully logged on", "2", "10.0.0.31",  "charlie"],
                [T(35),  "WIN-WEB01",     "CORP\\diana",   "4624", "An account was successfully logged on", "10","203.0.113.5","diana"],
                [T(60),  "WIN-SQL01",     "CORP\\alice",   "4624", "An account was successfully logged on", "2", "10.0.0.15",  "alice"],
                [T(90),  "WIN-DC01",      "NT AUTHORITY\\SYSTEM", "4625", "An account failed to log on",   "0", "10.0.0.1",   "SYSTEM"],
                [T(120), "WIN-FILE01",    "CORP\\eve",     "4624", "An account was successfully logged on", "2", "10.0.0.44",  "eve"],
                [T(180), "WIN-WEB01",     "CORP\\frank",   "4625", "An account failed to log on",           "3", "91.108.4.12","frank"],
                [T(200), "WIN-WEB01",     "CORP\\frank",   "4625", "An account failed to log on",           "3", "91.108.4.12","frank"],
                [T(240), "WIN-DC01",      "CORP\\grace",   "4624", "An account was successfully logged on", "2", "10.0.0.55",  "grace"],
                [T(300), "WIN-SQL01",     "CORP\\bob",     "4624", "An account was successfully logged on", "2", "10.0.0.16",  "bob"],
                [T(360), "WIN-FILE01",    "CORP\\hank",    "4625", "An account failed to log on",           "3", "198.51.100.7","hank"],
            ]),

        ["SigninLogs"] = Build(
            ["TimeGenerated", "UserPrincipalName", "AppDisplayName", "IPAddress", "ResultType", "ResultDescription", "Location", "RiskLevel", "ClientAppUsed"],
            [
                [T(3),   "alice@corp.com",   "Microsoft Teams",       "10.0.0.15",   "0",     "Success",                        "US, New York",    "none",   "Browser"],
                [T(7),   "bob@corp.com",     "Azure Portal",          "185.42.11.3", "50126", "Invalid username or password",   "RU, Moscow",      "high",   "Browser"],
                [T(10),  "bob@corp.com",     "Azure Portal",          "185.42.11.3", "50126", "Invalid username or password",   "RU, Moscow",      "high",   "Browser"],
                [T(13),  "bob@corp.com",     "Azure Portal",          "185.42.11.3", "50126", "Invalid username or password",   "RU, Moscow",      "high",   "Browser"],
                [T(18),  "charlie@corp.com", "Office 365",            "10.0.0.31",   "0",     "Success",                        "US, Chicago",     "none",   "Mobile Apps and Desktop clients"],
                [T(25),  "diana@corp.com",   "SharePoint Online",     "203.0.113.5", "53003", "Blocked by Conditional Access",  "GB, London",      "medium", "Browser"],
                [T(40),  "alice@corp.com",   "Azure Portal",          "10.0.0.15",   "0",     "Success",                        "US, New York",    "none",   "Browser"],
                [T(55),  "eve@corp.com",     "Microsoft Teams",       "10.0.0.44",   "0",     "Success",                        "US, Seattle",     "none",   "Mobile Apps and Desktop clients"],
                [T(70),  "frank@corp.com",   "Office 365",            "91.108.4.12", "50057", "User account is disabled",       "CN, Beijing",     "high",   "Browser"],
                [T(100), "grace@corp.com",   "OneDrive",              "10.0.0.55",   "0",     "Success",                        "US, Austin",      "none",   "Browser"],
                [T(130), "hank@corp.com",    "Azure Portal",          "198.51.100.7","50074", "The user account has expired",   "DE, Berlin",      "medium", "Browser"],
                [T(160), "alice@corp.com",   "Microsoft Teams",       "10.0.0.15",   "0",     "Success",                        "US, New York",    "none",   "Browser"],
                [T(190), "charlie@corp.com", "Azure DevOps",          "10.0.0.31",   "0",     "Success",                        "US, Chicago",     "none",   "Browser"],
                [T(220), "bob@corp.com",     "GitHub Enterprise",     "10.0.0.16",   "0",     "Success",                        "US, San Francisco","none", "Browser"],
                [T(250), "diana@corp.com",   "Microsoft Teams",       "10.0.0.45",   "0",     "Success",                        "GB, London",      "none",   "Browser"],
            ]),

        ["Heartbeat"] = Build(
            ["TimeGenerated", "Computer", "OSType", "OSName", "Version", "ComputerEnvironment", "Category", "ResourceGroup"],
            [
                [T(1),  "WIN-DC01",   "Windows", "Windows Server 2022", "10.0.20348", "Azure", "Direct Agent", "rg-corp-prod"],
                [T(1),  "WIN-WEB01",  "Windows", "Windows Server 2019", "10.0.17763", "Azure", "Direct Agent", "rg-corp-prod"],
                [T(1),  "WIN-SQL01",  "Windows", "Windows Server 2022", "10.0.20348", "Azure", "Direct Agent", "rg-corp-prod"],
                [T(1),  "WIN-FILE01", "Windows", "Windows Server 2019", "10.0.17763", "Azure", "Direct Agent", "rg-corp-prod"],
                [T(1),  "LNX-APP01",  "Linux",   "Ubuntu 22.04.3 LTS",  "22.04",      "Azure", "Direct Agent", "rg-corp-prod"],
                [T(1),  "LNX-APP02",  "Linux",   "Ubuntu 22.04.3 LTS",  "22.04",      "Azure", "Direct Agent", "rg-corp-dev"],
                [T(6),  "WIN-DC01",   "Windows", "Windows Server 2022", "10.0.20348", "Azure", "Direct Agent", "rg-corp-prod"],
                [T(6),  "WIN-WEB01",  "Windows", "Windows Server 2019", "10.0.17763", "Azure", "Direct Agent", "rg-corp-prod"],
                [T(6),  "WIN-SQL01",  "Windows", "Windows Server 2022", "10.0.20348", "Azure", "Direct Agent", "rg-corp-prod"],
                [T(6),  "LNX-APP01",  "Linux",   "Ubuntu 22.04.3 LTS",  "22.04",      "Azure", "Direct Agent", "rg-corp-prod"],
                [T(11), "WIN-DC01",   "Windows", "Windows Server 2022", "10.0.20348", "Azure", "Direct Agent", "rg-corp-prod"],
                [T(11), "LNX-APP02",  "Linux",   "Ubuntu 22.04.3 LTS",  "22.04",      "Azure", "Direct Agent", "rg-corp-dev"],
            ]),

        ["Perf"] = Build(
            ["TimeGenerated", "Computer", "ObjectName", "CounterName", "InstanceName", "CounterValue"],
            [
                [T(1),  "WIN-DC01",   "Processor",       "% Processor Time",        "_Total", "12.4"],
                [T(1),  "WIN-WEB01",  "Processor",       "% Processor Time",        "_Total", "78.2"],
                [T(1),  "WIN-SQL01",  "Processor",       "% Processor Time",        "_Total", "45.1"],
                [T(1),  "WIN-FILE01", "Processor",       "% Processor Time",        "_Total", "8.3"],
                [T(1),  "LNX-APP01",  "Processor",       "% Processor Time",        "_Total", "91.7"],
                [T(1),  "WIN-DC01",   "Memory",          "Available MBytes",        "",       "3200"],
                [T(1),  "WIN-WEB01",  "Memory",          "Available MBytes",        "",       "512"],
                [T(1),  "WIN-SQL01",  "Memory",          "Available MBytes",        "",       "1024"],
                [T(1),  "LNX-APP01",  "Memory",          "Available MBytes",        "",       "256"],
                [T(1),  "WIN-SQL01",  "LogicalDisk",     "% Disk Time",             "C:",     "62.3"],
                [T(1),  "WIN-WEB01",  "Network Adapter", "Bytes Received/sec",      "Ethernet","125430"],
                [T(5),  "WIN-WEB01",  "Processor",       "% Processor Time",        "_Total", "82.1"],
                [T(5),  "WIN-SQL01",  "Processor",       "% Processor Time",        "_Total", "50.3"],
                [T(5),  "LNX-APP01",  "Processor",       "% Processor Time",        "_Total", "88.4"],
                [T(5),  "WIN-DC01",   "Memory",          "Available MBytes",        "",       "3150"],
            ]),

        ["AzureActivity"] = Build(
            ["TimeGenerated", "Caller", "OperationName", "ResourceGroup", "ActivityStatus", "Level", "SubscriptionId"],
            [
                [T(10),  "alice@corp.com",      "Microsoft.Compute/virtualMachines/start/action",  "rg-corp-prod", "Succeeded", "Informational", "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
                [T(22),  "bob@corp.com",         "Microsoft.Network/networkSecurityGroups/write",   "rg-corp-prod", "Succeeded", "Informational", "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
                [T(45),  "charlie@corp.com",     "Microsoft.KeyVault/vaults/secrets/read",          "rg-corp-prod", "Succeeded", "Informational", "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
                [T(60),  "svc-deploy@corp.com",  "Microsoft.Web/sites/write",                       "rg-corp-prod", "Succeeded", "Informational", "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
                [T(75),  "alice@corp.com",        "Microsoft.Storage/storageAccounts/delete",        "rg-corp-dev",  "Failed",    "Error",         "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
                [T(90),  "bob@corp.com",          "Microsoft.Authorization/roleAssignments/write",   "rg-corp-prod", "Succeeded", "Warning",       "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
                [T(110), "diana@corp.com",        "Microsoft.Sql/servers/firewallRules/write",       "rg-corp-prod", "Succeeded", "Informational", "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
                [T(130), "svc-deploy@corp.com",   "Microsoft.ContainerService/managedClusters/write","rg-corp-dev",  "Succeeded", "Informational", "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
                [T(150), "eve@corp.com",          "Microsoft.Compute/virtualMachines/delete",        "rg-corp-dev",  "Failed",    "Error",         "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
                [T(200), "charlie@corp.com",      "Microsoft.Network/publicIPAddresses/write",       "rg-corp-prod", "Succeeded", "Informational", "xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx"],
            ]),

        ["DnsEvents"] = Build(
            ["TimeGenerated", "Computer", "Name", "QueryType", "IPAddresses", "ResultCode"],
            [
                [T(2),  "WIN-DC01",   "microsoft.com",            "A",    "20.112.52.29",     "0"],
                [T(4),  "WIN-WEB01",  "malware-c2.ru",            "A",    "91.108.4.55",      "0"],
                [T(6),  "LNX-APP01",  "api.github.com",           "A",    "140.82.113.5",     "0"],
                [T(9),  "WIN-SQL01",  "login.microsoftonline.com","A",    "20.190.132.16",    "0"],
                [T(12), "WIN-WEB01",  "evil-phishing.xyz",        "A",    "185.234.218.44",   "0"],
                [T(15), "WIN-FILE01", "windows.com",              "A",    "20.70.246.20",     "0"],
                [T(18), "LNX-APP02",  "packages.ubuntu.com",      "A",    "91.189.91.83",     "0"],
                [T(21), "WIN-DC01",   "corp.local",               "A",    "10.0.0.1",         "0"],
                [T(25), "WIN-WEB01",  "cdn-js-lib.tk",            "A",    "185.220.101.33",   "0"],
                [T(30), "WIN-SQL01",  "azure.microsoft.com",      "A",    "20.60.19.4",       "0"],
                [T(35), "LNX-APP01",  "botnet-update.net",        "A",    "91.108.56.130",    "0"],
                [T(40), "WIN-DC01",   "google.com",               "A",    "142.250.80.46",    "0"],
            ]),
    };

    private static (string[], List<string[]>) Build(string[] columns, string[][] rows) =>
        (columns, rows.Select(r => r).ToList());
}

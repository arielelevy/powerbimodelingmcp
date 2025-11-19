using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using PowerBIModelingMCP.Library.Contracts;
using PowerBIModelingMCP.Library.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

#nullable enable

var applicationBuilder = Host.CreateApplicationBuilder(args);

// Parse command line arguments
var startArg = args.FirstOrDefault(arg => arg.StartsWith("--start", StringComparison.OrdinalIgnoreCase));
var helpArg = args.FirstOrDefault(arg => arg.Equals("--help", StringComparison.OrdinalIgnoreCase) || arg.Equals("-h", StringComparison.OrdinalIgnoreCase));
var readOnlyArg = args.FirstOrDefault(arg => arg.Equals("--read-only", StringComparison.OrdinalIgnoreCase) || arg.Equals("--readonly", StringComparison.OrdinalIgnoreCase));
var readWriteArg = args.FirstOrDefault(arg => arg.Equals("--read-write", StringComparison.OrdinalIgnoreCase) || arg.Equals("--readwrite", StringComparison.OrdinalIgnoreCase));
var compatibilityArg = args.FirstOrDefault(arg => arg.StartsWith("--compatibility=", StringComparison.OrdinalIgnoreCase));
var skipConfirmationArg = args.FirstOrDefault(arg => arg.Equals("--skip-confirmation", StringComparison.OrdinalIgnoreCase) || arg.Equals("--skipconfirmation", StringComparison.OrdinalIgnoreCase));
var portArg = args.FirstOrDefault(arg => arg.StartsWith("--port=", StringComparison.OrdinalIgnoreCase) || arg.StartsWith("-p=", StringComparison.OrdinalIgnoreCase));
var httpArg = args.FirstOrDefault(arg => arg.Equals("--http", StringComparison.OrdinalIgnoreCase) || arg.Equals("--sse", StringComparison.OrdinalIgnoreCase));

bool shouldStart = !string.IsNullOrEmpty(startArg) || !string.IsNullOrEmpty(readOnlyArg) || !string.IsNullOrEmpty(readWriteArg);
bool shouldShowHelp = !string.IsNullOrEmpty(helpArg);

string? modeOverride = null;
if (!string.IsNullOrEmpty(readOnlyArg))
    modeOverride = "readonly";
else if (!string.IsNullOrEmpty(readWriteArg))
    modeOverride = "readwrite";

string? compatibilityOverride = null;
if (!string.IsNullOrEmpty(compatibilityArg))
    compatibilityOverride = compatibilityArg.Substring("--compatibility=".Length);

bool skipConfirmationEnabled = !string.IsNullOrEmpty(skipConfirmationArg);
bool useHttp = !string.IsNullOrEmpty(httpArg);
int port = 5000; // Default port

if (!string.IsNullOrEmpty(portArg))
{
    var portString = portArg.Contains("=") ? portArg.Split('=')[1] : "5000";
    if (!int.TryParse(portString, out port))
    {
        port = 5000;
    }
}

// If --http or --sse is specified, enable HTTP mode
if (useHttp)
{
    shouldStart = true;
}

if (shouldShowHelp)
{
    PrintUsage();
}
else if (!shouldStart)
{
    PrintWelcomeInfo();
}
else
{
    var config = CreateConfigurationFromArgs(modeOverride, compatibilityOverride, skipConfirmationEnabled);

    applicationBuilder.Services.AddSingleton(config);
    applicationBuilder.Services.AddSingleton<MarkdownResourceParser>();
    applicationBuilder.Services.AddSingleton<ToolRegistrationService>();
    applicationBuilder.Services.AddSingleton<PromptRegistrationService>();
    applicationBuilder.Services.AddSingleton<ResourceRegistrationService>();

    applicationBuilder.Logging.AddConsole(options =>
        options.LogToStandardErrorThreshold = LogLevel.Trace);
    applicationBuilder.Logging.AddEventSourceLogger();
    applicationBuilder.Logging.SetMinimumLevel(LogLevel.Information);

    var mcpBuilder = applicationBuilder.Services.AddMcpServer()
        .WithStdioServerTransport();

    if (useHttp)
    {
        Console.WriteLine($"HTTP mode requested but not yet implemented.");
        Console.WriteLine($"MCP protocol currently only supports STDIO transport.");
        Console.WriteLine($"Starting MCP Server in STDIO mode instead...");
    }

    var provider = applicationBuilder.Services.BuildServiceProvider();

    provider.GetRequiredService<ToolRegistrationService>().RegisterTools(mcpBuilder);
    provider.GetRequiredService<PromptRegistrationService>().RegisterPrompts(mcpBuilder);
    provider.GetRequiredService<ResourceRegistrationService>().RegisterResources(mcpBuilder);

    var logger = provider.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Server starting: Mode={Mode}, Compatibility={Compatibility}",
        config.Mode, config.Compatibility);

    LogConfigurationSettings(logger, config);

    await applicationBuilder.Build().RunAsync();

    logger.LogInformation("Server stopped");
}

static MCPServerConfiguration CreateConfigurationFromArgs(
    string? modeOverride,
    string? compatibilityOverride,
    bool skipConfirmationEnabled = false)
{
    var config = new MCPServerConfiguration
    {
        Mode = ToolMode.ReadWrite,
        Compatibility = CompatibilityMode.PowerBI,
        SkipConfirmation = skipConfirmationEnabled,
        Tools = new ToolsConfiguration
        {
            EnableDatabaseOperationsTool = true,
            EnableTableOperationsTool = true,
            EnableColumnOperationsTool = true,
            EnableMeasureOperationsTool = true,
            EnableBatchMeasureOperationsTool = true,
            EnableBatchColumnOperationsTool = true,
            EnableBatchTableOperationsTool = true,
            EnableCalculationGroupOperationsTool = true,
            EnableCalendarOperationsTool = true,
            EnableQueryGroupOperationsTool = true,
            EnableRelationshipOperationsTool = true,
            EnableDataSourceOperationsTool = true,
            EnablePartitionOperationsTool = true,
            EnableSecurityRoleOperationsTool = true,
            EnableUserHierarchyOperationsTool = true,
            EnableCultureOperationsTool = true,
            EnableModelOperationsTool = true,
            EnableNamedExpressionOperationsTool = true,
            EnableFunctionOperationsTool = true,
            EnableBatchFunctionOperationsTool = true,
            EnableObjectTranslationOperationsTool = true,
            EnableBatchObjectTranslationOperationsTool = true,
            EnablePerspectiveOperationsTool = true,
            EnableBatchPerspectiveOperationsTool = true,
            EnableConnectionOperationsTool = true,
            EnableDaxQueryOperationsTool = true,
            EnableTransactionOperationsTool = true,
            EnableFabricOperationsTool = false
        }
    };

    if (!string.IsNullOrEmpty(modeOverride))
    {
        try
        {
            config.SetToolMode(modeOverride);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Invalid mode argument '{modeOverride}': {ex.Message}");
            Environment.Exit(1);
        }
    }

    if (!string.IsNullOrEmpty(compatibilityOverride))
    {
        try
        {
            config.SetCompatibilityMode(compatibilityOverride);
        }
        catch (ArgumentException ex)
        {
            Console.Error.WriteLine($"Invalid compatibility argument '{compatibilityOverride}': {ex.Message}");
            Environment.Exit(1);
        }
    }

    return config;
}

static void LogConfigurationSettings(ILogger logger, MCPServerConfiguration config)
{
    logger.LogInformation("=== Final Configuration Settings ===");
    logger.LogInformation("Tool Configuration:");
    logger.LogInformation("  Mode: {Mode} (Source: Command Line)", config.Mode);
    logger.LogInformation("  Compatibility: {Compatibility} (Source: Command Line)", config.Compatibility);
    logger.LogInformation("  Skip Confirmation: {SkipConfirmation}",
        config.SkipConfirmation ? "Enabled" : "Disabled");
    logger.LogInformation("========================================");
}

static void PrintWelcomeInfo()
{
    WriteToConsole(@"
 ____                          ____ ___   __  __  ____ ____
|  _ \ _____      _____ _ __  | __ )_ _| |  \/  |/ ___|  _ \
| |_) / _ \ \ /\ / / _ \ '__| |  _ \| |  | |\/| | |   | |_) |
|  __/ (_) \ V  V /  __/ |    | |_) | |  | |  | | |___|  __/
|_|   \___/ \_/\_/ \___|_|    |____/___| |_|  |_|\____|_|
    ", ConsoleColor.Yellow);

    var serverName = "powerbi-modeling-mcp";
    var mcpConfig = new
    {
        servers = new Dictionary<string, object>
        {
            [serverName] = GetMCPRegistrationAsJson()
        }
    };

    var jsonText = JsonSerializer.Serialize(mcpConfig, new JsonSerializerOptions { WriteIndented = true });

    WriteToConsole("MCP configuration (for manual registration):", ConsoleColor.Cyan, emptyLines: 1);
    WriteToConsole(jsonText, ConsoleColor.Yellow);

    WriteToConsole("Visual Studio Code installation (CTRL + Click to open):", ConsoleColor.Cyan, emptyLines: 1);

    var configJson = JsonSerializer.Serialize(GetMCPRegistrationAsJson());
    var encodedConfig = Uri.EscapeDataString(configJson);
    WriteToConsole($"https://vscode.dev/redirect/mcp/install?name={serverName}&config={encodedConfig}", ConsoleColor.Blue);

    WriteToConsole("Warning: ", ConsoleColor.Yellow, false, 1);
    WriteToConsole("Please use caution when working with this MCP server. It's recommended to back up your semantic model, as AI interactions with this MCP may produce unexpected results.", ConsoleColor.Gray, false);

    WriteToConsole("More information: ", ConsoleColor.Cyan, false, 2);
    WriteToConsole("https://github.com/microsoft/powerbi-modeling-mcp", ConsoleColor.Blue);

    WriteToConsole("Press any key to close...", ConsoleColor.Gray, emptyLines: 1);
    Console.ReadKey(true);
}

static object GetMCPRegistrationAsJson(bool readOnly = false)
{
    var processPath = Environment.ProcessPath;
    if (string.IsNullOrEmpty(processPath))
        throw new Exception("Unable to determine the application path. This may occur when running from certain environments. Try running the executable directly or check your environment configuration.");

    var args = new List<string> { "--start" };

    return new
    {
        command = processPath,
        args = args,
        env = new { }
    };
}

static void PrintUsage()
{
    WriteToConsole("Semantic Model MCP Server", ConsoleColor.Yellow);
    WriteToConsole("", ConsoleColor.White);
    WriteToConsole("Usage:", ConsoleColor.Cyan);
    WriteToConsole("  --start                      Start the MCP server (uses default ReadWrite mode)", ConsoleColor.White);
    WriteToConsole("  --read-only                  Start in read-only mode", ConsoleColor.White);
    WriteToConsole("  --readonly                   Start in read-only mode (alias)", ConsoleColor.White);
    WriteToConsole("  --read-write                 Start in read-write mode", ConsoleColor.White);
    WriteToConsole("  --readwrite                  Start in read-write mode (alias)", ConsoleColor.White);
    WriteToConsole("  --skip-confirmation          Skip confirmation prompts for write operations", ConsoleColor.White);
    WriteToConsole("  --compatibility=powerbi      PowerBI only compatibility (default)", ConsoleColor.White);
    WriteToConsole("  --compatibility=full         Full compatibility (PowerBI + Analysis Services)", ConsoleColor.White);
    WriteToConsole("  --http, --sse                Start server in HTTP/SSE mode instead of STDIO", ConsoleColor.White);
    WriteToConsole("  --port=<port>, -p=<port>     Specify HTTP port (default: 5000)", ConsoleColor.White);
    WriteToConsole("  --help, -h                   Show this help message", ConsoleColor.White);
    WriteToConsole("", ConsoleColor.White);
    WriteToConsole("Examples:", ConsoleColor.Cyan);
    WriteToConsole("  powerbi-modeling-mcp --start", ConsoleColor.White);
    WriteToConsole("  powerbi-modeling-mcp --readonly", ConsoleColor.White);
    WriteToConsole("  powerbi-modeling-mcp --readwrite", ConsoleColor.White);
    WriteToConsole("  powerbi-modeling-mcp --http --port=5000", ConsoleColor.White);
    WriteToConsole("  powerbi-modeling-mcp --sse -p=8080", ConsoleColor.White);
    WriteToConsole("  powerbi-modeling-mcp --readwrite --skip-confirmation", ConsoleColor.White);
    WriteToConsole("  powerbi-modeling-mcp --start --compatibility=powerbi", ConsoleColor.White);
    WriteToConsole("  powerbi-modeling-mcp --readwrite --compatibility=full", ConsoleColor.White);
}

static void WriteToConsole(string text, ConsoleColor color, bool newLine = true, int emptyLines = 0)
{
    if (emptyLines > 0)
    {
        for (int i = 0; i < emptyLines; i++)
            Console.WriteLine();
    }

    Console.ForegroundColor = color;
    if (newLine)
        Console.WriteLine(text);
    else
        Console.Write(text);
    Console.ResetColor();
}

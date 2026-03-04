using looter_cli;

static void PrintHelp()
{
    Console.WriteLine($@"
Looter-CLI - High performance concurrent asset downloader.
SYNOPSIS
    looter-cli [FILE]
    looter-cli [OPTIONS]
DESCRIPTION
    A command-line utility designed to bulk download files from a list of URLs.
    It utilizes asynchronous streams and restricts concurrent connections to
    prevent memory spikes and network overloading.
OPTIONS
    -h, --help             Show this help message and exit.
    -c, --connections      Set the maximum number of concurrent downloads (Default: 5).
ARGUMENTS
    [FILE]                 Path to plain text file containing URLs to download.
                           The file must contain one valid URL per line.
EXAMPLES
    looter-cli targets.txt
        Reads 'targets.cli' and begins downloading all URLs concurrently
        into the current working directory.
");
}

string? filepath = null;
int maxConnections = 5;

for (int i = 0; i < args.Length; i++)
{
    var arg = args[i];
    if (arg == "-h" || arg == "--help")
    {
        PrintHelp();
        return;
    }
    else if (arg == "-c" || arg == "--connections")
    {
        if (i + 1 < args.Length && int.TryParse(args[i + 1], out int parsedConnections))
        {
            maxConnections = parsedConnections;
            i++;
        }
        else
        {
            Console.WriteLine("[Error] Please provide a valid positive number after -c or --connections.");
            return;
        }
    }
    else if (!arg.StartsWith("-"))
    {
        if (filepath == null)
        {
            filepath = arg;
        }
    }
    else
    {
        Console.WriteLine($"[Error]: Invalid option {arg}.");
        Console.WriteLine("Type 'looter-cli --help' for usage instructions.");
        return;
    }
}

if (string.IsNullOrEmpty(filepath))
{
    Console.WriteLine($"[Error]: No target file specified.");
    Console.WriteLine("Type 'looter-cli --help' for usage instructions.");
    return;
}
if (!File.Exists(filepath))
{
    Console.WriteLine($"[Error]: Could not find the file: {filepath}.");
    Console.WriteLine("Type 'looter-cli --help' for usage instructions.");
    return;
}
Console.WriteLine("Initializing Looter-CLI...");
Console.WriteLine($"Engine Config: {maxConnections} Max Concurrent Connections.");
try
{
    var targetUrls = new List<string>(File.ReadAllLines(filepath));
    targetUrls.RemoveAll(string.IsNullOrWhiteSpace);
    if (targetUrls.Count == 0)
    {
        Console.WriteLine($"[Error] The provided file is Empty.");
        return;
    }
    Console.WriteLine($"Found {targetUrls.Count} targets. Starting engine...");
    LooterEngine le = new(5);
    await le.StartLootingAsync(targetUrls);
} catch(Exception ex)
{
    Console.WriteLine($"[Fetal Error] {ex.Message}");
}

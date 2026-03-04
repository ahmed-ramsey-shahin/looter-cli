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
    -h, --help    Show this help message and exit.
ARGUMENTS
    [FILE]        Path to plain text file containing URLs to download.
                    The file must contain one valid URL per line.
EXAMPLES
    looter-cli targets.txt
        Reads 'targets.cli' and begins downloading all URLs concurrently
        into the current working directory.
");
}

if(args.Length == 1)
{
    string filepath = args[0];
    if (!File.Exists(filepath))
    {
        Console.WriteLine($"[Error]: Could not find the file: {filepath}");
        Console.WriteLine("Type 'looter-cli --help' for usage instructions.");
        return;
    }
    Console.WriteLine("Initializing Looter-CLI...");
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
}
else
{
    PrintHelp();
    return;
}

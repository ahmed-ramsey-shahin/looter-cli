namespace looter_cli;

public class LooterEngine
{
    private static readonly HttpClient _client = new();
    private readonly SemaphoreSlim _throttler;

    public LooterEngine(int maxConnections)
    {
        _throttler = new(maxConnections);
    }

    public async Task StartLootingAsync(List<string> urls)
    {
        var tasks = new List<Task>();

        foreach (var url in urls)
        {
            await _throttler.WaitAsync();
            tasks.Add(Task.Run(async () =>
            {
                try
                {
                    await DownloadFileAsync(url);
                }
                finally
                {
                    _throttler.Release();
                }
            }));
        }

        await Task.WhenAll(tasks);
        Console.WriteLine("All loot secured.");
    }

    private async Task DownloadFileAsync(string url)
    {
        Uri uri = new(url);
        string filename = Path.GetFileName(uri.LocalPath);
        if (string.IsNullOrEmpty(filename))
        {
            filename = $"looted_file_{Guid.NewGuid()}.html";
        }
        try
        {
            using var response = await _client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            long? totalBytes = response.Content.Headers.ContentLength;
            using var downloadStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filename, FileMode.Create, FileAccess.Write);
            var buffer = new byte[8192];
            long totalRead = 0;
            int bytesRead;
            int lastReportedPercentage = 0;
            Console.WriteLine($"[Started] {filename}");

            while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                totalRead += bytesRead;
                if (totalBytes.HasValue)
                {
                    int percentage = (int)((double) totalRead / totalBytes.Value * 100);
                    if (percentage >= lastReportedPercentage + 10 || percentage == 100)
                    {
                        Console.WriteLine($"[{filename}] {percentage}%");
                        lastReportedPercentage = percentage;
                    }
                }
            }
            Console.WriteLine($"Looted {filename}");
        } catch (Exception ex)
        {
            Console.WriteLine($"[Failed] {url}-{ex.Message}");
        }
    }
}

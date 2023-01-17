namespace AutoDeskDownloader;

internal class Program
{
    public static void Main(string[] args)
    {
        if (args.Length % 2 != 0)
            throw new ArgumentException("The arguments should also be provided with name and value e.g. --page 4");

        var startPage = 1;
        for (int i = 0; i < args.Length; i+=2)
        {
            if (args[i].ToLowerInvariant().Trim() == "--page")
            {
                startPage = int.Parse(args[i + 1]);
            }
        }
        
        
        var downloader = new DownloadLinkExtractor();

        downloader.DownloadScreenCasts(startPage);
    }
}
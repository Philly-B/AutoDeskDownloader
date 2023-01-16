namespace AutoDeskDownloader;

public class LoginDataProvider
{
    public string GetUserName()
    {
        var data = GetLoginFileData();
        return data.First();
    }

    private static string[] GetLoginFileData()
    {
        var data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
            Constants.DesktopLoginFileName);
        if (!File.Exists(data))
            throw new ArgumentException(
                $"Please provide a file named '{Constants.DesktopLoginFileName}' on your desktop which contains 2 lines. The first should be your autodesk email and the second your password.");
        var lines = File.ReadLines(data).ToArray();
        if (lines.Length != 2)
            throw new ArgumentException(
                "LoginData should contain 2 lines only. First is email and second is password.");
        return lines;
    }

    public string GetPassword()
    {
        var data = GetLoginFileData();
        return data.Last();
    }
}
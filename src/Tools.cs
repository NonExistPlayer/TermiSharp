using ConsoleTools;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace TermiSharp;

#nullable disable warnings

public static class Tools
{
    static Tools()
    {
        bool ExeExists(string exe)
        {
            foreach (string path in Tools.ExePath)
                if (Directory.Exists(path))
                    foreach (string file in Directory.GetFiles(path))
                        if (Path.GetFileName(file) == exe + ".exe")
                            return true;
            return false;
        }
        if (OperatingSystem.IsWindows() && ExeExists("winget"))
            AppVerbs.Add("winget", ["install", "show", "source", "search",
            "list", "upgrade", "uninstall", "hash", "validate", "settings",
            "features", "export", "import", "pin", "configure", "download",
            "repair"]);
        if (ExeExists("git"))
            AppVerbs.Add("git", ["clone", "init", "add", "mv", "restore",
            "rm", "bisect", "diff", "grep", "log", "show", "status",
            "branch", "commit", "merge", "rebase", "reset", "switch", "tag",
            "fetch", "pull", "push"]);
        if (ExeExists("dotnet"))
            AppVerbs.Add("dotnet", ["add", "build", "build-server",
            "clean", "format", "help", "list", "msbuild", "new", "nuget",
            "pack", "publish", "remove", "restore", "run", "sdk", "sln",
            "store", "tool", "vstest", "workload"]);
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [SupportedOSPlatform("windows")]
    private static extern uint GetLongPathName(string shortPath, StringBuilder longPath, int longPathLength);

    internal static bool TryActionWithFile(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Terminal.Writeln(ex.Message, ConsoleColor.Red);
            return false;
        }

        return true;
    }
    internal static string GetLength(long Bytes)
    {
        string e = "B";
        string length = "";
        if (Bytes > 1024) e = "KB";
        if (Bytes > 1048576) e = "MB";
        if (Bytes > 1073741824) e = "GB";
        switch (e)
        {
            case "KB":
                length = ((float)Math.Round((double)Bytes / 1024, 2)).ToString() + " " + e;
                break;
            case "MB":
                length = ((float)Math.Round((double)Bytes / 1048576, 2)).ToString() + " " + e;
                break;
            case "GB":
                length = ((float)Math.Round((double)Bytes / 1073741824, 2)).ToString() + " " + e;
                break;
            case "B":
                length = Bytes + " " + e;
                break;
        }
        return length;
    }

    public static string[] ExePath
    {
        get
        {
            if (MainHost != null)
                return Environment.GetEnvironmentVariable("PATH").Split(';').Concat([MainHost.CurrentPath]).ToArray();
            return Environment.GetEnvironmentVariable("PATH").Split(';');
        }
    }
    public static string? GetExePath(string fname)
    {
        foreach (var path in ExePath)
        {
            if (File.Exists(path + $"\\{fname}.exe"))
                return path + $"\\{fname}.exe";
            if (File.Exists(path + $"\\{fname}.bat"))
                return path + $"\\{fname}.bat";
            if (File.Exists(path + $"\\{fname}.cmd"))
                return path + $"\\{fname}.cmd";

            if (File.Exists(path + $"\\{fname}.csx"))
                return path + $"\\{fname}.csx";
            if (File.Exists(path + $"\\{fname}.tss"))
                return path + $"\\{fname}.tss";
        }
        if (File.Exists(Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.tss"))
            return Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.tss";
        if (File.Exists(Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.csx"))
            return Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.csx";

        if (File.Exists(Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.cmd"))
            return Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.cmd";
        if (File.Exists(Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.bat"))
            return Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.bat";
        if (File.Exists(Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.exe"))
            return Path.GetDirectoryName(Environment.ProcessPath) + $"\\Scripts\\{fname}.exe";

        if (File.Exists(fname))
            return Path.GetFullPath(fname);
        return null;
    }
    public static IEnumerable<string> GetLocalExes() => GetExes(MainHost.CurrentPath);
    public static IEnumerable<string> GetExes(string path)
    {
        foreach (var fname in Directory.GetFiles(path))
            if (fname.EndsWith("exe") ||
                fname.EndsWith("bat") ||
                fname.EndsWith("cmd") ||
                fname.EndsWith("csx") ||
                fname.EndsWith("tss"))
                yield return Path.GetFileName(fname);
    }
    public static string GetCorrectCasePath(string path)
    {
        if (!OperatingSystem.IsWindows()) return path;
        if (string.IsNullOrEmpty(path) || !Path.IsPathRooted(path))
            throw new ArgumentException("Invalid path");

        StringBuilder longPath = new(260);
        GetLongPathName(path, longPath, longPath.Capacity);
        return longPath.ToString();
    }
    public static void ShowExitCode(int exitcode, bool userExited)
    {
        if (exitcode == 0)
            Console.ForegroundColor = ConsoleColor.Green;
        else if (exitcode == -1 && !userExited)
            Console.ForegroundColor = ConsoleColor.Red;
        else if (userExited)
            Console.ForegroundColor = ConsoleColor.White;

        Console.WriteLine($"Script exited with code {exitcode}");
    }

    public static Dictionary<string, Command> NHCommands => MainHost.Commands.Where(c => !c.Value.Hidden).ToDictionary(); // NotHiddenCommands
    public static Dictionary<string, string[]> AppVerbs { get; } = [];
}
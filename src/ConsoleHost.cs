using Microsoft.Win32;
using System.Text;
using TermiSharp.ReadLine;
using ConsoleTools;
using LibGit2Sharp;
using TermiSharp.TSScript;
using System.Xml.Linq;

#nullable disable warnings

namespace TermiSharp;

public partial class ConsoleHost : ConsoleHostBase, IRunnable
{
    public readonly static string AutoExecTSSPath = $"{Path.GetDirectoryName(Environment.ProcessPath)}/autoexec.tss"; // autoexec.tss path
    public override string CurrentPath
    {
        get => Environment.CurrentDirectory;
        protected set
        {
            if (Path.GetDirectoryName(Environment.CurrentDirectory) == value && repo != null)
            {
                if (Repository.IsValid(value))
                    repo = new(value);
                else
                    repo = null;
            }

            if (repo != null)
            {
                string dirname = value[(value.LastIndexOf('\\') + 1)..];
                if (dirname.Length == 0)
                    repo = null;
                else if (repo.Ignore.IsPathIgnored(dirname))
                    repo = null;
            }

            if (repo == null && Repository.IsValid(value))
                repo = new(value);

            Environment.CurrentDirectory = value.Replace("\\", "/");
        }
    }
    
    public Dictionary<string, object> Variables = [];
    public BetterReadLine.ReadLine ReadLine;
    public const string Version = "1.1.2-Dev2";
    private Repository? repo;

    public override void Run() => Run(null);
    internal void Run(Options? args)
    {
        args ??= new();
        Console.Title = "TermiSharp";
        Console.OutputEncoding = Encoding.UTF8;
        Init();
        try
        {
            Config = Config.Load(args.ConfigPath ?? Config.ConfigPath);
        }
        catch { }
        if (Config.ShowVersionOnStart && !args.HideVersion)
            HandleCommand("ver");
        if (args.InitPath != null)
            if (Directory.Exists(args.InitPath))
                CurrentPath = args.InitPath;
        if (args.Command != null)
            HandleCommand(args.Command);
        ReadLine = new()
        {
            AutoCompletionHandler = new AutoCompleteHandler(),
            HighlightHandler = new HighlightHandler(),
            HistoryHandler = new HistoryHandler(),
            HintHandler = new HintHandler(),
        };
        AppDomain.CurrentDomain.ProcessExit += (s, e) =>
        {
            if (Config.FirstStart)
                Config.FirstStart = false;
            Config.Write();
            if (!Config.DisableHistoryFile)
                File.WriteAllLines($"{Path.GetDirectoryName(Environment.ProcessPath)}\\.history", HistoryHandler.History);
        };
        if (Config.FirstStart)
        {
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Console");
                    key.SetValue("VirtualTerminalLevel", 1, RegistryValueKind.DWord);
                }
                catch (Exception ex)
                {
                    Terminal.Writeln($"Failed to edit registry:\nHKEY_CURRENT_USER\\Console: VirtualTerminalLevel to 1 (0x00000001)\n{ex}", ConsoleColor.Red);
                }
            }
            Terminal.Writeln("Type `help` to get list of commands.", ConsoleColor.White);
        }
        foreach (string module in Config.AutoModulesInit)
            HandleCommand($"module init {module}");
        if (repo == null && Repository.IsValid(CurrentPath))
            repo = new(CurrentPath);
        if (File.Exists(AutoExecTSSPath))
        {
            bool userExited = false;
            int exitcode = Interpreter.Run(XDocument.Load(AutoExecTSSPath, LoadOptions.SetLineInfo), "autoexec.tss", ref userExited);
            Tools.ShowExitCode(exitcode, userExited);
        }
        MainLoop();
    }

    protected override void MainLoop()
    {
        while (IsRunning)
        {
            try
            {
                if (!File.Exists(Environment.ProcessPath))
                {
                    Terminal.Write("\n`TermiSharp.exe` not found.\n" +
                        "It is possible that the disk on which the program is located has been disabled, which can lead to errors.\n" +
                        "Waiting for a disk connection", ConsoleColor.Red);
                    byte cnt = 0;
                    Console.CursorVisible = false;
                    while (!File.Exists(Environment.ProcessPath))
                    {
                        cnt++;
                        if (cnt == 7)
                        {
                            Terminal.Write("\r                                         ", ConsoleColor.Red);
                            Terminal.Write("\rWaiting for a disk connection", ConsoleColor.Red);
                            cnt = 0;
                        }
                        if (cnt > 0)
                            Terminal.Write(" .", ConsoleColor.White);
                        Thread.Sleep(1000);
                    }
                    Console.CursorVisible = true;
                    Terminal.Writeln("\nDisk found!\nIt is recommended to restart the terminal with the `restart` command for correct operation.\n", ConsoleColor.Green);
                }
                if (!Config.NerdFontsSupport)
                {
                    Terminal.Write(Environment.UserName, Environment.IsPrivilegedProcess ? ConsoleColor.DarkRed : ConsoleColor.Cyan);
                    Console.Write('@');
                    Terminal.Write(CurrentPath, ConsoleColor.White);
                    if (CurrentPath.Length >= 90)
                        Console.WriteLine();
                    if (repo != null)
                        Terminal.Write($" at {repo.Head.FriendlyName} ", ConsoleColor.Green);
                    Terminal.Write("# ", ConsoleColor.Blue);
                }
                else
                {
                    Console.BackgroundColor = Environment.IsPrivilegedProcess ? ConsoleColor.Red : ConsoleColor.White;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(" ");
                    Console.Write(Environment.UserName + " ");
                    Console.ResetColor();
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Terminal.Write(" ", Environment.IsPrivilegedProcess ? ConsoleColor.Red : ConsoleColor.White);
                    Console.BackgroundColor = ConsoleColor.Blue;
                    Console.ForegroundColor = ConsoleColor.Black;
                    Console.Write(CurrentPath.Replace("\\", "  ") + (CurrentPath.EndsWith('\\') ? "" : " "));
                    Console.ResetColor();
                    if (repo != null)
                        Console.BackgroundColor = ConsoleColor.Green;
                    Terminal.Write(" ", ConsoleColor.Blue);
                    if (repo != null)
                    {
                        Console.BackgroundColor = ConsoleColor.Green;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.Write($"\ue725 {repo.Head.FriendlyName} ");
                        Console.ResetColor();
                        Terminal.Write($" ", ConsoleColor.Green);
                    }
                    if (CurrentPath.Length >= 70)
                        Terminal.Write("\n# ", ConsoleColor.Blue);
                }
                string ln = ReadLine.Read();
                if (ln != "")
                    HistoryHandler.History.Insert(0, ln);
                HistoryHandler.Selected = 0;
                if (string.IsNullOrWhiteSpace(ln)) continue;
                if (ln.Split(' ').Where(s => s.StartsWith('$')).Any(s => Environment.GetEnvironmentVariable(s[1..]) == null))
                {
                    Terminal.Writeln($"Environment variable is missing.", ConsoleColor.Red);
                    continue;
                }
                if (ln.Split(' ').Where(s => s.StartsWith('@')).Any(s => !Variables.ContainsKey(s)))
                {
                    Terminal.Writeln($"Variable is missing.", ConsoleColor.Red);
                    continue;
                }
                ln = string.Join(' ', ln.Split(' ').Select(s =>
                {
                    if (s.StartsWith('$'))
                        s = '"' + Environment.GetEnvironmentVariable(s[1..]) + '"';
                    else if (s.StartsWith('@'))
                        s = '"' + Variables[s[1..]].ToString() + '"';
                    return s;
                }));
                try
                {
                    HandleCommand(ln);
                }
                catch (Exception e)
                {
                    ExceptionHandler(e);
                }
            }
            catch (Exception e)
            {
                ExceptionHandler(e);
            }
        }
    }
}
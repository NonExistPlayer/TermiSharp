using Microsoft.Win32;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TermiSharp.Attributes;
using TermiSharp.ReadLine;
using TermiSharp.TSScript;
using ConsoleTools;
using LibGit2Sharp;

#nullable disable warnings

namespace TermiSharp;

public partial class ConsoleHost : IRunnable
{
    internal readonly Dictionary<string, Command> Commands = [];
    internal Dictionary<string, Command> NHCommands => Commands.Where(c => !c.Value.Hidden).ToDictionary(); // NotHiddenCommands
    internal string CurrentPath
    {
        get => Environment.CurrentDirectory;
        set
        {
            value = value.Replace("\\", "/");
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

            Environment.CurrentDirectory = value;
        }
    }
    public Config Config { get; private set; }
    public string[] ExePath => Environment.GetEnvironmentVariable("PATH").Split(';').Concat([CurrentPath]).ToArray();
    public Dictionary<string, object> Variables = [];
    public BetterReadLine.ReadLine ReadLine;
    public const string Version = "1.1.1-Dev";
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern uint GetLongPathName(string shortPath, StringBuilder longPath, int longPathLength);
    private Repository? repo;

    public bool IsRunning { get; set; } = true;
    public void Run() => Run(null);
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
        catch
        {
            Config = new();
        }
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
        MainLoop();
    }
    public void RunAsync() => Task.Run(Run);

    void MainLoop()
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

    public string? GetExePath(string fname)
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

    public IEnumerable<string> GetLocalExes() => GetExes(CurrentPath);
    public IEnumerable<string> GetExes(string path)
    {
        foreach (var fname in Directory.GetFiles(path))
            if (fname.EndsWith("exe") ||
                fname.EndsWith("bat") ||
                fname.EndsWith("cmd") ||
                fname.EndsWith("csx") ||
                fname.EndsWith("tss"))
                yield return fname[(fname.LastIndexOf('\\') + 1)..];
    }

    static string GetLength(long Bytes)
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

    static string GetCorrectCasePath(string path)
    {
        if (!OperatingSystem.IsWindows()) return path;
        if (string.IsNullOrEmpty(path) || !Path.IsPathRooted(path))
            throw new ArgumentException("Invalid path");

        StringBuilder longPath = new(260);
        GetLongPathName(path, longPath, longPath.Capacity);
        return longPath.ToString();
    }
    public void HandleCommand(string ln)
    {
        string command = ln.Split(' ')[0];
        string[] args = ln.Split(' ')[1..];
        string? path = GetExePath(command);
        if (path == null && !Commands.ContainsKey(command) && !File.Exists(command))
        {
            Terminal.Writeln($"`{command}` is not an executable file, batch script,\nexternal or internal command.", ConsoleColor.Red);
            return;
        }
        if (args.Any(s => s.Contains('"')))
        {
            string pattern = "(?<=^|\\s)(?=[^\"]*\"[^\"]*$)([^\"\\s]+)|\"([^\"]*)\"";
            MatchCollection matches = Regex.Matches(string.Join(' ', args), pattern);
            args = new string[matches.Count];
            for (int i = 0; i < matches.Count; i++)
            {
                if (matches[i].Groups[1].Success)
                {
                    args[i] = matches[i].Groups[1].Value;
                }
                else if (matches[i].Groups[2].Success)
                {
                    args[i] = matches[i].Groups[2].Value;
                }
            }
        }
        if (Commands.TryGetValue(command, out Command com))
            com.Call(args);
        if (path != null || File.Exists(command))
        {
            path ??= Path.GetFullPath(command);
            switch (path[(path.Length - 3)..path.Length])
            {
                case "exe":
                    Process prc = new()
                    {
                        StartInfo = new ProcessStartInfo(path, string.Join(' ', args))
                    };
                    try
                    {
                        prc.Start();
                    }
                    catch (Win32Exception ex)
                    {
                        if (ex.NativeErrorCode == 740)
                        {
                            Terminal.Writeln($"Access denied: {ex.Message.Split(". ")[1]}", ConsoleColor.Red);
                            return;
                        }
                        else throw;
                    }
                    prc.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
                    prc.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);
                    prc.WaitForExit();
                    break;
                case "cmd":
                case "bat":
                    prc = new()
                    {
                        StartInfo = new ProcessStartInfo("cmd.exe", $"/c \"{path}\"" + string.Join(' ', args))
                    };
                    prc.Start();
                    prc.OutputDataReceived += (s, e) => Console.WriteLine(e.Data);
                    prc.ErrorDataReceived += (s, e) => Console.WriteLine(e.Data);
                    prc.WaitForExit();
                    break;
                case "tss": // TermiSharpScript
                    bool userExited = false;
                    int exitcode = Interpreter.Run(XDocument.Load(path, LoadOptions.SetLineInfo), Path.GetFileName(path), ref userExited);
                    if (exitcode == 0)
                        Console.ForegroundColor = ConsoleColor.Green;
                    else if (exitcode == -1 && !userExited)
                        Console.ForegroundColor = ConsoleColor.Red;
                    else if (userExited)
                        Console.ForegroundColor = ConsoleColor.White;

                    Console.WriteLine($"Script exited with code {exitcode}");
                    break;
                case "csx":
                    try
                    {
                        CSharpScript.RunAsync(File.ReadAllText(path),
                            ScriptOptions.Default
                                .WithReferences(typeof(Range).Assembly, typeof(Console).Assembly, typeof(object).Assembly, typeof(ConsoleHost).Assembly));
                    }
                    catch (CompilationErrorException ex)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Terminal.Write(ex.Diagnostics.Select(d => d.ToString()).ToArray());
                        Console.ResetColor();
                        Console.Write('\n');
                    }
                    break;
                default:
                    Terminal.Writeln($"`{command}` is not an executable file, batch script,\nexternal or internal command.", ConsoleColor.Red);
                    return;
            }
            return;
        }
    }
    public void ModuleInit(string modulename)
    {
        string firstPath = Path.GetDirectoryName(Environment.ProcessPath) + "\\Modules\\" + modulename + $"\\bin\\Debug\\net8.0\\{modulename}.dll";
        string secondPath = Path.GetDirectoryName(Environment.ProcessPath) + $"\\Modules\\{modulename}.dll";
        string thirdPath = Path.GetDirectoryName(Environment.ProcessPath) + $"\\Modules\\{modulename}\\{modulename}.dll";
        Assembly asm;
        if (File.Exists(firstPath))
            asm = Assembly.LoadFrom(firstPath);
        else if (File.Exists(secondPath))
            asm = Assembly.LoadFrom(secondPath);
        else if (File.Exists(thirdPath))
            asm = Assembly.LoadFrom(thirdPath);
        else if (File.Exists(modulename))
            asm = Assembly.LoadFrom(modulename);
        else
        {
            Terminal.Writeln($"Can not find the module `{modulename}`", ConsoleColor.Red);
            return;
        }
        Type module = asm.GetType($"{modulename}.Module");
        MethodInfo? init = module.GetMethod("Init");
        MethodInfo? uninit = module.GetMethod("Uninit");
        var methods = module.GetMethods(BindingFlags.Public | BindingFlags.Static);
        foreach (var method in methods)
        {
            if (method.IsDefined(typeof(NotACommandAttribute), false))
                continue;
            byte max = 0, min = 0;
            string name = method.Name;
            max = (byte)(min + method.GetParameters().Count(p => p.HasDefaultValue));

            IEnumerable<Type> types = method.GetParameters().Select(p => p.ParameterType);
            if (method.IsDefined(typeof(OverrideCommandAttribute), false))
            {
                name = method.GetCustomAttribute<OverrideCommandAttribute>().Command;
                Commands.Remove(name);
            }
            if (method.IsDefined(typeof(SubCommandAttribute), false))
                SubCommands.Add(name, method.GetCustomAttributes<SubCommandAttribute>().Select(a => a.SubCommand).ToArray());
            if (method.IsDefined(typeof(CustomCommandNameAttribute), false))
                name = method.GetCustomAttribute<CustomCommandNameAttribute>().CommandName;
            if (Commands.ContainsKey(method.Name))
                name = $"{modulename}.{name}";
            Commands.Add(name, new(null, (min, max), types.ToArray(), method, Hidden: method.IsDefined(typeof(HiddenCommandAttribute), false)));
        }
        init?.Invoke(null, null);
        if (uninit != null)
            AppDomain.CurrentDomain.ProcessExit += (s, e) => uninit.Invoke(null, null);
    }
}
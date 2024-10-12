using ConsoleTools;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using TermiSharp.TSScript;

namespace TermiSharp;

public abstract class ConsoleHostBase : IRunnable
{
    public abstract void Run();
    public void RunAsync() => Task.Run(Run);
    public bool IsRunning { get; set; } = true;
    public Config Config { get; protected set; } = new(); // default
    public ReadOnlyDictionary<string, Command> Commands => new(_commands);

    protected Dictionary<string, Command> _commands = [];
    public virtual string CurrentPath
    {
        get => Environment.CurrentDirectory;
        protected set => Environment.CurrentDirectory = value;
    }
    protected abstract void MainLoop();

    public virtual void HandleCommand(string ln)
    {
        string command = ln.Split(' ')[0];
        string[] args = ln.Split(' ')[1..];
        string? path = Tools.GetExePath(command);
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
                    args[i] = matches[i].Groups[1].Value;
                else if (matches[i].Groups[2].Success)
                    args[i] = matches[i].Groups[2].Value;
            }
        }
        if (_commands.TryGetValue(command, out Command com))
        {
            com.Call(args);
            return;
        }
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
                    Tools.ShowExitCode(exitcode, userExited);
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
}
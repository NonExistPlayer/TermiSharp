// This module is only for studying the creation of similar modules that modify the TermiSharp environment.
// Данный модуль является только для изучения создания подобных модулей которые изменяют среду TermiSharp.

using ConsoleTools;
using TermiSharp;

namespace BashShell;

internal sealed partial class BashHost : ConsoleHostBase, IRunnable
{
    public override void Run()
    {
        try
        {
            Config = Config.Load(Config.ConfigPath);
        }
        catch { }
        foreach (string module in Config.AutoModulesInit)
            HandleCommand($"module init {module}");
        MainLoop();
    }

    protected override void MainLoop()
    {
        while (IsRunning)
        {
            Terminal.Write($"{Environment.UserName}@{Environment.MachineName} ", ConsoleColor.Green);
            Terminal.Write(CurrentPath, ConsoleColor.Yellow);
            Console.Write(" $ ");
            string ln = Console.ReadLine() ?? "";
            if (ln != "")
                HandleCommand(ln);
        }
    }
}
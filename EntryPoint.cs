global using static TermiSharp.EntryPoint;
using ConsoleTools;

namespace TermiSharp;

#pragma warning disable CS8618

public static class EntryPoint
{
    public static ConsoleHost MainHost { get; private set; }
    internal static void Main(string[] Args)
    {
        MainHost = new ConsoleHost();
        AppDomain.CurrentDomain.UnhandledException += (s, e) => ExceptionHandler((Exception)e.ExceptionObject);
        MainHost.Run();
    }

    internal static void ExceptionHandler(Exception ex) => Terminal.Writeln(ex.ToString(), ConsoleColor.Red);
}
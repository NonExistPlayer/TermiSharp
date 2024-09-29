global using static TermiSharp.EntryPoint;

namespace TermiSharp;

#pragma warning disable CS8618

public static class EntryPoint
{
    public static ConsoleHost MainHost { get; private set; }
    public static Exception? LastException { get; private set; }
    internal static void Main(string[] Args)
    {
        Random rnd = new();
        MainHost = new ConsoleHost();
        AppDomain.CurrentDomain.UnhandledException += (s, e) => ExceptionHandler((Exception)e.ExceptionObject);
        MainHost.Run(Args);
    }

    internal static void ExceptionHandler(Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(ex.Message);
        LastException = ex;
        Console.ResetColor();
    }
}
global using static TermiSharp.EntryPoint;
using CommandLine;
using ConsoleTools;

namespace TermiSharp;

#pragma warning disable CS8618

public static class EntryPoint
{
    public static ConsoleHost MainHost { get; private set; }
    public static Exception? LastException { get; private set; }
    internal static void Main(string[] _args)
    {
        Random rnd = new();
        MainHost = new ConsoleHost();
        AppDomain.CurrentDomain.UnhandledException += (s, e) => ExceptionHandler((Exception)e.ExceptionObject);
        Parser parser = new(with => with.AutoVersion = false);
        Options? args = parser.ParseArguments<Options>(_args).Value;
        MainHost.Run(args);
    }

    internal static void ExceptionHandler(Exception ex)
    {
        if (Console.CursorLeft > 0)
            Console.Error.WriteLine();
        Console.Beep();
        if (MainHost.Config.NerdFontsSupport)
        {
            Terminal.Write("\ue0b6", ConsoleColor.Red);
            Console.BackgroundColor = ConsoleColor.Red;
            Terminal.Write("\uf06a ", ConsoleColor.White);
            Terminal.Write("\ue0b4 ", ConsoleColor.Red);
        }
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Error.WriteLine(ex.Message);
        LastException = ex;
        Console.ResetColor();
    }
}
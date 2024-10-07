global using static TermiSharp.EntryPoint;
using CommandLine;
using ConsoleTools;

namespace TermiSharp;

#pragma warning disable CS8618

public static class EntryPoint
{
    public static ConsoleHostBase MainHost
    {
        get => _mainhost;
        set
        {
            if (_mainhost != null)
                _mainhost.IsRunning = false;
            _mainhost = value;
        }
    }
    public static Exception? LastException { get; private set; }
    private static ConsoleHostBase _mainhost;

    internal static void Main(string[] _args)
    {
        Random rnd = new();
        _mainhost = new ConsoleHost();
        AppDomain.CurrentDomain.UnhandledException += (s, e) => ExceptionHandler((Exception)e.ExceptionObject);
        Parser parser = new(with => with.AutoVersion = false);
        Options? args = parser.ParseArguments<Options>(_args).Value;
        ((ConsoleHost)_mainhost).Run(args);
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
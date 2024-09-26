using ConsoleTools;
using LunaPlayer;
using TermiSharp.Attributes;

namespace LunaPlayerAPI;

#pragma warning disable CA1416

public static class Module
{
    [CustomCommandName("play")]
    public static void Play(string path, bool loop = false)
    {
        if (!File.Exists(path))
        {
            Terminal.Writeln("Specified file does not exist.", ConsoleColor.Red);
            return;
        }
        Player.PlayUI(path, loop: (sbyte)loop.GetHashCode());
        Console.Title = "TermiSharp";
    }
}
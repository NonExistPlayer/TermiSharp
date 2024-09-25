using ConsoleTools;
using LunaPlayer;
using TerminalPP.Attributes;

namespace LunaPlayerAPI;

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
        Console.Title = "Terminal++";
    }
}
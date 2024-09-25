using ConsoleTools;

namespace TermiSharp;

#nullable disable warnings

internal static class Tools
{
    public static bool TryActionWithFile(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Terminal.Writeln(ex.Message, ConsoleColor.Red);
            return false;
        }

        return true;
    }
}
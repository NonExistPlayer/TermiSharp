using BetterReadLine;

namespace TermiSharp.ReadLine;

internal sealed class HighlightHandler : IHighlightHandler
{
    public const string RED        = "\u001b[31m";   // Non-existent directory
    public const string YELLOW     = "\u001b[33m";   // Existing command
    public const string GREEN      = "\u001b[32m";   // Existing executable
    public const string BLUE       = "\u001b[34m";   // Existing directory
    public const string RESET      = "\u001b[0m";    // ...
    public const string MAGNETA    = "\u001b[35;3m"; // debug
    public const string DARK_RED   = "\u001b[31;3m"; // out, outln arg[0] && "text in quotes"
    public const string DARK_GREEN = "\u001b[32;3m"; // Enviroment variable ($variable)
    public const string DARK_GRAY  = "\u001b[90m";   // hint

    public string Highlight(string text)
    {
        string[] words = text.Split(' ');
        if (MainHost.SubCommands.TryGetValue(words[0], out string[]? value) && words.Length > 1)
            text = text.Insert(words[0].Length + 1, value.Contains(words[1]) ? YELLOW : RED);
        if (MainHost.Commands.ContainsKey(words[0]))
        {
            if (words.Length > 1)
                text = text.Insert(words[0].Length + 1, words[0] switch
                {
                    "cd" => Directory.Exists(words[1].Replace("\"", "")) ? BLUE : RED,
                    "out" => DARK_RED,
                    "outln" => DARK_RED,
                    _ => RESET
                });
            else
                text = text.Insert(words[0].Length, RESET);

            text = text.Insert(0, YELLOW);
        }
        else if (MainHost.GetExePath(words[0]) != null)
        {
            text = text.Insert(words[0].Length, RESET);
            text = text.Insert(0, GREEN);
        }
        else
            text = text.Insert(0, RESET);

        text += RESET;

        return text;
    }
}
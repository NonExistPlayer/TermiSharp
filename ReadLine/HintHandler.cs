using BetterReadLine;
using static TermiSharp.ReadLine.HighlightHandler;

namespace TermiSharp.ReadLine;

internal sealed class HintHandler : IHintHandler
{
    public string Hint(string promptText)
    {
        string hint = "";

        if (MainHost.NHCommands.Keys.ToArray().Any(s => s.StartsWith(promptText)) && !MainHost.NHCommands.ContainsKey(promptText))
        {
            string str = Array.Find(MainHost.Commands.Keys.ToArray(), s => s.StartsWith(promptText)) ?? "";
            hint = str[promptText.Length..];
        }

        if (promptText.Split(' ').Length == 2)
        {
            if (promptText.StartsWith("cd "))
            {
                string[] dirs = Directory.GetDirectories(MainHost.CurrentPath).Select(d => d[(d.LastIndexOf('\\') + 1)..]).ToArray();
                string? hintDir = Array.Find(dirs, n => n.StartsWith(promptText.Split(' ')[1]));
                if (hintDir != null)
                    hint = hintDir[promptText.Split(' ')[1].Length..];
            }
            if (MainHost.SubCommands.ContainsKey(promptText.Split(' ')[0]))
            {
                string? hintSCom = Array.Find(MainHost.SubCommands[promptText.Split(' ')[0]], n => n.StartsWith(promptText.Split(' ')[1]));
                if (hintSCom != null)
                    hint = hintSCom[promptText.Split(' ')[1].Length..];
            }
        }

        if (!string.IsNullOrWhiteSpace(hint))
        {
            hint = hint.Insert(0, DARK_GRAY);
            hint += RESET;
        }

        return hint;
    }

    public void Reset() { }
}
using BetterReadLine;
using static TermiSharp.ReadLine.HighlightHandler;

namespace TermiSharp.ReadLine;

internal sealed class AutoCompleteHandler : IAutoCompleteHandler
{
    public char[] Separators { get; set; } = [];
    public static readonly string[] CA1FORD = [
        "rm", "cp", "mv"
        ]; // CommandArgument1FileOrDir = these are commands that require an existing file or existing directory as the first argument.
    public static readonly string[] CA1FILE = [
        "see", "see-bin", "see-meta"
        ]; // CommandArgument1FILE = these are commands that require an existing file as the first argument.


    public IList<Completion> GetSuggestions(string text, int completionStart, int completionEnd)
    {
        string AddQuotes(string path) => path.Contains(' ') ? $"\"{path}\"" : path;
        string[] words = text.Split(' ');
        if (words.Length > 1)
        {
            if (CA1FILE.Contains(words[0]))
            {
                return Directory
                    .GetFiles(MainHost.CurrentPath)
                    .Select(f =>
                        new Completion($"{words[0]} {AddQuotes(f[(f.LastIndexOf('\\') + 1)..])}",
                        $"{BLUE}{f[(f.LastIndexOf('\\') + 1)..]}"))
                    .ToList();
            }
            if (CA1FORD.Contains(words[0]))
            {
                return Directory
                    .GetDirectories(MainHost.CurrentPath)
                    .Select(d =>
                        new Completion($"{words[0]} {AddQuotes(d[(d.LastIndexOf('\\') + 1)..])}",
                        $"{BLUE}{d[(d.LastIndexOf('\\') + 1)..]}"))
                    .Concat(
                        Directory
                        .GetFiles(MainHost.CurrentPath)
                        .Select(f =>
                            new Completion($"{words[0]} {AddQuotes(f[(f.LastIndexOf('\\') + 1)..])}",
                            $"{BLUE}{f[(f.LastIndexOf('\\') + 1)..]}"))
                    )
                    .ToList();
            }
        }
        if (text.StartsWith("cd "))
            return Directory
                .GetDirectories(MainHost.CurrentPath)
                .Where(d => !string.IsNullOrWhiteSpace(text[3..]) && d.StartsWith(text[3..]))
                .Select(c => 
                new Completion($"cd {(c[(c.LastIndexOf('\\') + 1)..].Contains(' ') ? '"' : "")}" + 
                    c[(c.LastIndexOf('\\') + 1)..] + 
                    (c[(c.LastIndexOf('\\') + 1)..].Contains(' ') ? '"' : ""),
                    $"{BLUE}{c[(c.LastIndexOf('\\') + 1)..]}{RESET}")).ToList();
        if (MainHost.Commands.TryGetValue(words[0], out Command com))
            return com
                .SubCommands
                .Select(c => new Completion($"{words[0]} {c}", $"{YELLOW}{c}{RESET}"))
                .ToList();
        if (Tools.AppVerbs.TryGetValue(words[0], out string[]? sc))
            return sc
                .Where(c => words.Length <= 1 || c.StartsWith(words[1]))
                .Select(c => new Completion($"{words[0]} {c}", $"{YELLOW}{c}{RESET}")).ToList();
        return Tools.NHCommands.Keys
            .Where(c => c.StartsWith(text))
            .Select(c => new Completion(c, $"{YELLOW}{c}{RESET}"))
            .Concat(
                Tools.GetLocalExes()
                    .Where(e => e.StartsWith(text))
                    .Select(e => new Completion(Path.GetFileNameWithoutExtension(e), $"{GREEN}{e}{RESET}"))
            )
            .ToList();
    }
}
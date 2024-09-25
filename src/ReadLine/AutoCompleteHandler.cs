using BetterReadLine;
using static TermiSharp.ReadLine.HighlightHandler;

namespace TermiSharp.ReadLine;

internal sealed class AutoCompleteHandler : IAutoCompleteHandler
{
    public char[] Separators { get; set; } = [];

    public IList<Completion> GetSuggestions(string text, int completionStart, int completionEnd)
    {
        if (text.StartsWith("cd "))
            return Directory
                .GetDirectories(MainHost.CurrentPath)
                .Where(d => !string.IsNullOrWhiteSpace(text[3..]) && d.StartsWith(text[3..]))
                .Select(c => 
                new Completion($"cd {(c[(c.LastIndexOf('\\') + 1)..].Contains(' ') ? '"' : "")}" + 
                    c[(c.LastIndexOf('\\') + 1)..] + 
                    (c[(c.LastIndexOf('\\') + 1)..].Contains(' ') ? '"' : ""),
                    $"{BLUE}{c[(c.LastIndexOf('\\') + 1)..]}{RESET}")).ToList();
        if (MainHost.SubCommands.TryGetValue(text.Split(' ')[0], out string[]? sc))
            return sc.Select(c => new Completion($"{text.Split(' ')[0]} {c}", $"{YELLOW}{c}{RESET}")).ToList();
        return MainHost.NHCommands.Keys
            .Where(c => c.StartsWith(text))
            .Select(c => new Completion(c, $"{YELLOW}{c}{RESET}"))
            .Concat(MainHost.GetLocalExes()
                .Where(e => e.StartsWith(text))
                .Select(e => new Completion(e, $"{GREEN}{e}{RESET}")))
            .ToList();
    }
}
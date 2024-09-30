using BetterReadLine;
using static TermiSharp.ReadLine.HighlightHandler;

namespace TermiSharp.ReadLine;

internal sealed class HintHandler : IHintHandler
{
    static AutoCompleteHandler achandler = new();
    public string Hint(string promptText)
    {
        return DARK_GRAY +
            achandler.GetSuggestions(promptText, 0, 0)
                .Where(c => c.CompletionText.StartsWith(promptText))
                .Select(c => c.CompletionText)
                .FirstOrDefault(promptText)[promptText.Length..]
            + RESET;
    }

    public void Reset() { }
}
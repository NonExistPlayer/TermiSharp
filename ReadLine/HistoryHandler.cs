using BetterReadLine;

namespace TermiSharp.ReadLine;

internal sealed class HistoryHandler : IHistoryHandler
{
    public static readonly List<string> History = [];
    public static byte Selected = 0;
    public string? GetNext(string promptText, int caret, bool wasEdited)
    {
        if (History.Count == 0) return null;
        if (Selected + 1 > History.Count) return null;
        Selected++;
        return History[Selected - 1];
    }

    public string? GetPrevious(string promptText, int caret)
    {
        if (History.Count == 0) return null;
        if (Selected - 1 < 0) return "";
        Selected--;
        return History[Selected];
    }
}
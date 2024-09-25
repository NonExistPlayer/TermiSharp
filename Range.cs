namespace TermiSharp;

public readonly record struct Range(int Start, int? End = null)
{
    public static bool TryParse(string s)
    {
        if (s.Split("..").Length != 2 && s.Split("..").Length != 1) return false;
        if (!int.TryParse(s.Split("..")[0], out _)) return false;
        if (s.Split("..").Length > 1)
            if (!string.IsNullOrEmpty(s.Split("..")[1]))
                if (!int.TryParse(s.Split("..")[1], out _)) return false;
        return true;
    }

    public static Range Parse(string s)
    {
        if (!TryParse(s)) throw new FormatException();
        if (s.Contains(".."))
            return new(int.Parse(s.Split("..")[0]), s.Split("..").Length == 1 ? -1 : int.Parse(s.Split("..")[1]));
        return new(int.Parse(s));
    }
}
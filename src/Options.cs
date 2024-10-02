using CommandLine;

namespace TermiSharp;

internal sealed class Options
{
    [Option("hideversion")]
    public bool HideVersion { get; set; }
    [Option('p', "path", Default = null)]
    public string? InitPath { get; set; }
    [Option("config", Default = null)]
    public string? ConfigPath { get; set; }
    [Option('c', Default = null)]
    public string? Command { get; set; }
}
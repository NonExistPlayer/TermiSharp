using Newtonsoft.Json;

namespace TermiSharp;

#nullable disable warnings

public sealed class Config
{
    internal static readonly string ConfigPath = Path.GetDirectoryName(Environment.ProcessPath) + "\\config.json";
    public static Config Load(string configPath) => JsonConvert.DeserializeObject<Config>(File.ReadAllText(configPath));
    
    public void Write() => File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));


    public bool FirstStart = true;
    public string[] AutoModulesInit = []; // which modules will be initialized when TermiSharp is started
    public bool DisableHistoryFile = false; // disables saving the file .history
    public bool ShowVersionOnStart = true; // will the version screen be shown when TermiSharp starts
    public bool SimplifiedVersionWindow = false; // simplified version window
    public bool NerdFontsSupport = false; // support for icons from nerd fonts
}
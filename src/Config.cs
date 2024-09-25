using Newtonsoft.Json;

namespace TermiSharp;

#nullable disable warnings

public sealed class Config
{
    static readonly string ConfigPath = Path.GetDirectoryName(Environment.ProcessPath) + "\\config.json";
    public static Config Load() => JsonConvert.DeserializeObject<Config>(File.ReadAllText(ConfigPath));
    
    public void Write() => File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));


    public bool FirstStart = true;
}
namespace TermiSharp.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
public sealed class SubCommandAttribute(string subCommand) : Attribute
{
    public string SubCommand { get; set; } = subCommand;
}
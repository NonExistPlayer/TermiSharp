namespace TermiSharp.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class OverrideCommandAttribute(string command) : Attribute
{
    public string Command { get; } = command;
}
namespace TermiSharp.Attributes;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
public sealed class CustomCommandNameAttribute(string cname) : Attribute
{
    public string CommandName { get; } = cname;
}
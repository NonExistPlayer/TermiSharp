using ConsoleTools;
using System.Reflection;

#nullable disable warnings

namespace TermiSharp;

public readonly struct Command(Action<object[]>? Handler,
    (byte Min, byte Max) ArgRange,
    Type[]? ArgTypes = null,
    MethodInfo? OtherHandler = null,
    bool Hidden = false,
    string[]? SubCommands = null)
{
    public readonly Action<object[]>? Handler { get; init; } = Handler;
    public void Call(string[] arg)
    {
        object[] args = new object[arg.Length];
        if (arg.Length < ArgRange.Min || arg.Length > ArgRange.Max)
        {
            Terminal.Writeln($"The arguments received go beyond the range of arguments.\n    Min: {ArgRange.Min}, Max: {ArgRange.Max}, Received: {arg.Length}", ConsoleColor.Red);
            return;
        }
        if (ArgTypes != null)
            for (int i = 0; i < arg.Length; i++)
            {
                MethodInfo? parse = ArgTypes[i].GetMethod("Parse", [typeof(string)]);
                if (ArgTypes[i] == typeof(object)) { args[i] = arg[i]; continue; }
                if (ArgTypes[i] == typeof(string))
                {
                    if (long.TryParse(arg[i], out _))
                    {
                        Terminal.Writeln($"The wrong type of argument or arguments.\n    Argument {i}, Excepted: System.String", ConsoleColor.Red);
                        return;
                    }
                    args[i] = arg[i];
                    continue;
                }
                try
                {
                    args[i] = parse.Invoke(null, [arg[i]]);
                } catch (TargetInvocationException ex)
                {
                    if (ex.InnerException is FormatException)
                        Terminal.Writeln($"The wrong type of argument or arguments.\n    Argument {i}, Excepted: {ArgTypes[i]}", ConsoleColor.Red);
                    else throw ex.InnerException;
                    return;
                }
            }
        if (Handler != null)
            Handler(arg);
        else
        {
            try
            {
                object[] defaultArgs = OtherHandler.GetParameters().Where(p => p.HasDefaultValue).Select(p => p.DefaultValue).ToArray();
                OtherHandler.Invoke(null, [.. arg, .. defaultArgs]);
            }
            catch (TargetParameterCountException)
            {
                Terminal.Writeln($"The arguments received go beyond the range of arguments.\n    Min: {ArgRange.Min}, Max: {ArgRange.Max}, Received: {arg.Length}", ConsoleColor.Red);
                return;
            }
            catch (TargetInvocationException ex)
            {
                Terminal.Writeln(ex.InnerException.ToString(), ConsoleColor.DarkRed);
            }
        }
    }
    public readonly (byte Min, byte Max) ArgRange { get; init; } = ArgRange;
    public readonly MethodInfo? OtherHandler = OtherHandler;
    public readonly Type[]? ArgTypes = ArgTypes;
    public readonly bool Hidden = Hidden;
    public readonly string[] SubCommands = SubCommands ?? [];
}
// This module is only for studying the creation of similar modules that modify the TermiSharp environment.
// Данный модуль является только для изучения создания подобных модулей которые изменяют среду TermiSharp.

using TermiSharp;
using TermiSharp.Attributes;

namespace BashShell;

public static class Module
{
    [NotACommand]
    public static void Init()
    {
        BashHost host = new(EntryPoint.MainHost.Commands.ToDictionary());
        EntryPoint.MainHost = host;
        EntryPoint.MainHost.Run();
    }
}
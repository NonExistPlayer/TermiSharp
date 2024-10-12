// This module is only for studying the creation of similar modules that modify the TermiSharp environment.
// Данный модуль является только для изучения создания подобных модулей которые изменяют среду TermiSharp.

using ConsoleTools;
using TermiSharp;

namespace BashShell;

#nullable disable

partial class BashHost
{
    public BashHost(Dictionary<string, Command> commands)
    {
        commands.Remove("mkf");   // touch
        commands.Remove("dir");   // ls
        commands.Remove("out");
        commands.Remove("outln"); // echo

        commands.Add("touch", new(arg =>
            File.Create(arg[0].ToString() ?? "").Dispose(),
        (1, 1)));
        commands.Add("pwd", new(
            arg => Console.WriteLine(CurrentPath)
        , (0, 0)));
        commands.Add("ls", new((arg) =>
        {
            string[] dirs = Directory.GetDirectories(CurrentPath).Select(d => Path.GetFileName(d) + '/').ToArray();
            string[] files = Directory.GetFiles(CurrentPath).Select(f => Path.GetFileName(f)).ToArray();
            bool newLine = (dirs.Sum(d => d.Length) + files.Sum(f => f.Length)) > Console.BufferWidth;

            Terminal.Write(dirs, newLine ? '\n' : ' ');
            if (newLine) Console.WriteLine();
            else         Console.Write(' ');
            Terminal.Writeln(files, newLine ? '\n' : ' ');
        }, (0, 0)));
        commands.Add("echo", new(
            arg => Console.WriteLine(arg[0]),
        (1, 1)));
        commands.Add("whoami", new(
            arg => Console.WriteLine(Environment.UserName),
        (0, 0)));
        commands.Add("cat", new(
            arg => Terminal.Writeln(File.ReadAllText(arg[0].ToString()), ConsoleColor.White),
        (1, 1)));

        _commands = commands;
    }
}
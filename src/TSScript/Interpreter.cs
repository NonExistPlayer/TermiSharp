using ConsoleTools;
using System.Xml;
using System.Xml.Linq;

#nullable disable warnings

namespace TermiSharp.TSScript;

internal static class Interpreter
{
    public static int Run(XDocument doc, string fname, ref bool userExited)
    {
        int Error(string message, int line = 0)
        {
            static void DrawIcon()
            {
                if (MainHost.Config.NerdFontsSupport)
                {
                    Terminal.Write("\ue0b6", ConsoleColor.Red);
                    Console.BackgroundColor = ConsoleColor.Red;
                    Terminal.Write("\uf071 ", ConsoleColor.White);
                    Terminal.Write("\ue0b4 ", ConsoleColor.Red);
                }
                Console.ForegroundColor = ConsoleColor.Red;
            }
            if (Console.CursorLeft > 0)
                Console.Error.WriteLine();
            Console.Beep();
            DrawIcon();
            Console.Error.WriteLine($"Execution Error!");
            DrawIcon();
            Console.Error.Write($"File: `{fname}`");
            if (line != 0)
            {
                Console.Error.WriteLine($", Line: {line}");
                DrawIcon();
                Console.Error.WriteLine(message);
            }
            else
            {
                Console.Error.WriteLine();
                DrawIcon();
                Console.Error.WriteLine(message);
            }
            Console.ResetColor();
            return -1;
        }
        XElement? root = doc.Root;
        if (root == null) 
            return Error("root element is missing");
        if (root.Name != "script")
            return Error("root element should be named as `script`");
        XNode[] nodes = root.Nodes().ToArray();
        for (int i = 0; i < nodes.Length; i++)
        {
            XNode node = nodes[i];
            int line = (node as IXmlLineInfo).LineNumber;
            void HandleText(XText text, ref string errMessage)
            {
                var code = text.Value.Split('\n').Where(l => !string.IsNullOrWhiteSpace(l));

                foreach (string command in code)
                {
                    try
                    {
                        MainHost.HandleCommand(command.TrimStart());
                    }
                    catch (Exception ex)
                    {
                        errMessage = ex.Message;
                    }
                    line++;
                }
            }
            string? errMessage = null;
            if (node is XText text)
                HandleText(text, ref errMessage);
            else if (node is XElement el)
            {
                switch (el.Name.LocalName)
                {
                    case "if":
                        if (!el.HasAttributes)
                            return Error("the element has no attribute", line);
                        if (el.Attributes().Count() > 1)
                            return Error("too many attributes", line);
                        XAttribute? condition = el.Attribute("condition");
                        if (condition == null)
                            return Error("can't find attribute `condition`", line);
                        
                        bool result = HandleCondition(condition.Value, ref errMessage);
                        if (result)
                            HandleText(new(el.Value), ref errMessage);
                        if (i + 1 < nodes.Length)
                        {
                            XNode node2 = nodes[i + 1];
                            if (node2 is XElement el2)
                                if (el2.Name == "else")
                                {
                                    if (!result)
                                        HandleText(new(el2.Value), ref errMessage);
                                    i++;
                                }
                        }
                        break;
                    case "while":
                        if (!el.HasAttributes)
                            return Error("the element has no attribute", line);
                        if (el.Attributes().Count() > 1)
                            return Error("too many attributes", line);
                        condition = el.Attribute("condition");
                        if (condition == null)
                            return Error("can't find attribute `condition`", line);
                        result = HandleCondition(condition.Value, ref errMessage);
                        while (result)
                        {
                            result = HandleCondition(condition.Value, ref errMessage);
                            if (result)
                                HandleText(new(el.Value), ref errMessage);
                        }
                        break;
                    case "exit":
                        if (el.Attributes().Count() > 1)
                            return Error("too many attributes", line);
                        if (el.Attributes().Count() == 1)
                        {
                            XName name = el.Attributes().First().Name;
                            if (name != "code")
                                return Error($"unexcepted attribute: `{name}`");
                        }
                        XAttribute? code = el.Attribute("code");
                        if (code == null)
                            return 0;
                        if (int.TryParse(code.Value, out int exitcode))
                        {
                            userExited = true;
                            return exitcode;
                        }
                        else
                            return Error("excepted an int32");
                    default:
                        return Error($"unexcepted element: `{el.Name}`", line);
                }
            }
            if (errMessage != null)
                return Error(errMessage, line);
        }
        return 0;
    }

    private static bool HandleCondition(string condition, ref string? errMessage)
    {
        string[] words = condition.TrimStart().TrimEnd().Split(' ');
        if (string.IsNullOrWhiteSpace(condition))
        {
            errMessage = "condition is empty";
            return false;
        }
        if (words.Length == 1)
        {
            if (bool.TryParse(condition, out bool result))
                return result;
            if (condition.StartsWith('@') || condition.StartsWith("!@"))
            {
                string varname = condition.StartsWith("!@") ? condition[2..] : condition[1..];
                if (!MainHost.Variables.TryGetValue(varname, out object? value))
                {
                    errMessage = "variable does not exist.";
                    return false;
                }
                if (value is bool res)
                    if (condition.StartsWith("!@"))
                        return !res;
                    else
                        return res;
                errMessage = "variable is not boolean";
                return false;
            }
        }
        if (words.Length == 3)
        {
            string? firstValue = null, secondValue = null;
            if (words[0].StartsWith('@'))
            {
                string varname = words[0][1..];
                if (!MainHost.Variables.TryGetValue(varname, out object? value))
                {
                    errMessage = "variable does not exist.";
                    return false;
                }
                firstValue = value.ToString() ?? "<NULL>";
            }
            if (words[0].StartsWith('$'))
            {
                string? value = Environment.GetEnvironmentVariable(words[0][1..]);

                if (value == null)
                {
                    errMessage = "variable does not exist.";
                    return false;
                }
                firstValue = value.ToString() ?? "<NULL>";
            }
            if (words[2].StartsWith('@'))
            {
                string varname = words[2][1..];
                if (!MainHost.Variables.TryGetValue(varname, out object? value))
                {
                    errMessage = "variable does not exist.";
                    return false;
                }
                secondValue = value.ToString() ?? "<NULL>";
            }
            if (words[2].StartsWith('$'))
            {
                string? value = Environment.GetEnvironmentVariable(words[2][1..]);

                if (value == null)
                {
                    errMessage = "variable does not exist.";
                    return false;
                }
                secondValue = value.ToString() ?? "<NULL>";
            }

            switch (words[1])
            {
                case "==":
                    return firstValue == secondValue;
                case "!=":
                    return firstValue != secondValue;
                default:
                    errMessage = $"excepted an operator, recevied: `{words[1]}`";
                    return false;
            }
        }

        errMessage = "unable to parse this condition";
        return false;
    }
}
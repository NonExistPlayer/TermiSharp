using ConsoleTools;
using ConsoleTools.Frames;
using System.Diagnostics;
using System.Data;
using System.Reflection;
using TermiSharp.ReadLine;
using System.Management;

#nullable disable warnings

namespace TermiSharp;

partial class ConsoleHost
{
    internal readonly Dictionary<string, string[]> SubCommands = [];

    public ConsoleHost()
    {
        bool ExeExists(string exe)
        {
            foreach (string path in ExePath)
                if (Directory.Exists(path))
                    foreach (string file in Directory.GetFiles(path))
                        if (Path.GetFileName(file) == exe + ".exe")
                            return true;
            return false;
        }

        SubCommands.Add("module", ["init", "list", "help"]);
        SubCommands.Add("config", ["set", "get", "add", "rem", "list", "help"]);
        if (OperatingSystem.IsWindows() && ExeExists("winget"))
            SubCommands.Add("winget", ["install", "show", "source", "search",
            "list", "upgrade", "uninstall", "hash", "validate", "settings",
            "features", "export", "import", "pin", "configure", "download",
            "repair"]);
        if (ExeExists("git"))
            SubCommands.Add("git", ["clone", "init", "add", "mv", "restore",
            "rm", "bisect", "diff", "grep", "log", "show", "status",
            "branch", "commit", "merge", "rebase", "reset", "switch", "tag",
            "fetch", "pull", "push"]);
        if (ExeExists("dotnet"))
            SubCommands.Add("dotnet", ["add", "build", "build-server", 
            "clean", "format", "help", "list", "msbuild", "new", "nuget",
            "pack", "publish", "remove", "restore", "run", "sdk", "sln",
            "store", "tool", "vstest", "workload"]);
    }

    void Init()
    {
        var INT = typeof(int);
        var STRING = typeof(string);
        var ANY = typeof(object);
        var RANGE = typeof(Range);
        var BOOL = typeof(bool);

        char sep = Path.DirectorySeparatorChar;

        string? INFO(string s)
        {
            string[] audio = ["mp3", "aiff", "ogg", "wav"];
            string[] video = [".mp4", "webm", ".mkv", ".avi", ".wmv"];
            string[] image = [".png", "jpeg", ".gif", "tiff", ".bmp"];
            if (s == "") return null;
            if (s.Length > 3)
                if (File.Exists(s) && audio.Contains(s[(s.Length - 3)..s.Length])) return "Audio";
            if (s.Length > 4)
            {
                if (File.Exists(s) && video.Contains(s[(s.Length - 4)..s.Length])) return "Video";
                if (File.Exists(s) && image.Contains(s[(s.Length - 4)..s.Length])) return "Image";
            }
            //if (s.StartsWith("https://"))
            //    if (IsSiteAvailable(s[8..])) return "Site";
            //if (s.StartsWith("http://"))
            //    if (IsSiteAvailable(s[7..])) return "Site";
            if (File.Exists(s)) return "File";
            if (Environment.GetLogicalDrives().Select(d => d[0..(d.Length - 1)]).Contains(s)) return "Drive";
            if (Directory.Exists(s)) return "Dir";
            if (Commands.ContainsKey(s)) return "Command";
            if (GetExePath(s) != null) return "ExePath";
            return null;
        }

        //static bool IsSiteAvailable(string site)
        //{
        //    try
        //    {
        //        return new Ping().Send(site, 1, new byte[32], new()).Status == IPStatus.Success;
        //    }
        //    catch
        //    {
        //        return false;
        //    }
        //}

        Commands.Add("clear", new((arg) => { Console.Clear(); }, (0, 0)));
        Commands.Add("c", new((arg) => {
            string expression = string.Join(' ', arg);
            try
            {
                DataTable table = new();
                table.Columns.Add("expression", typeof(string), expression);
                DataRow row = table.NewRow();
                table.Rows.Add(row);
                Console.WriteLine(double.Parse((string)row["expression"]));
            }
            catch
            {
                Terminal.Writeln("Invalid expression.", ConsoleColor.Red);
            }
        }, (1, 255)));
        Commands.Add("config", new((arg) =>
        {
            switch (arg[0].ToString())
            {
                case "set":
                    if (arg.Length != 3)
                    {
                        Terminal.Writeln("Excepted 2 arguments.", ConsoleColor.Red);
                        return;
                    }
                    string name = arg[1].ToString();
                    object value = arg[2];
                    FieldInfo? key = typeof(Config).GetField(name, BindingFlags.Public | BindingFlags.Instance);
                    if (key == null)
                    {
                        Terminal.Writeln("Specified parameter does not exist.", ConsoleColor.Red);
                        return;
                    }
                    if (int.TryParse(arg[2].ToString(), out int iResult))
                        value = iResult;
                    else if (bool.TryParse(arg[2].ToString(), out bool bResult))
                        value = bResult;
                    key.SetValue(Config, value);
                    break;
                case "get":
                    if (arg.Length != 2)
                    {
                        Terminal.Writeln("Excepted one more argument.", ConsoleColor.Red);
                        return;
                    }
                    name = arg[1].ToString();
                    key = typeof(Config).GetField(name, BindingFlags.Public | BindingFlags.Instance);
                    if (key == null)
                    {
                        Terminal.Writeln("Specified parameter does not exist.", ConsoleColor.Red);
                        return;
                    }
                    value = key.GetValue(Config);
                    if (value is string[] arr)
                        value = $"[{string.Join(", ", arr)}]";
                    Terminal.Writeln(value.ToString(), ConsoleColor.White);
                    break;
                case "add":
                    if (arg.Length != 3)
                    {
                        Terminal.Writeln("Excepted 2 arguments.", ConsoleColor.Red);
                        return;
                    }
                    name = arg[1].ToString();
                    value = arg[2];
                    key = typeof(Config).GetField(name, BindingFlags.Public | BindingFlags.Instance);
                    if (key == null)
                    {
                        Terminal.Writeln("Specified parameter does not exist.", ConsoleColor.Red);
                        return;
                    }
                    if (int.TryParse(arg[2].ToString(), out iResult))
                        value = iResult;
                    else if (bool.TryParse(arg[2].ToString(), out bool bResult))
                        value = bResult;
                    object obj = key.GetValue(Config);
                    if (obj is not string[])
                    {
                        Terminal.Writeln("Specified parameter is not array.", ConsoleColor.White);
                        return;
                    }
                    string[] strings = (string[])obj;
                    key.SetValue(Config, strings.Concat([value.ToString()]).ToArray());
                    break;
                case "rem":
                    if (arg.Length != 3)
                    {
                        Terminal.Writeln("Excepted 2 arguments.", ConsoleColor.Red);
                        return;
                    }
                    name = arg[1].ToString();
                    if (!int.TryParse(arg[2].ToString(), out int ind))
                    {
                        Terminal.Writeln("The type is expected for argument2: System.Int32", ConsoleColor.Red);
                        return;
                    }
                    key = typeof(Config).GetField(name, BindingFlags.Public | BindingFlags.Instance);
                    if (key == null)
                    {
                        Terminal.Writeln("Specified parameter does not exist.", ConsoleColor.Red);
                        return;
                    }
                    obj = key.GetValue(Config);
                    if (obj is not string[])
                    {
                        Terminal.Writeln("Specified parameter is not array.", ConsoleColor.White);
                        return;
                    }
                    List<string> list = ((string[])obj).ToList();
                    list.RemoveAt(ind);
                    key.SetValue(Config, list.ToArray());
                    break;
                case "list":
                    FieldInfo[] infos = typeof(Config).GetFields(BindingFlags.Instance | BindingFlags.Public);
                    foreach (var field in infos)
                    {
                        object val = field.GetValue(Config);
                        if (val is string[] array)
                            val = $"[{string.Join(", ", array)}]";
                        Terminal.Writeln($"{field.Name} = {val}", ConsoleColor.White);
                    }
                    break;
                case "help":
                    Terminal.DrawTable([
                        ["Command", "Description", "ArgCount", "ArgType"],
                        ["set", "Changes the value of the specified parameter", "2", "<Any>"],
                        ["get", "Gets the value of the specified parameter", "1", "<Any>"],
                        ["add", "Adds a value to the parameter if it is an array", "2", "<Any>"],
                        ["rem", "Removes the value from the parameter if it is an array", "2", "[ANY, INT]"],
                        ["list", "Shows a list of parameters", "0", "<Null>"],
                        ["help", "Shows this table", "0", "<Null>"]
                    ]);
                    break;
                default:
                    Terminal.Writeln("Unknown command.\nTip: Type `config help` to get list of \"config\" commands.", ConsoleColor.Red);
                    return;
            }
        }, (1, 3)));
        Commands.Add("hclear", new((arg) => {
            HistoryHandler.History.Clear();
            if (HistoryHandler.History.Count == 0)
                File.Delete($"{Path.GetDirectoryName(Environment.ProcessPath)}\\.history");
        }, (0, 0)));
        Commands.Add("make", new((arg) => { File.Create(arg[0].ToString()).Close(); }, (1, 1)));
        Commands.Add("mkdir", new((arg) => { Directory.CreateDirectory(arg[0].ToString()); }, (1, 1)));
        Commands.Add("exit", new((arg) => {
            if (arg.Length > 0)
                Environment.Exit(int.Parse((string)arg[0]));
            Environment.Exit(0);
        }, (0, 1), [INT]));
        Commands.Add("restart", new((arg) => {
            ProcessStartInfo info = new()
            {
                FileName = Environment.ProcessPath,
                Arguments = string.Join(' ', arg.Select(o => o.ToString()))
            };
            Process.Start(info);
            Environment.Exit(0);
        }, (0, 255)));
        Commands.Add("out", new((arg) => {
            Console.Write(arg[0].ToString());
        }, (1, 1)));
        Commands.Add("outln", new((arg) => { Console.WriteLine(arg[0]); }, (1, 1)));
        Commands.Add("ver", new((arg) => {
            if (Config.SimplifiedVersionWindow || OperatingSystem.IsLinux() )
            {
                Console.WriteLine($"TermiSharp {Version}\nby NonExistPlayer");
                return;
            }
            static void WriteColor(ConsoleColor color)
            {
                Console.BackgroundColor = color;
                Console.Write("         ");
                Console.BackgroundColor = ConsoleColor.Black;
            }
            string GetIcon(string icon) => Config.NerdFontsSupport ? icon : "";
            Console.Clear();
            Console.CursorVisible = false;
            Terminal.Writeln(
                         "       @@@@@@@@@@@@@@@@@@@@@@@@@@@@\r\n" +
                          "    @@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\r\n   " +
                            "@@@@@@@@@@@@@@@@@@@@@@@@ @@@ @@@ @@@\r\n   " +
                            "@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@@\r\n  \r\n   " +
                            "████████████████████████████████████\r\n   " +
                            "████████████████████████████████████\r\n   " +
                            "██████  ████████  ██  ██████████████\r\n   " +
                            "██████   ██████        █████████████\r\n   " +
                            "███████    █████  ██  ██████████████\r\n   " +
                            "███████    ████        █████████████\r\n   " +
                            "██████   ███████  ██  ██████████████\r\n   " +
                            "██████  ████████████████████████████\r\n   " +
                            " ██████████████████████████████████ \r\n   " +
                            "   ██████████████████████████████",
                ConsoleColor.Green);
            Console.SetCursorPosition(5, 2);
            Terminal.Write("TermiSharp", ConsoleColor.White);
            Console.SetCursorPosition(27, 2);
            Terminal.Write("-", ConsoleColor.White);
            Console.SetCursorPosition(31, 2);
            Terminal.Write("▢", ConsoleColor.White);
            Console.SetCursorPosition(35, 2);
            Terminal.Write("X", ConsoleColor.Red);

            ManagementObject cpu = new ManagementObjectSearcher("SELECT * FROM Win32_Processor").Get().Cast<ManagementObject>().FirstOrDefault();
            ManagementObject bios = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS").Get().Cast<ManagementObject>().FirstOrDefault();
            ManagementObject gpu = new ManagementObjectSearcher("SELECT * FROM Win32_VideoController").Get().Cast<ManagementObject>().FirstOrDefault();
            float availableMemory = new PerformanceCounter("Memory", "Available Bytes").NextValue() / 1024 / 1024 / 1024;
            float totalMemory = new PerformanceCounter("Memory", "Committed Bytes").NextValue()     / 1024 / 1024 / 1024;


            Frame display = [
               $"{GetIcon("\uf489 ")}TermiSharp {Version}",
                "by NonExistPlayer",
               $"{GetIcon("\uf085  ")}System Info:",
               $"   {GetIcon("\ue62a  ")}{Environment.OSVersion} {(Environment.Is64BitOperatingSystem ? "x64" : "x86")}",
               $"   {GetIcon("\uf109  ")}{Environment.MachineName}@{Environment.UserName} ",
               $"   {GetIcon("\ue72e  ")}.NET Version : {Environment.Version}",
               $"{GetIcon("\uf108  ")}Machine Info:",
               $"   {GetIcon("\ueabe  ")}Processor    : {cpu["Name"]}",
               $"   {GetIcon("\ue79d  ")}BIOS Version : {bios["Version"]}",
               $"   {GetIcon("\ueabe  ")}GPU          : {gpu["Name"]}",
               $"   {GetIcon("\uf03e  ")}GPU Memory   : {GetLength(long.Parse(gpu["AdapterRAM"].ToString()))}",
               $"   {GetIcon("\uefc5  ")}RAM          : {availableMemory:F2} GB / {totalMemory:F2} GB"
            ];

            Console.SetCursorPosition(41, 0);
            display.DrawTop();
            for (short i = 0; i < display.Count; i++)
            {
                Console.SetCursorPosition(41, i + 1);
                display.Draw(i);
            }
            Console.SetCursorPosition(41, display.Count + 1);
            display.DrawLow();
            Console.SetCursorPosition(41, 15);
            WriteColor(ConsoleColor.White);
            WriteColor(ConsoleColor.Red);
            WriteColor(ConsoleColor.Yellow);
            WriteColor(ConsoleColor.Green);
            WriteColor(ConsoleColor.Cyan);
            WriteColor(ConsoleColor.Blue);
            WriteColor(ConsoleColor.Magenta);
            Console.SetCursorPosition(41, 16);
            WriteColor(ConsoleColor.Gray);
            WriteColor(ConsoleColor.DarkRed);
            WriteColor(ConsoleColor.DarkYellow);
            WriteColor(ConsoleColor.DarkGreen);
            WriteColor(ConsoleColor.DarkCyan);
            WriteColor(ConsoleColor.DarkBlue);
            WriteColor(ConsoleColor.DarkMagenta);
            Console.SetCursorPosition(0, 17);
            Console.WriteLine();
            Console.CursorVisible = true;
        }, (0, 0)));
        Commands.Add("see", new((arg) => {
            if (!File.Exists(arg[0].ToString()))
            {
                Terminal.Writeln("Specified file does not exist.", ConsoleColor.Red);
                return;
            }
            string[] content = File.ReadAllLines(arg[0].ToString());
            if (arg.Length == 1)
            {
                Console.WriteLine(string.Join('\n', content));
                return;
            }
            if (Range.TryParse(arg[1].ToString()))
            {
                Range range = Range.Parse(arg[1].ToString());
                int end = range.End.GetValueOrDefault(range.Start);
                if (range.Start < 0 || range.Start > content.Length || end > content.Length || end < 0)
                {
                    Terminal.Writeln("The range goes beyond the file.", ConsoleColor.Red);
                    return;
                }

                Terminal.Writeln(content[range.Start..end]);
                return;
            }
            else if (int.TryParse(arg[1].ToString(), out _))
            {
                int line = int.Parse(arg[1].ToString());
                if (line < 0 || line > content.Length)
                {
                    Terminal.Writeln("The line number goes beyond the file.", ConsoleColor.Red);
                    return;
                }

                Console.WriteLine(content[line]);
            }
            else
                Terminal.Writeln("The wrong type of argument or arguments.\n    Argument 1, Excepted: TermiSharp.Range OR System.Int32", ConsoleColor.Red);
        }, (1, 2)));
        Commands.Add("see-bin", new((arg) =>
        {
            if (!File.Exists(arg[0].ToString()))
            {
                Terminal.Writeln("File doesn't exist.", ConsoleColor.Red); return;
            }
            Console.WriteLine(
                BitConverter.ToString(File.ReadAllBytes(arg[0].ToString())).Replace("-", " "));
        }, (1, 1)));
        Commands.Add("see-meta", new((arg) =>
        {
            if (!File.Exists(arg[0].ToString()))
            {
                Terminal.Writeln("Specified file does not exist.", ConsoleColor.Red);
                return;
            }
            IReadOnlyList<MetadataExtractor.Directory> directories;
            try
            {
                directories = MetadataExtractor.ImageMetadataReader.ReadMetadata(arg[0].ToString());
            }
            catch (Exception ex)
            {
                Terminal.Writeln($"Error: {ex.Message}", ConsoleColor.Red);
                return;
            }
            List<string[]> table = [];

            foreach (var directory in directories)
            {
                foreach (var tag in directory.Tags)
                    table.Add([directory.Name, tag.Name, tag.Description]);

                if (directory.HasError)
                {
                    Terminal.Writeln("Something went wrong...", ConsoleColor.Red);
                    break;
                }
            }

            table.Reverse();

            table.Insert(0, ["Type", "Name", "Value"]);

            Terminal.DrawTable(table);
        }, (1, 1)));
        Commands.Add("cd", new((arg) =>
        {
            string dir = arg[0].ToString();
            if (dir == "..")
            {
                CurrentPath = Path.GetDirectoryName(CurrentPath) ?? CurrentPath[0..(CurrentPath.IndexOf(':') + 1)];
                return;
            }
            if (!Directory.Exists(dir))
            {
                Terminal.Writeln($"Directory '{dir}' doesn't exists.", ConsoleColor.Red);
                return;
            }
            
            try
            {
                Directory.GetDirectories(Path.GetFullPath(dir));
            }
            catch (UnauthorizedAccessException)
            {
                Terminal.Writeln($"Access denied.", ConsoleColor.Red);
                return;
            }
            
            CurrentPath = GetCorrectCasePath(Path.GetFullPath(dir));
        }, (1, 1)));
        Commands.Add("dir", new((arg) =>
        {
            FileInfo[] files = Directory
                .GetFiles(CurrentPath)
                .Select(d => new FileInfo(d))
                .ToArray();
            DirectoryInfo[] directories = new DirectoryInfo(CurrentPath).GetDirectories();
            int[] largest = [0, 0];
            List<string> info = [];
            foreach (var d in directories)
            {
                if (d.Name.Length > largest[0])
                    largest[0] = d.Name.Length;
                if (d.CreationTime.ToString().Length > largest[1])
                    largest[1] = d.CreationTime.ToString().Length;
            }
            largest[0] += 3;
            largest[1] += 5;
            info.Add(string.Join('\n',
                directories
                .Select(d => $"{d.Name}{new string(' ', largest[0] - d.Name.Length)}" +
                $"{d.CreationTime}{new string(' ', largest[1] - d.CreationTime.ToString().Length)}")
            ) + '\n');
            largest = [0, 0, 0, 0];
            foreach (var f in files)
            {
                if (f.Name.Length > largest[0])
                    largest[0] = f.Name.Length;
                if (f.CreationTime.ToString().Length > largest[1])
                    largest[1] = f.CreationTime.ToString().Length;
                if (f.LastWriteTime.ToString().Length > largest[2])
                    largest[2] = f.LastWriteTime.ToString().Length;
                if (GetLength(f.Length).Length > largest[3])
                    largest[3] = GetLength(f.Length).Length;
            }
            largest[0] += 3;
            largest[1] += 5;
            largest[2] += 5;
            largest[3] += 5;
            info.Add(string.Join('\n',
                files
                .Select(f => $"{f.Name}{new string(' ', largest[0] - f.Name.Length)}" +
                $"{GetLength(f.Length)}{new string(' ', largest[3] - GetLength(f.Length).Length)}" +
                $"{f.CreationTime}{new string(' ', largest[1] - f.CreationTime.ToString().Length)}" +
                $"{f.LastWriteTime}{new string(' ', largest[2] - f.LastWriteTime.ToString().Length)}")
            ));

            Terminal.Writeln($"――――――――――`{CurrentPath[(CurrentPath.LastIndexOf(sep) + 1)..]}` directory at {CurrentPath[0..(CurrentPath.IndexOf(sep) + 1)]}――――――――――", ConsoleColor.White);

            // draw directories
            for (int i = 0; i < directories.Length; ++i)
            {
                string name = directories[i].Name;
                try
                {
                    Directory.GetDirectories(CurrentPath + $"{sep}{name}");
                }
                catch (UnauthorizedAccessException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.Write(name);
                Console.ResetColor();
                Console.WriteLine(info[0].Split('\n')[i].Split(name)[1]);
            }

            Console.WriteLine();

            // draw files
            for (int i = 0; i < files.Length; ++i)
            {
                string name = files[i].Name;
                try
                {
                    File.Open(name, FileMode.Open, FileAccess.Read).Close();
                }
                catch (IOException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                catch (UnauthorizedAccessException)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                }
                Console.Write(name);
                Console.ResetColor();
                Console.WriteLine(info[1].Split('\n')[i].Split(name)[1]);
            }

            //Console.Write('\n');
        }, (0, 0)));
        Commands.Add("module", new((arg) =>
        {
            switch (arg[0].ToString())
            {
                case "init":
                    if (arg.Length == 1)
                    {
                        Terminal.Writeln("Requires one more argument", ConsoleColor.Red);
                        return;
                    }
                    ModuleInit(arg[1].ToString());
                    break;
                case "list":
                    List<string> inscribed = [];
                    Command[] commands = Commands.Values.Where(c => {
                        if (c.OtherHandler == null) return false;
                        if (inscribed.Contains(c.OtherHandler.DeclaringType.Namespace)) return false;
                        inscribed.Add(c.OtherHandler.DeclaringType.Namespace);
                        return true;
                    }).ToArray();
                    if (commands.Length <= 0)
                    {
                        Terminal.Writeln("Not a single module has been imported.", ConsoleColor.Red);
                        return;
                    }
                    Frame display = [
                        $"――――――――――Modules Count: {commands.Length}――――――――――"
                    ];
                    display.AddRange(commands.Select(c => $"{c.OtherHandler.DeclaringType.Namespace}:" +
                    $" {string.Join(',', c.OtherHandler
                    .DeclaringType
                    .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static).Select(m => m.Name))}").ToArray());
                    display.Draw();
                    break;
                case "help":
                    Terminal.DrawTable([
                        ["Command", "Description", "ArgCount", "ArgType"],
                        ["init", "Initializes the specified module", "1", "<Any>"],
                        ["list", "Shows a list of initialized modules", "0", "<Null>"],
                        ["help", "Shows this table", "0", "<Null>"]
                    ]);
                    break;
                default:
                    Terminal.Writeln("Unknown command.\nTip: Type `module help` to get list of \"module\" commands.", ConsoleColor.Red);
                    return;
            }
        }, (1, 3)));
        Commands.Add("info", new((arg) =>
        {
            Frame display = [];
            switch (INFO(arg[0].ToString()))
            {
                case "File":
                    string f = Path.GetFullPath(arg[0].ToString());
                    bool accessdenied = false;
                    var info = new FileInfo(f);
                    try
                    {
                        info.Open(FileMode.Open, FileAccess.Read).Close();
                    }
                    catch (IOException)
                    {
                        accessdenied = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        accessdenied = true;
                    }
                    //if (IsSiteAvailable("fileinfo.com"))
                    //{
                    //    using HttpClient client = new();
                    //    HttpResponseMessage response;
                    //    response = client.GetAsync($"https://fileinfo.com/extension/{info.Extension[1..]}").Result;
                    //    try
                    //    {
                    //        response.EnsureSuccessStatusCode();
                    //    }
                    //    catch { goto ignore; }

                    //    string responseBody = response.Content.ReadAsStringAsync().Result;

                    //    HtmlDocument doc = new();
                    //    doc.LoadHtml(responseBody);

                    //    var filetypes = doc.DocumentNode.SelectNodes("//h2[█class='title']");
                    //    if (filetypes != null && filetypes.Count > 0)
                    //        designation = filetypes[0].InnerText;
                    //}

                    display.AddRange([
                        $"Information of `{f}` as File:",
                        $"Name             : {Path.GetFileNameWithoutExtension(f)}",
                        $"Extension        : {info.Extension}",
                        $"Size             : {GetLength(info.Length)}",
                        $"Last Write Time  : {info.LastAccessTime}",
                        $"Last Access Time : {info.LastAccessTime}",
                        $"Access Denied    : {accessdenied}",
                         "[Attributes]",
                        $"   Hidden    = {info.Attributes.HasFlag(FileAttributes.Hidden)}",
                        $"   Read Only = {info.IsReadOnly}",
                        $"   System    = {info.Attributes.HasFlag(FileAttributes.System)}",
                    ]);
                    break;
                case "Audio":
                    f = Path.GetFullPath(arg[0].ToString());
                    info = new FileInfo(f);
                    var tag = TagLib.File.Create(f).Tag;

                    //if (IsSiteAvailable("fileinfo.com"))
                    //{
                    //    using HttpClient client = new();
                    //    HttpResponseMessage response;
                    //    response = client.GetAsync($"https://fileinfo.com/extension/{info.Extension[1..]}").Result;
                    //    try
                    //    {
                    //        response.EnsureSuccessStatusCode();
                    //    }
                    //    catch { goto ignore1; }

                    //    string responseBody = response.Content.ReadAsStringAsync().Result;

                    //    HtmlDocument doc = new();
                    //    doc.LoadHtml(responseBody);

                    //    var filetypes = doc.DocumentNode.SelectNodes("//h2[█class='title']");
                    //    if (filetypes != null && filetypes.Count > 0)
                    //        designation = filetypes[0].InnerText;
                    //}
                    display.AddRange([
                        $"Information of `{f}` as Audio:",
                        $"Name             : {Path.GetFileNameWithoutExtension(f)}",
                        $"Extension        : {info.Extension}",
                        $"Size             : {GetLength(info.Length)}",
                        $"Last Write Time  : {info.LastAccessTime}",
                        $"Last Access Time : {info.LastAccessTime}",
                         "[Audio]",
                        $"   Title   = {tag.Title}",
                        $"   Artists = {string.Join(',', tag.Performers)}",
                        $"   Album   = {tag.Album}",
                        $"   Year    = {tag.Year}",
                         "[Attributes]",
                        $"   Hidden    = {info.Attributes.HasFlag(FileAttributes.Hidden)}",
                        $"   Read Only = {info.IsReadOnly}",
                        $"   System    = {info.Attributes.HasFlag(FileAttributes.System)}",
                    ]);
                    break;
                case "Video":
                    f = Path.GetFullPath(arg[0].ToString());
                    info = new FileInfo(f);
                    tag = TagLib.File.Create(f).Tag;

                    //if (IsSiteAvailable("fileinfo.com"))
                    //{
                    //    using HttpClient client = new();
                    //    HttpResponseMessage response;
                    //    response = client.GetAsync($"https://fileinfo.com/extension/{info.Extension[1..]}").Result;
                    //    try
                    //    {
                    //        response.EnsureSuccessStatusCode();
                    //    }
                    //    catch { goto ignore2; }

                    //    string responseBody = response.Content.ReadAsStringAsync().Result;

                    //    HtmlDocument doc = new();
                    //    doc.LoadHtml(responseBody);

                    //    var filetypes = doc.DocumentNode.SelectNodes("//h2[█class='title']");
                    //    if (filetypes != null && filetypes.Count > 0)
                    //        designation = filetypes[0].InnerText;
                    //}

                    display.AddRange([
                        $"Information of `{f}` as Video:",
                        $"Name             : {Path.GetFileNameWithoutExtension(f)}",
                        $"Extension        : {info.Extension}",
                        $"Size             : {GetLength(info.Length)}",
                        $"Last Write Time  : {info.LastAccessTime}",
                        $"Last Access Time : {info.LastAccessTime}",
                         "[Video]",
                        $"   Title   = {tag.Title}",
                        $"   Artists = {string.Join(',', tag.Performers)}",
                        $"   Year    = {tag.Year}",
                         "[Attributes]",
                        $"   Hidden    = {info.Attributes.HasFlag(FileAttributes.Hidden)}",
                        $"   Read Only = {info.IsReadOnly}",
                        $"   System    = {info.Attributes.HasFlag(FileAttributes.System)}",
                    ]);
                    break;
                case "Dir":
                    string d = Path.GetFullPath(arg[0].ToString());
                    accessdenied = false;
                    var dinfo = new DirectoryInfo(d);
                    try
                    {
                        dinfo.GetDirectories();
                    }
                    catch (UnauthorizedAccessException)
                    {
                        accessdenied = true;
                    }
                    display.AddRange([
                        $"Information of `{d}` as Directory:",
                        $"Name             : {Path.GetDirectoryName(d)}",
                        $"Last Write Time  : {dinfo.LastAccessTime}",
                        $"Last Access Time : {dinfo.LastAccessTime}",
                        $"Access Denied    : {accessdenied}",
                         "[Attributes]",
                        $"   Hidden    = {dinfo.Attributes.HasFlag(FileAttributes.Hidden)}",
                        $"   System    = {dinfo.Attributes.HasFlag(FileAttributes.System)}",
                    ]);
                    break;
                case "Drive":
                    string disk = arg[0].ToString() + sep;
                    var drinfo = DriveInfo.GetDrives().Where(d => d.RootDirectory.FullName == disk).ToArray()[0];
                    try
                    {
                        display.AddRange([
                            $"Information of `{drinfo.VolumeLabel} ({arg[0]})` as Drive:",
                            $"{GetLength(drinfo.AvailableFreeSpace)} / {GetLength(drinfo.TotalSize)}",
                            $"File System : {drinfo.DriveFormat}",
                            $"Type        : {drinfo.DriveType}",
                        ]);
                    }
                    catch (IOException)
                    {
                        Terminal.Writeln("Device is not ready.", ConsoleColor.Red); return;
                    }
                    break;
                default:
                    Terminal.Writeln($"`{arg[0]}` is not a file, audio, video, image, folder,\ninternal or external command, disk.", ConsoleColor.Red);
                    return;
            }
            display.Draw();
        }, (1, 1)));
        Commands.Add("rm", new((arg) =>
        {
            string f = arg[0].ToString();
            if (File.Exists(f))
            {
                try
                {
                    File.Delete(f);
                }
                catch (UnauthorizedAccessException)
                {
                    Terminal.Writeln("Access denied.", ConsoleColor.Red);
                }
                catch (IOException)
                {
                    Terminal.Writeln("File already using.", ConsoleColor.Red);
                }
            }
            else if (Directory.Exists(f))
            {
                try
                {
                    Directory.Delete(f);
                }
                catch (UnauthorizedAccessException)
                {
                    Terminal.Writeln("Access denied.", ConsoleColor.Red);
                }
                catch (IOException)
                {
                    Terminal.Writeln("Access denied.", ConsoleColor.Red);
                }
            }
            else
            {
                Terminal.Writeln($"`{f}` is not a folder or file", ConsoleColor.Red);
            }
        }, (1, 1)));
        Commands.Add("sleep", new((arg) =>
        {
            Thread.Sleep(int.Parse(arg[0].ToString()));
        }, (1, 1), [INT]));
        Commands.Add("cp", new((arg) =>
        {
            string f = arg[0].ToString();
            string to = arg[1].ToString();
            if (!File.Exists(f) && !Directory.Exists(f))
            {
                Terminal.Writeln($"`{f}` is not directory or file", ConsoleColor.Red);
                return;
            }

            to = Path.GetFullPath(to);
            if (!Tools.TryActionWithFile(() => {
                if (File.Exists(f))
                    File.Copy(f, to);
                else
                    Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(Path.GetFullPath(f), to + Path.GetDirectoryName(f));
            })) return;
        }, (2, 2)));
        Commands.Add("mv", new((arg) =>
        {
            string f = arg[0].ToString();
            string to = arg[1].ToString();
            if (!File.Exists(f) && !Directory.Exists(f))
            {
                Terminal.Writeln($"`{f}` is not directory or file", ConsoleColor.Red);
                return;
            }

            to = Path.GetFullPath(to);
            if (!Tools.TryActionWithFile(() => {
                if (File.Exists(f))
                    File.Move(f, to);
                else
                    Microsoft.VisualBasic.FileIO.FileSystem.MoveDirectory(Path.GetFullPath(f), to);
            })) return;
        }, (2, 2)));
        Commands.Add("help", new((arg) =>
        {
            List<string[]> table = [
                ["Command", "Argument Range", "Arguments Type", "Module"]
            ];

            if (arg.Length > 0)
            {
                string com = arg[0].ToString();
                if (!Commands.TryGetValue(com, out Command val))
                {
                    Terminal.Writeln("Command does not exist.", ConsoleColor.Red);
                    return;
                }
                table.Add([com, val.ArgRange.ToString(), val.ArgTypes != null ? '[' + string.Join(',',
                    val.ArgTypes.Select(t => t.Name != "Object" ? t.Name : "Any")) + ']'
                    : "<Any>",
                    val.OtherHandler != null ? val.OtherHandler.DeclaringType.Namespace : (com.StartsWith("debug") ? "std-debug" : "std")]);
            }
            else
            {
                foreach (var command in NHCommands.Values)
                    table.Add([Commands.FindKey(command), command.ArgRange.ToString(), command.ArgTypes != null ? '[' + string.Join(',',
                    command.ArgTypes.Select(t => t.Name != "Object" ? t.Name : "Any")) + ']'
                    : "<Any>",
                    command.OtherHandler != null ? command.OtherHandler.DeclaringType.Namespace : "std"]);
            }

            Terminal.DrawTable(table);
        }, (0, 1), [STRING]));
        // Debug
        Commands.Add("debug-lexes", new((arg) => { Terminal.Writeln(MainHost.GetLocalExes().ToArray(), ' '); }, (0, 0), Hidden: true));
        Commands.Add("debug-varlist", new((arg) => { Terminal.Writeln(MainHost.Variables.Select(v => $"{v.Key}={v.Value}").ToArray(), '\n'); }, (0, 0), Hidden: true));
        Commands.Add("debug-findexe", new((arg) => {
            foreach (string path in ExePath)
                if (Directory.Exists(path))
                    foreach (string file in Directory.GetFiles(path))
                    {
                        if (Path.GetFileName(file) == arg[0].ToString())
                            Terminal.Writeln(file, ConsoleColor.White);
                    }
        }, (1, 1), [STRING], Hidden: true));
        Commands.Add("debug-lasterr", new((arg) => 
        {
            if (LastException != null)
                Terminal.Writeln(LastException.ToString(), ConsoleColor.White);
        }, (0, 0), Hidden: true));
        Commands.Add("debug-throw", new((arg) =>
        {
            if (arg.Length == 0)
                throw new Exception("debug");
            Type type = Type.GetType(arg[0].ToString());
            if (type == null) return;
            throw (Exception)Activator.CreateInstance(type);
        }, (0, 1), [STRING], Hidden: true));
    }
}
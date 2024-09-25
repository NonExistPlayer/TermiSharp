[Русская версия](docs/ru/Знакомство.md)
# TermiSharp
TermiSharp is a terminal that tries to be beautiful and convenient to use.

The terminal is primarily aimed at the Windows platform, as in the opinion of the developer (me), there are very few beautiful terminals on Windows.
## Project Information and About Me
I won't reveal my real name, but I can say that at the time of version 1.0, I was 11 years old, and that's no joke.

I'm passionate about programming, and this is one of the few projects that I'm proud of and admire.

I really like C#. That's why my first three projects are in C#.

Now, about TermiSharp itself: TermiSharp and its modules are fully written in C#.

I started working on this project on May 24, 2024 (if you believe the creation date of [TermiSharp.sln](../../TermiSharp.sln)). It turns out that it's been 5 months, but that's not true. I didn't work on the project for the entire months of July and June, so it's actually been 3 months.

Help with developing the project is welcome: nonexistplayer@gmail.com.

## Getting Started
Attention! Your OS must be Windows 7 or higher.

It's recommended to use Windows Terminal (wt.exe) for more customization options.

After installing Windows Terminal and TermiSharp, it's recommended to set up a profile in Windows Terminal:

* Open the settings (Ctrl + ,)
* Click on the "Add new profile" button on the left.
* Click on "New empty profile".
* Click on "Command line" and specify the path to the TermiSharp executable file (`termisharp.exe`)
* Now, click on "Launch" on the left, and you can optionally set TermiSharp as the default terminal for Windows Terminal. Now you can run TermiSharp.
* You can now use TermiSharp and customize it! More information To view the list of commands, you can type the help command or view below.

### Command List
Commands will be listed in the following format: Name - Description - Min. number of arguments, max. number of arguments

* `clear` - Clears the console - 0, 0
* `c` - Evaluates an expression - 1, 255
* `cr` - Creates a file - 1, 1
* `cr-dir` - Creates a directory - 1, 1
* `exit` - Exits the application - 0, 1
* `restart` - Restarts TermiSharp - 0, 0
* `out` - Outputs the specified text without a newline character at the end - 1, 1
* `outln` - Outputs the specified text - 1, 1
* `ver` - Displays the initial screen - 0, 0
* `see` - Outputs the contents of the specified file - 1, 2
* `see-bin` - Outputs the contents of the specified file in bytes - 1, 1
* `see-meta` - Outputs the metadata of the specified file - 1, 1
* `cd` - Changes the directory - 1, 1
* `dir` - Outputs all files and directories in the current directory - 0, 0
* `module` - Initializes modules and lists modules - 1, 3
* `info` - Displays information about the file - 1, 1
* `rm` - Deletes a directory or file - 1, 1
* `sleep` - Pauses execution for the specified time - 1, 1
* `help` - Displays the list of commands - 0, 1

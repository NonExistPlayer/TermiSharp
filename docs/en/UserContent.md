# User Content in TermiSharp 
TermiSharp can work with user content, such as:

* Modules of commands
* .csx scripts

This is enough for a small customization of your TermiSharp.

Let's break it down:

## Modules
What are modules in the context of TermiSharp?

They are additional packages of commands that can be used in TermiSharp. Commands can be absolutely any, performing any actions that are possible in C#. However, it's essential to note that not all types of arguments are supported. Only those with static methods TryParse and Parse are supported, which allow converting a string to that type.

Modules are written in C#. Let's take a look at an example.
```C#
using System;

namespace MyTPPModule; // <-- namespace must match the module name

public static class Module { // <-- all public static methods will be considered commands
	public static void mycommand() {
		Console.WriteLine("This is my first command!");
	}
}
```
Now, if we initialize this module using `module init <module_name>`, then the `help` command will display a new command, in this case: `mycommand`.

Before starting your module project, it's recommended to add a reference to `termisharp.dll` in the folder with the executable file of the program, as this will allow interaction with the terminal.

Now, let's try to create a command that will display a list of executable files in the current directory.
```C#
using System;
using NotALibrary.ConsoleTools;
using TerminalPP;
using TerminalPP.Attributes;

namespace MyTPPModule;

public static class Module {
	public static void get_local_exes() {
		Terminal.Writeln(MainHost.GetLocalExes().ToArray(), ' ');
	}
}
```
### Attributes from `TerminalPP.Attributes`
But `get_local_exes` doesn't look nice. Sometimes you need to change the command name to something else, but not change the method name.

Let's say we want to get `get-local-exes` instead of `get_local_exes`, but C# syntax doesn't allow creating a method with such a name.

Attributes from the `TerminalPP.Attributes` namespace come to the rescue:

In this namespace, there are attributes that allow marking certain methods in the Module class. There is an attribute called `CustomCommandName`, which, as the name suggests, changes the command name ignoring the method name, for example:
```C#
using System;
using NotALibrary.ConsoleTools;
using TerminalPP;
using TerminalPP.Attributes;

namespace MyTPPModule;

public static class Module {
	[CustomCommandName("get-local-exes")]
	public static void get_local_exes() {
		Terminal.Writeln(MainHost.GetLocalExes().ToArray(), ' ');
	}
}
```
Now, when importing our module, instead of the `get_local_exes` command, we get `get-local-exes`.

`get-local-exes` also better matches the structure of "spaces" in standard TermiSharp commands.

It may also happen that a certain method should be public, but it's not a command. Then you need to use the `NotACommand` attribute, which marks this method as not being a command for TermiSharp.

Example:
```C#
using System;
using NotALibrary.ConsoleTools;
using TerminalPP;
using TerminalPP.Attributes;

namespace MyTPPModule;

public static class Module {
	[CustomCommandName("get-local-exes")]
	public static void get_local_exes() {
		Terminal.Writeln(MainHost.GetLocalExes().ToArray(), ' ');
	}

	[NotACommand]
	public static void SomeMethod() {
		// some code...
	}
}
```
It may happen that you want to override an existing command in your module, for example, improve it. This can be done using the `OverrideCommandAttribute`:
```C#
using System;
using NotALibrary.ConsoleTools;
using TerminalPP;
using TerminalPP.Attributes;

namespace MyTPPModule;

public static class Module {
	[OverrideCommand("see")]
	public static void NewSeeCommand(string filepath) {
		// some code...
	}
}
```
Using the `CustomCommandName` attribute together with OverrideCommand is not required.

Also, if your command has "sub-commands" (example: `git <sub-command>`), you can use the `SubCommandAttribute`. It allows you to highlight your sub-command and also provide a hint for the user.

## Scripts
All scripts and programs that should be accessible from any directory (folder) in TermiSharp should be located here:

`<path-to-TermiSharp.exe\Scripts>`

TermiSharp supports the execution of the following executable files:
`.exe`, `.bat`, `.cmd`, `.csx`.

To execute one of these executable files, it is enough to enter the name of the executable file in the terminal.
If you have done everything correctly, and the executable file exists, then its name will be green.
### C# Scripts (`.csx`)
C# scripts are not much different from C#. So if you know C# well, then obviously your knowledge of C# scripts
will not be useless. TermiSharp when executed .the css file imports "itself". That is, you can use
the namespace `TermiSharp` in the .css file, that is, it turns out that you are given a small access to the terminal via .csx file.

C# scripts do not have to be in the "Scripts" folder, they can be in your current folder or in one of the folders
that is written to the `PATH` environment variable.

.csx scripts are very easy to replace with a full-fledged C# module. And .csx files should only be used for small
tasks. It 's not worth doing a bunch .csx files as commands, it would be better to make one C# module.
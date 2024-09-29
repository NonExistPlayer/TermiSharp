# TermiSharp ChangeLog
## 1.0.1
* Fix the work of `bool`[`ExeExists`](src/Commands.cs#L20)`(string)`.
* Added the "AutoModulesInit" key to `config.json'. [More details](src/Config.cs#L16).
* Fixed a bug in [code](src/ConsoleHost.cs#L351), now attributes for methods in modules work correctly.
* Added the [`config`](src/Commands.cs#L106) command.
* Fixed a bug where when entering a command, and then it is a sub-command, then any text after it will be yellow. [More details](src/ReadLine/HighlightHandler.cs#L20).
* Now the history of entries in the terminal command is stored in `.history`.
* Added the [`hclear`](src/Commands.cs#L231) command, which clears the history.
* Added the "DisableHistoryFile" key to `config.json'. [More details](src/Config.cs#L17).
## 1.1
* Now it's a team like:<br>
 `see`, `see-bin`, `see-meta`, `info`, `mv`, `rm`, `cp`<br>
 There will be a highlight of the first argument and a hint.
* The logic of `TermiSharp` has been changed `TermiSharp.ReadLine.HintHandler` [more details](src/ReadLine/HintHandler.cs#L11).
* The commands `dotnet.exe` have been added to the list under the command [more details](src/Commands.cs#L40).
* Now, when prompted, clear the history via `hclear`, if it is already empty, then the `.history` file will be deleted.
* Now, along with the `mk` command, there will be: `make`.
* Now, when an exception is made, only the message is displayed.
* The `debug-lasterr` command has been added, which outputs information about the subsequent exception.
* Now, if the command from the module has the name of an existing command, then the refinement is added `<module-name>.<command-name>` [more details](src/ConsoleHost.cs#L368).
* Now the command from the module can be hidden via the [`HiddenCommandAttribute`](src/Attributes/HiddenCommandAttribute.cs) attribute.
* The TermiSharp logo has been changed, which is output via the 'ver` command.
* Now the ">" symbol will be " #" together [more details](src/ConsoleHost.cs#L129).
* Now the `ver` command has a more appropriate menu on the right.
* Now the main menu (the `ver` command) can be disabled like this:<br>
`config set ShowVersionOnStart false`
* Now you can strengthen the initial menu like this:<br>
`config set SimplifiedVersionWindow false`
* You can now hide the start menu with the argument: `--hideversion`.
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
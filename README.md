[Русская версия](docs/Знакомство.md)
# Terminal++
Terminal++ is a terminal that tries to be beautiful and easy to use.

The terminal is primarily aimed at the Windows platform, since according to the developer (me) there are very few beautiful terminals on Windows.
## Information about the project and about me
I won't reveal my real name. I can only say that at the time of version 1.0 I was 11 years old and this is not a joke.

I am fond of programming and this is one of the few projects I admire and am satisfied with.

I really like C#. That's why my first 3 projects are in C#.

Now about Terminal++ itself:

Terminal++ and its modules are completely written in C#.

Terminal++ and its modules are completely written in C#.

I started doing this project on May 24, 2024 (if you believe the creation date of `Terminal++.sln').

It turns out that it is 5 months, but it is not so. I haven't done any projects all July and June, so it turns out that it's been 3 months.

Assistance in the development of the project is welcome: [nonexistplayer@gmail.com](mailto:nonexistplayer@gmail.com).
## Getting started
**Attention! Your OS must be at least Windows 7.**

It is recommended to use [Windows Terminal] for more customization of Terminal++(https://github.com/microsoft/terminal) (`wt.exe`).
Also, after installing Windows Terminal and Terminal++, it is recommended to set up a profile in Windows Terminal:
1. Open the settings (Ctrl + ,)
2. Click on the "Add new profile" button on the left.
3. Click on "New Empty Profile".
4. Click on "Command Prompt" and specify the path to the Terminal++ executable file (`terminal++.exe`)
5. Now click on "Launch" on the left and optionally you can set Terminal++ as the main terminal for Windows Terminal.
Now you can run Terminal++.

Now you can use Terminal++, as well as customize it!

Now you can use Terminal++, as well as customize it! More details

In order to study the list of commands, you can write the `help` command or view it below.
### List of commands
The command will be specified as follows:
Name - Description - Min. number of arguments, max. number of arguments
* `clear`    - Clears the console                                                - 0, 0
* `c`        - Evaluates the expression                                          - 1, 255
* `cr`       - Creates a file                                                    - 1, 1
* `cr-dir`   - Creates a folder                                                  - 1, 1
* `exit`     - Exits the application                                             - 0, 1
* `restart`  - Restarts Terminal++                                               - 0, 0
* `out`      - Outputs the specified text without a newline character at the end - 1, 1
* `outln`    - Outputs the specified text                                        - 1, 1
* `ver`      - Displays the version of Terminal++ and other information          - 0, 0
* `see`      - Outputs the contents of the specified file                        - 1, 2
* `see-bin`  - Outputs the contents of the specified file in bytes               - 1, 1
* `see-meta` - Outputs the metadata of the specified file                        - 1, 1
* `cd`       - Changes the directory                                             - 1, 1
* `dir`      - Displays all files and folders in the current directory           - 0, 0
* `module`   - Initialization of modules and a list of modules                   - 1, 3
* `info`     - Displays information about the file                               - 1, 1
* `rm`       - Deletes a folder or file                                          - 1, 1
* `sleep`    - Freeze execution for the specified time                           - 1, 1
* `help`     - Displays a list of commands                                       - 0, 1

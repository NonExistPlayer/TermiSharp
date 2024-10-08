# TermiSharp Changelog
## 1.1.1
* Добавлена команда [`debug-throw`](https://github.com/NonExistPlayer/TermiSharp/blob/v1.1.1/src/Commands.cs#L913) для отладки исключений.
* Теперь при исключении если [`NerdFontsSupport`](https://github.com/NonExistPlayer/TermiSharp/blob/v1.1.1/src/Config.cs#L19) включено, то выводится иконка ошибки.
* Добавлено отображение текущей ветки в репозиториях.
* Исправлено наименование [`HiddenCommandAttribute`](https://github.com/NonExistPlayer/TermiSharp/blob/v1.1.1/src/Attributes/HiddenCommandAttribute.cs) из пространства имён [`TermiSharp.Attributes`](https://github.com/NonExistPlayer/TermiSharp/blob/v1.1.1/src/Attributes).
* Добавлено выполнение TSS (TermiSharpScript) скриптов.
## 1.1
* Теперь у таких команд как:<br>
 `see`, `see-bin`, `see-meta`, `info`, `mv`, `rm`, `cp`<br>
 Будет подсветка первого аргумента и подсказка.
* Изменена логика работы `TermiSharp.ReadLine.HintHandler` [подробнее](src/ReadLine/HintHandler.cs#L11).
* В список под команд добавлены команды `dotnet.exe` [подробнее](src/Commands.cs#L40).
* Теперь при попытке очистить историю через `hclear`, если она и так пустая, то удалится файл `.history`.
* Теперь вместо команды `mk` будет: `make`.
* Теперь при исключении отображается только сообщение.
* Добавлена команда `debug-lasterr` которая выводит информацию об последнем исключении.
* Теперь если команда из модуля имеет имя существующей команды, то добавляется уточнение `<имя-модуля>.<имя-команды>` [подробнее](src/ConsoleHost.cs#L368).
* Теперь команду из модуля можно скрыть через атрибут [`HiddenCommandAttribute`](src/Attributes/HiddenCommandAtrribute.cs).
* Изменён логотип TermiSharp который выводится через команду `ver`.
* Теперь вместо символа ">" будет "#" [подробнее](src/ConsoleHost.cs#L129).
* Теперь в команде `ver` более подробное меню справа.
* Теперь начальное меню (команда `ver`) можно отключить так:<br>
`config set ShowVersionOnStart false`
* Теперь можно упростить начальное меню так:<br>
`config set SimplifiedVersionWindow false`
* Теперь можно скрыть начальное меню аргументом: `--hideversion`.
* Добавлена поддержка шрифта [Nerd Fonts](https://www.nerdfonts.com/#home). Чтобы включить поддержку пропишите:<br>
`config set NerdFontsSupport true`
## 1.0.1
* Исправлена работа `bool`[`ExeExists`](https://github.com/NonExistPlayer/TermiSharp/blob/v1.0.1/src/Commands.cs#L20)`(string)`.
* Добавлен ключ "AutoModulesInit" в `config.json`. [Подробнее](https://github.com/NonExistPlayer/TermiSharp/blob/v1.0.1/src/Config.cs#L20).
* Исправлен баг в [коде](https://github.com/NonExistPlayer/TermiSharp/blob/v1.0.1/src/ConsoleHost.cs#L356), теперь аттрибуты для методов в модулях работают коректно.
* Добавлена команда [`config`](src/Commands.cs#L106).
* Исправлена ошибка когда при вводе команды, а затем её под-команды, то любой текст после будет жёлтым. [Подробнее](src/ReadLine/HighlightHandler.cs#L20).
* Теперь история написаний в терминал команд хранится в `.history`.
* Добавлена команда [`hclear`](src/Commands.cs#L231), которая очищает историю.
* Добавлен ключ "DisableHistoryFile" в `config.json`. [Подробнее](src/Config.cs#L17).

# TermiSharp Changelog
## 1.0.1
* Исправлена работа `bool`[`ExeExists`](src/Commands.cs#L20)`(string)`.
* Добавлен ключ "AutoModulesInit" в `config.json`. [Подробнее](src/Config.cs#L16).
* Исправлен баг в [коде](src/ConsoleHost.cs#L351), теперь аттрибуты для методов в модулях работают коректно.
* Добавлена команда [`config`](src/Commands.cs#L106).
* Исправлена ошибка когда при вводе команды, а затем её под-команды, то любой текст после будет жёлтым. [Подробнее](src/ReadLine/HighlightHandler.cs#L20).
* Теперь история написаний в терминал команд хранится в `.history`.
* Добавлена команда [`hclear`](src/Commands.cs#L231), которая очищает историю.
* Добавлен ключ "DisableHistoryFile" в `config.json`. [Подробнее](src/Config.cs#L17).
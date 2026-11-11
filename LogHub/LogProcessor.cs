using System;
using System.Collections.Generic;
using LogHub.Interfaces;

namespace LogHub;

public class LogProcessor
{
    private readonly ILogParser _parser;
    private readonly IEnumerable<ILogSink> _sinks;

    // Конструктор принимает любые классы, реализующие наши интерфейсы.
    // Теперь добавить новый приёмник можно без изменения этого файла (OCP).
    public LogProcessor(ILogParser parser, IEnumerable<ILogSink> sinks)
    {
        _parser = parser;
        _sinks = sinks;
    }

    // Метод принимает готовые строки, чтобы не зависеть от файловой системы.
    public void ProcessLogs(IEnumerable<string> lines, string minSeverity)
    {
        int minLevel = SeverityToInt(minSeverity);
        int written = 0;
        int skipped = 0;

        foreach (var line in lines)
        {
            var record = _parser.Parse(line);
            
            if (record == null)
            {
                skipped++;
                continue;
            }

            if (SeverityToInt(record.Severity) < minLevel)
            {
                skipped++;
                continue;
            }

            // Отправляем лог во все подключенные приёмники (консоль, SQLite и т.д.)
            foreach (var sink in _sinks)
            {
                sink.Emit(record);
            }
            written++;
        }

        Console.WriteLine($"\n[Система] Обработка завершена. Записано: {written}, пропущено: {skipped}");
    }

    private static int SeverityToInt(string severity)
    {
        return severity switch
        {
            "Debug" => 0,
            "Info" => 1,
            "Warning" => 2,
            "Error" => 3,
            "Critical" => 4,
            _ => 1
        };
    }
}
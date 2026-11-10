using System;
using LogHub.Interfaces;
using LogHub.Models;

namespace LogHub.Parsers;

public class JournaldParser : ILogParser
{
    public LogRecord? Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line)) return null;

        // Пример строки Journald: 
        // __REALTIME_TIMESTAMP=1609459200000000 PRIORITY=3 _SYSTEMD_UNIT=nginx.service MESSAGE=Сбой в работе
        
        var record = new LogRecord { Date = DateTime.Now }; // В реальной жизни тут парсится TIMESTAMP

        // 1. Разбор приоритета (требование варианта)
        if (line.Contains("PRIORITY=3")) record.Severity = "Error";
        else if (line.Contains("PRIORITY=4")) record.Severity = "Warning";
        else if (line.Contains("PRIORITY=6")) record.Severity = "Info";
        else record.Severity = "Debug";

        // 2. Разбор unit (требование варианта)
        string unitName = "Система";
        var unitIndex = line.IndexOf("_SYSTEMD_UNIT=");
        if (unitIndex != -1)
        {
            var start = unitIndex + 14;
            var end = line.IndexOf(' ', start);
            unitName = end == -1 ? line.Substring(start) : line.Substring(start, end - start);
        }

        // 3. Извлечение сообщения
        var msgIndex = line.IndexOf("MESSAGE=");
        if (msgIndex != -1)
        {
            // Добавляем unit прямо в сообщение для наглядности
            record.Message = $"[{unitName}] {line.Substring(msgIndex + 8)}";
        }
        else
        {
            record.Message = line;
        }

        return record;
    }
}
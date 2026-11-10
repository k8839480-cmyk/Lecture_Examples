using System;
using LogHub.Interfaces;
using LogHub.Models;

namespace LogHub.Sinks;

public class ConsoleSink : ILogSink
{
    public void Emit(LogRecord record)
    {
        var previousColor = Console.ForegroundColor;
        
        // Красим критические ошибки в красный, остальное в серый
        if (record.Severity == "Error" || record.Severity == "Critical")
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Gray;
        }

        // Выводим строку на экран
        Console.WriteLine($"{record.Date:yyyy-MM-dd HH:mm:ss} [{record.Severity}] {record.Message}");
        
        // Возвращаем старый цвет
        Console.ForegroundColor = previousColor;
    }
}
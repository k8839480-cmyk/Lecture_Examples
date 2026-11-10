using System.Collections.Generic;
using Xunit;
using LogHub.Interfaces;
using LogHub.Parsers;
using LogHub.Sinks;

namespace LogHub.Tests;

public class LogProcessorTests
{
    [Fact]
    public void ProcessLogs_WithMultipleSinks_ShouldWork()
    {
        var logLines = new List<string>
        {
            "__REALTIME_TIMESTAMP=1609459200000000 PRIORITY=6 _SYSTEMD_UNIT=systemd MESSAGE=Система запущена",
            "__REALTIME_TIMESTAMP=1609459201000000 PRIORITY=3 _SYSTEMD_UNIT=nginx.service MESSAGE=Сбой подключения к порту 80",
            "какая-то сломанная строка"
        };

        ILogParser parser = new JournaldParser();
        
        // Торжество SOLID! Добавляем БД просто как новый элемент списка.
        var sinks = new List<ILogSink> 
        { 
            new ConsoleSink(),
            new SqliteSink("test_logs.db") // Файл БД создастся автоматически
        }; 
        
        var processor = new LogProcessor(parser, sinks);

        var exception = Record.Exception(() => processor.ProcessLogs(logLines, "Info"));

        Assert.Null(exception);
    }
}
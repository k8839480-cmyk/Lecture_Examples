using System.Collections.Generic;
using Xunit;
using LogHub.Interfaces;
using LogHub.Parsers;
using LogHub.Models;

namespace LogHub.Tests;

// Специальная заглушка для тестов, которая не трогает жесткий диск и БД
public class FakeSink : ILogSink
{
    public List<LogRecord> EmittedRecords { get; } = new();
    public void Emit(LogRecord record) => EmittedRecords.Add(record);
}

public class LogProcessorTests
{
    [Fact]
    public void ProcessLogs_ValidLines_EmitsToAllSinks()
    {
        // 1. Проверка маршрутизации в несколько приёмников
        var logLines = new List<string> { "PRIORITY=6 _SYSTEMD_UNIT=systemd MESSAGE=Start" };
        ILogParser parser = new JournaldParser();
        var fakeSink1 = new FakeSink();
        var fakeSink2 = new FakeSink();
        var processor = new LogProcessor(parser, new List<ILogSink> { fakeSink1, fakeSink2 });

        processor.ProcessLogs(logLines, "Info");

        Assert.Single(fakeSink1.EmittedRecords);
        Assert.Single(fakeSink2.EmittedRecords);
    }

    [Fact]
    public void Parse_ValidJournaldLine_ExtractsCorrectSeverityAndUnit()
    {
        // 2. Проверка правильного извлечения приоритета и юнита
        var parser = new JournaldParser();
        var line = "PRIORITY=3 _SYSTEMD_UNIT=nginx.service MESSAGE=Сбой";
        var record = parser.Parse(line);
        
        Assert.NotNull(record);
        Assert.Equal("Error", record.Severity);
        Assert.Contains("[nginx.service]", record.Message);
    }

    [Fact]
    public void Parse_EmptyLine_ReturnsNull()
    {
        // 3. Проверка обработки пустых строк
        var parser = new JournaldParser();
        Assert.Null(parser.Parse("   "));
    }

    [Fact]
    public void Parse_LineWithoutPriority_DefaultsToDebug()
    {
        // 4. Проверка дефолтного приоритета
        var parser = new JournaldParser();
        var record = parser.Parse("MESSAGE=Some text");
        Assert.Equal("Debug", record.Severity);
    }

    [Fact]
    public void Parse_LineWithoutUnit_DefaultsToSystem()
    {
        // 5. Проверка дефолтного юнита
        var parser = new JournaldParser();
        var record = parser.Parse("PRIORITY=6 MESSAGE=Text");
        Assert.Contains("[Система]", record.Message);
    }

    [Fact]
    public void ProcessLogs_InvalidLines_SkipsThem()
    {
        // 6. Проверка пропуска невалидных строк процессором
        var logLines = new List<string> { "", "   " };
        ILogParser parser = new JournaldParser();
        var fakeSink = new FakeSink();
        var processor = new LogProcessor(parser, new List<ILogSink> { fakeSink });

        processor.ProcessLogs(logLines, "Info");

        Assert.Empty(fakeSink.EmittedRecords);
    }
}
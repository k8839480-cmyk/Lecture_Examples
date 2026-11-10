using LogHub.Models;

namespace LogHub.Interfaces;

public interface ILogSink
{
    void Emit(LogRecord record);
}
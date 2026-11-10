using LogHub.Models;

namespace LogHub.Interfaces;

public interface ILogParser
{
    LogRecord? Parse(string line);
}
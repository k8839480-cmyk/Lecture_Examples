using Microsoft.Data.Sqlite;
using LogHub.Interfaces;
using LogHub.Models;

namespace LogHub.Sinks;

public class SqliteSink : ILogSink
{
    private readonly string _connectionString;

    public SqliteSink(string dbPath = "logs.db")
    {
        _connectionString = $"Data Source={dbPath}";
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // При первом запуске создаем таблицу в файле, если её ещё нет
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            CREATE TABLE IF NOT EXISTS Logs (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Date TEXT,
                Severity TEXT,
                Message TEXT
            )";
        command.ExecuteNonQuery();
    }

    public void Emit(LogRecord record)
    {
        // Записываем лог в базу
        using var connection = new SqliteConnection(_connectionString);
        connection.Open();
        var command = connection.CreateCommand();
        command.CommandText = @"
            INSERT INTO Logs (Date, Severity, Message) 
            VALUES ($date, $severity, $message)";
        
        command.Parameters.AddWithValue("$date", record.Date.ToString("O"));
        command.Parameters.AddWithValue("$severity", record.Severity);
        command.Parameters.AddWithValue("$message", record.Message);
        
        command.ExecuteNonQuery();
    }
}
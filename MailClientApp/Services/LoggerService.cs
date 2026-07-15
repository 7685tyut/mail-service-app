using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailClientApp.Models;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace MailClientApp.Services
{
    public interface ILoggerService
    {
        void Log(LogEntry entry);
        void LogInfo(string message, string details = "", string accountId = "", string emailId = "");
        void LogError(string message, string details = "", string accountId = "", string emailId = "");
        void LogWarning(string message, string details = "", string accountId = "", string emailId = "");
        void LogDebug(string message, string details = "", string accountId = "", string emailId = "");
        List<LogEntry> GetLogs(LogType? type = null, LogLevel? level = null, string accountId = "", DateTime? fromDate = null, DateTime? toDate = null);
        List<LogEntry> GetRecentLogs(int count = 100);
        void ClearLogs();
        void Initialize(string logDirectory);
    }

    public class LoggerService : ILoggerService
    {
        private readonly List<LogEntry> _logs = new List<LogEntry>();
        private readonly object _lock = new object();
        private readonly string _logDirectory;
        private Logger? _nlogLogger;

        public LoggerService(string logDirectory)
        {
            _logDirectory = logDirectory;
            Initialize(logDirectory);
        }

        public void Initialize(string logDirectory)
        {
            try
            {
                // Ensure directory exists
                Directory.CreateDirectory(logDirectory);

                // Configure NLog
                var config = new LoggingConfiguration();
                
                // File target
                var fileTarget = new FileTarget
                {
                    Name = "file",
                    FileName = Path.Combine(logDirectory, "mail-client-${shortdate}.log"),
                    Layout = "${longdate} | ${level:uppercase=true} | ${message} | ${exception:format=tostring}",
                    MaxArchiveFiles = 7,
                    ArchiveEvery = FileArchivePeriod.Day,
                    ArchiveFileName = Path.Combine(logDirectory, "mail-client-{#}.log")
                };
                config.AddTarget(fileTarget);
                config.AddRule(LogLevel.Debug, LogLevel.Fatal, fileTarget);

                // Console target
                var consoleTarget = new ConsoleTarget
                {
                    Name = "console",
                    Layout = "${longdate} | ${level:uppercase=true} | ${message}"
                };
                config.AddTarget(consoleTarget);
                config.AddRule(LogLevel.Info, LogLevel.Fatal, consoleTarget);

                LogManager.Configuration = config;
                _nlogLogger = LogManager.GetLogger("MailClientApp");
            }
            catch (Exception ex)
            {
                // Fallback to simple logging
                File.AppendAllText(Path.Combine(logDirectory, "error.log"), $"NLog initialization failed: {ex}\n");
            }
        }

        public void Log(LogEntry entry)
        {
            lock (_lock)
            {
                _logs.Add(entry);
                
                // Keep only last 10000 logs in memory
                if (_logs.Count > 10000)
                {
                    _logs.RemoveRange(0, _logs.Count - 10000);
                }
            }

            try
            {
                _nlogLogger?.Log(GetNLogLevel(entry.Level), entry, entry.Message);
            }
            catch
            {
                // Ignore NLog errors
            }
        }

        public void LogInfo(string message, string details = "", string accountId = "", string emailId = "")
        {
            var entry = new LogEntry
            {
                Level = LogLevel.Info,
                Type = LogType.System,
                Message = message,
                Details = details,
                AccountId = accountId,
                EmailId = emailId
            };
            Log(entry);
        }

        public void LogError(string message, string details = "", string accountId = "", string emailId = "")
        {
            var entry = new LogEntry
            {
                Level = LogLevel.Error,
                Type = LogType.Error,
                Message = message,
                Details = details,
                AccountId = accountId,
                EmailId = emailId
            };
            Log(entry);
        }

        public void LogWarning(string message, string details = "", string accountId = "", string emailId = "")
        {
            var entry = new LogEntry
            {
                Level = LogLevel.Warning,
                Type = LogType.System,
                Message = message,
                Details = details,
                AccountId = accountId,
                EmailId = emailId
            };
            Log(entry);
        }

        public void LogDebug(string message, string details = "", string accountId = "", string emailId = "")
        {
            var entry = new LogEntry
            {
                Level = LogLevel.Debug,
                Type = LogType.System,
                Message = message,
                Details = details,
                AccountId = accountId,
                EmailId = emailId
            };
            Log(entry);
        }

        public List<LogEntry> GetLogs(LogType? type = null, LogLevel? level = null, string accountId = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            lock (_lock)
            {
                var query = _logs.AsQueryable();

                if (type.HasValue)
                    query = query.Where(l => l.Type == type.Value);

                if (level.HasValue)
                    query = query.Where(l => l.Level == level.Value);

                if (!string.IsNullOrEmpty(accountId))
                    query = query.Where(l => l.AccountId == accountId);

                if (fromDate.HasValue)
                    query = query.Where(l => l.Timestamp >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(l => l.Timestamp <= toDate.Value);

                return query.OrderByDescending(l => l.Timestamp).ToList();
            }
        }

        public List<LogEntry> GetRecentLogs(int count = 100)
        {
            lock (_lock)
            {
                return _logs.OrderByDescending(l => l.Timestamp).Take(count).ToList();
            }
        }

        public void ClearLogs()
        {
            lock (_lock)
            {
                _logs.Clear();
            }
        }

        private NLog.LogLevel GetNLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Debug => NLog.LogLevel.Debug,
                LogLevel.Info => NLog.LogLevel.Info,
                LogLevel.Warning => NLog.LogLevel.Warn,
                LogLevel.Error => NLog.LogLevel.Error,
                LogLevel.Critical => NLog.LogLevel.Fatal,
                _ => NLog.LogLevel.Info
            };
        }
    }

    public class FileLoggerService : ILoggerService
    {
        private readonly string _logDirectory;
        private readonly List<LogEntry> _logs = new List<LogEntry>();
        private readonly object _lock = new object();

        public FileLoggerService(string logDirectory)
        {
            _logDirectory = logDirectory;
            Directory.CreateDirectory(logDirectory);
        }

        public void Initialize(string logDirectory)
        {
            // Already initialized in constructor
        }

        public void Log(LogEntry entry)
        {
            lock (_lock)
            {
                _logs.Add(entry);
                
                if (_logs.Count > 10000)
                {
                    _logs.RemoveRange(0, _logs.Count - 10000);
                }
            }

            try
            {
                var logLine = $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss}] [{entry.Level}] [{entry.Type}] {entry.Message}";
                if (!string.IsNullOrEmpty(entry.Details))
                    logLine += $"\n{entry.Details}";
                
                var filePath = Path.Combine(_logDirectory, "mail-client.log");
                File.AppendAllText(filePath, logLine + "\n\n");
            }
            catch
            {
                // Ignore file write errors
            }
        }

        public void LogInfo(string message, string details = "", string accountId = "", string emailId = "")
        {
            Log(new LogEntry { Level = LogLevel.Info, Type = LogType.System, Message = message, Details = details, AccountId = accountId, EmailId = emailId });
        }

        public void LogError(string message, string details = "", string accountId = "", string emailId = "")
        {
            Log(new LogEntry { Level = LogLevel.Error, Type = LogType.Error, Message = message, Details = details, AccountId = accountId, EmailId = emailId });
        }

        public void LogWarning(string message, string details = "", string accountId = "", string emailId = "")
        {
            Log(new LogEntry { Level = LogLevel.Warning, Type = LogType.System, Message = message, Details = details, AccountId = accountId, EmailId = emailId });
        }

        public void LogDebug(string message, string details = "", string accountId = "", string emailId = "")
        {
            Log(new LogEntry { Level = LogLevel.Debug, Type = LogType.System, Message = message, Details = details, AccountId = accountId, EmailId = emailId });
        }

        public List<LogEntry> GetLogs(LogType? type = null, LogLevel? level = null, string accountId = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            lock (_lock)
            {
                var query = _logs.AsQueryable();

                if (type.HasValue)
                    query = query.Where(l => l.Type == type.Value);

                if (level.HasValue)
                    query = query.Where(l => l.Level == level.Value);

                if (!string.IsNullOrEmpty(accountId))
                    query = query.Where(l => l.AccountId == accountId);

                if (fromDate.HasValue)
                    query = query.Where(l => l.Timestamp >= fromDate.Value);

                if (toDate.HasValue)
                    query = query.Where(l => l.Timestamp <= toDate.Value);

                return query.OrderByDescending(l => l.Timestamp).ToList();
            }
        }

        public List<LogEntry> GetRecentLogs(int count = 100)
        {
            lock (_lock)
            {
                return _logs.OrderByDescending(l => l.Timestamp).Take(count).ToList();
            }
        }

        public void ClearLogs()
        {
            lock (_lock)
            {
                _logs.Clear();
            }
        }
    }
}
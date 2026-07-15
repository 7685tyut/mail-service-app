using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MailClientApp.Models
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    public enum LogType
    {
        System,
        EmailSend,
        EmailReceive,
        Account,
        Sync,
        Error
    }

    public class LogEntry : INotifyPropertyChanged
    {
        private string _id;
        private DateTime _timestamp;
        private LogLevel _level;
        private LogType _type;
        private string _message;
        private string _details;
        private string _accountId;
        private string _emailId;
        private string _source;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set { _timestamp = value; OnPropertyChanged(); }
        }

        public LogLevel Level
        {
            get => _level;
            set { _level = value; OnPropertyChanged(); }
        }

        public LogType Type
        {
            get => _type;
            set { _type = value; OnPropertyChanged(); }
        }

        public string Message
        {
            get => _message;
            set { _message = value; OnPropertyChanged(); }
        }

        public string Details
        {
            get => _details;
            set { _details = value; OnPropertyChanged(); }
        }

        public string AccountId
        {
            get => _accountId;
            set { _accountId = value; OnPropertyChanged(); }
        }

        public string EmailId
        {
            get => _emailId;
            set { _emailId = value; OnPropertyChanged(); }
        }

        public string Source
        {
            get => _source;
            set { _source = value; OnPropertyChanged(); }
        }

        public LogEntry()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
            Source = "MailClientApp";
        }

        public static LogEntry CreateSendLog(string accountId, string emailId, string message, string details = "", LogLevel level = LogLevel.Info)
        {
            return new LogEntry
            {
                Type = LogType.EmailSend,
                AccountId = accountId,
                EmailId = emailId,
                Message = message,
                Details = details,
                Level = level
            };
        }

        public static LogEntry CreateReceiveLog(string accountId, string emailId, string message, string details = "", LogLevel level = LogLevel.Info)
        {
            return new LogEntry
            {
                Type = LogType.EmailReceive,
                AccountId = accountId,
                EmailId = emailId,
                Message = message,
                Details = details,
                Level = level
            };
        }

        public static LogEntry CreateErrorLog(string source, string message, string details = "", string accountId = "", string emailId = "")
        {
            return new LogEntry
            {
                Type = LogType.Error,
                Level = LogLevel.Error,
                Source = source,
                AccountId = accountId,
                EmailId = emailId,
                Message = message,
                Details = details
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MailClientApp.Models
{
    public class EmailAccount : INotifyPropertyChanged
    {
        private string _id;
        private string _email;
        private string _name;
        private string _smtpServer;
        private int _smtpPort;
        private bool _smtpUseSsl;
        private string _smtpUsername;
        private string _smtpPassword;
        private string _imapServer;
        private int _imapPort;
        private bool _imapUseSsl;
        private string _imapUsername;
        private string _imapPassword;
        private bool _isDefault;
        private DateTime _createdDate;
        private DateTime _lastSyncDate;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }

        public string Name
        {
            get => _name;
            set { _name = value; OnPropertyChanged(); }
        }

        public string SmtpServer
        {
            get => _smtpServer;
            set { _smtpServer = value; OnPropertyChanged(); }
        }

        public int SmtpPort
        {
            get => _smtpPort;
            set { _smtpPort = value; OnPropertyChanged(); }
        }

        public bool SmtpUseSsl
        {
            get => _smtpUseSsl;
            set { _smtpUseSsl = value; OnPropertyChanged(); }
        }

        public string SmtpUsername
        {
            get => _smtpUsername;
            set { _smtpUsername = value; OnPropertyChanged(); }
        }

        public string SmtpPassword
        {
            get => _smtpPassword;
            set { _smtpPassword = value; OnPropertyChanged(); }
        }

        public string ImapServer
        {
            get => _imapServer;
            set { _imapServer = value; OnPropertyChanged(); }
        }

        public int ImapPort
        {
            get => _imapPort;
            set { _imapPort = value; OnPropertyChanged(); }
        }

        public bool ImapUseSsl
        {
            get => _imapUseSsl;
            set { _imapUseSsl = value; OnPropertyChanged(); }
        }

        public string ImapUsername
        {
            get => _imapUsername;
            set { _imapUsername = value; OnPropertyChanged(); }
        }

        public string ImapPassword
        {
            get => _imapPassword;
            set { _imapPassword = value; OnPropertyChanged(); }
        }

        public bool IsDefault
        {
            get => _isDefault;
            set { _isDefault = value; OnPropertyChanged(); }
        }

        public DateTime CreatedDate
        {
            get => _createdDate;
            set { _createdDate = value; OnPropertyChanged(); }
        }

        public DateTime LastSyncDate
        {
            get => _lastSyncDate;
            set { _lastSyncDate = value; OnPropertyChanged(); }
        }

        public EmailAccount()
        {
            Id = Guid.NewGuid().ToString();
            CreatedDate = DateTime.Now;
            SmtpPort = 587;
            ImapPort = 993;
            SmtpUseSsl = true;
            ImapUseSsl = true;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MailClientApp.Models;
using MailClientApp.Services;

namespace MailClientApp.ViewModels
{
    public class AccountDialogViewModel : INotifyPropertyChanged
    {
        private EmailAccount _account;
        private bool _isEditMode;
        private List<string> _commonProviders = new List<string> { "Gmail", "Yandex", "Mail.ru", "Outlook", "Custom" };
        private string _selectedProvider = "Custom";

        public EmailAccount Account
        {
            get => _account;
            set { _account = value; OnPropertyChanged(); }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set { _isEditMode = value; OnPropertyChanged(); }
        }

        public List<string> CommonProviders
        {
            get => _commonProviders;
            set { _commonProviders = value; OnPropertyChanged(); }
        }

        public string SelectedProvider
        {
            get => _selectedProvider;
            set
            {
                _selectedProvider = value;
                OnPropertyChanged();
                ApplyProviderSettings();
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public AccountDialogViewModel()
        {
            Account = new EmailAccount
            {
                Name = "Новый аккаунт",
                SmtpPort = 587,
                ImapPort = 993,
                SmtpUseSsl = true,
                ImapUseSsl = true
            };
            
            SaveCommand = new RelayCommand(_ => DialogResult = true);
            CancelCommand = new RelayCommand(_ => DialogResult = false);
        }

        public AccountDialogViewModel(EmailAccount account) : this()
        {
            Account = account.Clone();
            IsEditMode = true;
            DetectProvider();
        }

        private void DetectProvider()
        {
            var email = Account.Email.ToLower();
            
            if (email.Contains("gmail.com") || email.Contains("googlemail.com"))
                SelectedProvider = "Gmail";
            else if (email.Contains("yandex."))
                SelectedProvider = "Yandex";
            else if (email.Contains("mail.ru") || email.Contains("inbox.ru") || email.Contains("list.ru") || email.Contains("bk.ru"))
                SelectedProvider = "Mail.ru";
            else if (email.Contains("outlook.com") || email.Contains("hotmail.com") || email.Contains("live.com"))
                SelectedProvider = "Outlook";
            else
                SelectedProvider = "Custom";
        }

        private void ApplyProviderSettings()
        {
            switch (SelectedProvider)
            {
                case "Gmail":
                    Account.SmtpServer = "smtp.gmail.com";
                    Account.SmtpPort = 587;
                    Account.SmtpUseSsl = true;
                    Account.ImapServer = "imap.gmail.com";
                    Account.ImapPort = 993;
                    Account.ImapUseSsl = true;
                    break;
                case "Yandex":
                    Account.SmtpServer = "smtp.yandex.ru";
                    Account.SmtpPort = 587;
                    Account.SmtpUseSsl = true;
                    Account.ImapServer = "imap.yandex.ru";
                    Account.ImapPort = 993;
                    Account.ImapUseSsl = true;
                    break;
                case "Mail.ru":
                    Account.SmtpServer = "smtp.mail.ru";
                    Account.SmtpPort = 587;
                    Account.SmtpUseSsl = true;
                    Account.ImapServer = "imap.mail.ru";
                    Account.ImapPort = 993;
                    Account.ImapUseSsl = true;
                    break;
                case "Outlook":
                    Account.SmtpServer = "smtp-mail.outlook.com";
                    Account.SmtpPort = 587;
                    Account.SmtpUseSsl = true;
                    Account.ImapServer = "outlook.office365.com";
                    Account.ImapPort = 993;
                    Account.ImapUseSsl = true;
                    break;
            }
            
            OnPropertyChanged(nameof(Account));
        }

        public bool? DialogResult { get; set; }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public static class EmailAccountExtensions
    {
        public static EmailAccount Clone(this EmailAccount account)
        {
            return new EmailAccount
            {
                Id = account.Id,
                Email = account.Email,
                Name = account.Name,
                SmtpServer = account.SmtpServer,
                SmtpPort = account.SmtpPort,
                SmtpUseSsl = account.SmtpUseSsl,
                SmtpUsername = account.SmtpUsername,
                SmtpPassword = account.SmtpPassword,
                ImapServer = account.ImapServer,
                ImapPort = account.ImapPort,
                ImapUseSsl = account.ImapUseSsl,
                ImapUsername = account.ImapUsername,
                ImapPassword = account.ImapPassword,
                IsDefault = account.IsDefault,
                CreatedDate = account.CreatedDate,
                LastSyncDate = account.LastSyncDate
            };
        }
    }
}
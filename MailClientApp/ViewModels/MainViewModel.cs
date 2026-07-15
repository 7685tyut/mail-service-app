using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using MailClientApp.Models;
using MailClientApp.Services;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;

namespace MailClientApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IEmailService _emailService;
        private readonly IAccountService _accountService;
        private readonly ILoggerService _logger;
        private readonly string _dataDirectory;
        private readonly string _logDirectory;

        private ObservableCollection<EmailAccount> _accounts = new ObservableCollection<EmailAccount>();
        private ObservableCollection<EmailMessage> _emails = new ObservableCollection<EmailMessage>();
        private ObservableCollection<LogEntry> _logs = new ObservableCollection<LogEntry>();
        private EmailAccount? _selectedAccount;
        private EmailMessage? _selectedEmail;
        private LogEntry? _selectedLog;
        private string _searchText = string.Empty;
        private string _newEmailTo = string.Empty;
        private string _newEmailSubject = string.Empty;
        private string _newEmailBody = string.Empty;
        private bool _isBusy;
        private string _statusMessage = "Готово";
        private int _totalEmails;
        private int _unreadEmails;
        private string _selectedFolder = "INBOX";
        private List<string> _folders = new List<string>();
        private Visibility _emailDetailVisibility = Visibility.Collapsed;
        private string _htmlBody = string.Empty;

        public ObservableCollection<EmailAccount> Accounts
        {
            get => _accounts;
            set { _accounts = value; OnPropertyChanged(); }
        }

        public ObservableCollection<EmailMessage> Emails
        {
            get => _emails;
            set { _emails = value; OnPropertyChanged(); }
        }

        public ObservableCollection<LogEntry> Logs
        {
            get => _logs;
            set { _logs = value; OnPropertyChanged(); }
        }

        public EmailAccount? SelectedAccount
        {
            get => _selectedAccount;
            set
            {
                _selectedAccount = value;
                OnPropertyChanged();
                if (value != null)
                    LoadEmailsForAccount(value);
            }
        }

        public EmailMessage? SelectedEmail
        {
            get => _selectedEmail;
            set
            {
                _selectedEmail = value;
                OnPropertyChanged();
                if (value != null)
                {
                    EmailDetailVisibility = Visibility.Visible;
                    HtmlBody = value.HtmlBody ?? value.Body;
                    // Mark as read
                    value.IsRead = true;
                }
                else
                {
                    EmailDetailVisibility = Visibility.Collapsed;
                }
            }
        }

        public LogEntry? SelectedLog
        {
            get => _selectedLog;
            set { _selectedLog = value; OnPropertyChanged(); }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                FilterEmails();
            }
        }

        public string NewEmailTo
        {
            get => _newEmailTo;
            set { _newEmailTo = value; OnPropertyChanged(); }
        }

        public string NewEmailSubject
        {
            get => _newEmailSubject;
            set { _newEmailSubject = value; OnPropertyChanged(); }
        }

        public string NewEmailBody
        {
            get => _newEmailBody;
            set { _newEmailBody = value; OnPropertyChanged(); }
        }

        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public int TotalEmails
        {
            get => _totalEmails;
            set { _totalEmails = value; OnPropertyChanged(); }
        }

        public int UnreadEmails
        {
            get => _unreadEmails;
            set { _unreadEmails = value; OnPropertyChanged(); }
        }

        public string SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                _selectedFolder = value;
                OnPropertyChanged();
                if (SelectedAccount != null)
                    LoadEmailsForAccount(SelectedAccount, value);
            }
        }

        public List<string> Folders
        {
            get => _folders;
            set { _folders = value; OnPropertyChanged(); }
        }

        public Visibility EmailDetailVisibility
        {
            get => _emailDetailVisibility;
            set { _emailDetailVisibility = value; OnPropertyChanged(); }
        }

        public string HtmlBody
        {
            get => _htmlBody;
            set { _htmlBody = value; OnPropertyChanged(); }
        }

        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand SetDefaultAccountCommand { get; }
        public ICommand SendEmailCommand { get; }
        public ICommand ReceiveEmailsCommand { get; }
        public ICommand SyncAccountCommand { get; }
        public ICommand DeleteEmailCommand { get; }
        public ICommand ReplyEmailCommand { get; }
        public ICommand ForwardEmailCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand ShowLogsCommand { get; }
        public ICommand TestConnectionCommand { get; }

        public MainViewModel()
        {
            _dataDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MailClientApp");
            _logDirectory = Path.Combine(_dataDirectory, "Logs");
            
            Directory.CreateDirectory(_dataDirectory);
            Directory.CreateDirectory(_logDirectory);

            _logger = new FileLoggerService(_logDirectory);
            _accountService = new AccountService(_dataDirectory, _logger);
            _emailService = new EmailService(_logger);

            InitializeCommands();
            LoadData();
        }

        private void InitializeCommands()
        {
            AddAccountCommand = new RelayCommand(async _ => await AddAccountAsync());
            EditAccountCommand = new RelayCommand(async _ => await EditAccountAsync(), _ => SelectedAccount != null);
            DeleteAccountCommand = new RelayCommand(async _ => await DeleteAccountAsync(), _ => SelectedAccount != null);
            SetDefaultAccountCommand = new RelayCommand(async _ => await SetDefaultAccountAsync(), _ => SelectedAccount != null);
            SendEmailCommand = new RelayCommand(async _ => await SendEmailAsync(), CanSendEmail);
            ReceiveEmailsCommand = new RelayCommand(async _ => await ReceiveEmailsAsync(), _ => SelectedAccount != null);
            SyncAccountCommand = new RelayCommand(async _ => await SyncAccountAsync(), _ => SelectedAccount != null);
            DeleteEmailCommand = new RelayCommand(async _ => await DeleteEmailAsync(), _ => SelectedEmail != null);
            ReplyEmailCommand = new RelayCommand(async _ => await ReplyEmailAsync(), _ => SelectedEmail != null);
            ForwardEmailCommand = new RelayCommand(async _ => await ForwardEmailAsync(), _ => SelectedEmail != null);
            AddAttachmentCommand = new RelayCommand(async _ => await AddAttachmentAsync());
            ClearSearchCommand = new RelayCommand(_ => ClearSearch());
            ShowLogsCommand = new RelayCommand(_ => ShowLogs());
            TestConnectionCommand = new RelayCommand(async _ => await TestConnectionAsync(), _ => SelectedAccount != null);
        }

        private async void LoadData()
        {
            IsBusy = true;
            StatusMessage = "Загрузка аккаунтов...";

            try
            {
                await _accountService.LoadAccountsAsync();
                var accounts = await _accountService.GetAccountsAsync();
                
                foreach (var account in accounts)
                {
                    Accounts.Add(account);
                }

                if (Accounts.Any())
                {
                    SelectedAccount = Accounts.FirstOrDefault(a => a.IsDefault) ?? Accounts.First();
                }

                StatusMessage = "Готово";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки";
                _logger.LogError("Ошибка загрузки данных", ex.ToString());
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async void LoadEmailsForAccount(EmailAccount account, string folder = "INBOX")
        {
            IsBusy = true;
            StatusMessage = "Загрузка писем...";

            try
            {
                Emails.Clear();
                
                // Load folders
                var folders = await _emailService.GetFoldersAsync(account);
                Folders = folders;
                
                if (!Folders.Contains(folder))
                    folder = "INBOX";
                
                SelectedFolder = folder;

                var result = await _emailService.ReceiveEmailsAsync(account, folder, 100);
                
                foreach (var email in result.Emails)
                {
                    Emails.Add(email);
                }

                UpdateEmailStats();
                StatusMessage = $"Загружено {Emails.Count} писем";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка загрузки писем";
                _logger.LogError("Ошибка загрузки писем", ex.ToString(), account.Id);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void UpdateEmailStats()
        {
            TotalEmails = Emails.Count;
            UnreadEmails = Emails.Count(e => !e.IsRead);
        }

        private void FilterEmails()
        {
            // Simple filtering - in a real app, you'd want to implement this more efficiently
            // For now, we'll just show all emails
        }

        private void ClearSearch()
        {
            SearchText = string.Empty;
        }

        private void ShowLogs()
        {
            var recentLogs = _logger.GetRecentLogs(100);
            Logs.Clear();
            foreach (var log in recentLogs)
            {
                Logs.Add(log);
            }
        }

        private bool CanSendEmail(object parameter)
        {
            return !string.IsNullOrEmpty(NewEmailTo) && SelectedAccount != null;
        }

        private async Task AddAccountAsync()
        {
            var dialog = new AccountDialogViewModel();
            if (DialogHost.Show(dialog, "AddAccountDialog") is true)
            {
                var account = dialog.Account;
                await _accountService.AddAccountAsync(account);
                Accounts.Add(account);
                SelectedAccount = account;
            }
        }

        private async Task EditAccountAsync()
        {
            if (SelectedAccount == null) return;

            var dialog = new AccountDialogViewModel(SelectedAccount);
            if (DialogHost.Show(dialog, "EditAccountDialog") is true)
            {
                await _accountService.UpdateAccountAsync(dialog.Account);
                
                // Update in collection
                var index = Accounts.IndexOf(SelectedAccount);
                if (index >= 0)
                {
                    Accounts[index] = dialog.Account;
                    SelectedAccount = dialog.Account;
                }
            }
        }

        private async Task DeleteAccountAsync()
        {
            if (SelectedAccount == null) return;

            var result = MessageBox.Show(
                $"Удалить аккаунт {SelectedAccount.Email}?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                await _accountService.DeleteAccountAsync(SelectedAccount.Id);
                Accounts.Remove(SelectedAccount);
                
                if (Accounts.Any())
                    SelectedAccount = Accounts.First();
                else
                    SelectedAccount = null;
            }
        }

        private async Task SetDefaultAccountAsync()
        {
            if (SelectedAccount == null) return;

            await _accountService.SetDefaultAccountAsync(SelectedAccount.Id);
            
            foreach (var account in Accounts)
            {
                account.IsDefault = account.Id == SelectedAccount.Id;
            }
        }

        private async Task SendEmailAsync()
        {
            if (SelectedAccount == null) return;

            IsBusy = true;
            StatusMessage = "Отправка письма...";

            try
            {
                var email = new EmailMessage
                {
                    To = NewEmailTo,
                    Subject = NewEmailSubject,
                    Body = NewEmailBody,
                    From = SelectedAccount.Email,
                    AccountId = SelectedAccount.Id
                };

                var result = await _emailService.SendEmailAsync(SelectedAccount, email);

                if (result.Success)
                {
                    StatusMessage = "Письмо отправлено";
                    
                    // Clear form
                    NewEmailTo = string.Empty;
                    NewEmailSubject = string.Empty;
                    NewEmailBody = string.Empty;

                    // Refresh emails
                    LoadEmailsForAccount(SelectedAccount);
                }
                else
                {
                    StatusMessage = result.Message;
                    MessageBox.Show(result.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка отправки";
                _logger.LogError("Ошибка отправки письма", ex.ToString(), SelectedAccount.Id);
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ReceiveEmailsAsync()
        {
            if (SelectedAccount == null) return;

            LoadEmailsForAccount(SelectedAccount, SelectedFolder);
        }

        private async Task SyncAccountAsync()
        {
            if (SelectedAccount == null) return;

            IsBusy = true;
            StatusMessage = "Синхронизация...";

            try
            {
                await _emailService.SyncAccountAsync(
                    SelectedAccount,
                    email => Application.Current.Dispatcher.Invoke(() => Emails.Insert(0, email)),
                    log => Application.Current.Dispatcher.Invoke(() => Logs.Insert(0, log)));

                UpdateEmailStats();
                StatusMessage = "Синхронизация завершена";
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка синхронизации";
                _logger.LogError("Ошибка синхронизации", ex.ToString(), SelectedAccount.Id);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task DeleteEmailAsync()
        {
            if (SelectedEmail == null || SelectedAccount == null) return;

            var result = MessageBox.Show(
                "Удалить выбранное письмо?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                IsBusy = true;
                StatusMessage = "Удаление письма...";

                try
                {
                    var deleteResult = await _emailService.DeleteEmailAsync(
                        SelectedAccount, 
                        SelectedEmail.MessageId, 
                        SelectedFolder);

                    if (deleteResult.Success)
                    {
                        Emails.Remove(SelectedEmail);
                        SelectedEmail = null;
                        UpdateEmailStats();
                        StatusMessage = "Письмо удалено";
                    }
                    else
                    {
                        StatusMessage = deleteResult.Message;
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = "Ошибка удаления";
                    _logger.LogError("Ошибка удаления письма", ex.ToString(), SelectedAccount.Id, SelectedEmail.Id);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        private async Task ReplyEmailAsync()
        {
            if (SelectedEmail == null || SelectedAccount == null) return;

            NewEmailTo = SelectedEmail.From;
            NewEmailSubject = SelectedEmail.Subject.StartsWith("Re: ") ? SelectedEmail.Subject : $"Re: {SelectedEmail.Subject}";
            NewEmailBody = $"\n\n--- Original Message ---\nFrom: {SelectedEmail.From}\nSent: {SelectedEmail.Date}\nTo: {SelectedEmail.To}\nSubject: {SelectedEmail.Subject}\n\n{SelectedEmail.Body}";
        }

        private async Task ForwardEmailAsync()
        {
            if (SelectedEmail == null) return;

            NewEmailSubject = SelectedEmail.Subject.StartsWith("Fwd: ") ? SelectedEmail.Subject : $"Fwd: {SelectedEmail.Subject}";
            NewEmailBody = $"--- Forwarded Message ---\nFrom: {SelectedEmail.From}\nSent: {SelectedEmail.Date}\nTo: {SelectedEmail.To}\nSubject: {SelectedEmail.Subject}\n\n{SelectedEmail.Body}";
        }

        private async Task AddAttachmentAsync()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Выберите файл для вложения",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (var filePath in openFileDialog.FileNames)
                {
                    // In a real implementation, you'd add the attachment to the email
                    // For now, just show a message
                    StatusMessage = $"Добавлено вложение: {Path.GetFileName(filePath)}";
                }
            }
        }

        private async Task TestConnectionAsync()
        {
            if (SelectedAccount == null) return;

            IsBusy = true;
            StatusMessage = "Проверка подключения...";

            try
            {
                var result = await _emailService.TestConnectionAsync(SelectedAccount);
                
                if (result.Success)
                {
                    StatusMessage = "Подключение успешно";
                    MessageBox.Show("Подключение к серверам IMAP и SMTP успешно проверено!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = result.Message;
                    MessageBox.Show(result.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = "Ошибка подключения";
                _logger.LogError("Ошибка проверки подключения", ex.ToString(), SelectedAccount.Id);
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object? parameter) => _execute(parameter);

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}
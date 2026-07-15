using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using MailClientApp.Models;

namespace MailClientApp.Services
{
    public interface IAccountService
    {
        Task<List<EmailAccount>> GetAccountsAsync();
        Task<EmailAccount?> GetAccountByIdAsync(string id);
        Task<EmailAccount?> GetDefaultAccountAsync();
        Task AddAccountAsync(EmailAccount account);
        Task UpdateAccountAsync(EmailAccount account);
        Task DeleteAccountAsync(string id);
        Task SetDefaultAccountAsync(string id);
        Task SaveAccountsAsync();
        Task LoadAccountsAsync();
    }

    public class AccountService : IAccountService
    {
        private readonly string _accountsFile;
        private List<EmailAccount> _accounts = new List<EmailAccount>();
        private readonly ILoggerService _logger;

        public AccountService(string dataDirectory, ILoggerService logger)
        {
            _accountsFile = Path.Combine(dataDirectory, "accounts.json");
            _logger = logger;
        }

        public async Task LoadAccountsAsync()
        {
            try
            {
                if (File.Exists(_accountsFile))
                {
                    var json = await File.ReadAllTextAsync(_accountsFile);
                    _accounts = JsonSerializer.Deserialize<List<EmailAccount>>(json) ?? new List<EmailAccount>();
                    _logger.LogInfo("Загружено аккаунтов", $"Количество: {_accounts.Count}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка загрузки аккаунтов", ex.ToString());
            }
        }

        public async Task SaveAccountsAsync()
        {
            try
            {
                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(_accounts, options);
                await File.WriteAllTextAsync(_accountsFile, json);
                _logger.LogInfo("Аккаунты сохранены", $"Количество: {_accounts.Count}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Ошибка сохранения аккаунтов", ex.ToString());
            }
        }

        public Task<List<EmailAccount>> GetAccountsAsync()
        {
            return Task.FromResult(_accounts.ToList());
        }

        public Task<EmailAccount?> GetAccountByIdAsync(string id)
        {
            var account = _accounts.FirstOrDefault(a => a.Id == id);
            return Task.FromResult(account);
        }

        public async Task<EmailAccount?> GetDefaultAccountAsync()
        {
            var defaultAccount = _accounts.FirstOrDefault(a => a.IsDefault);
            if (defaultAccount != null)
                return defaultAccount;
            
            // If no default, return first account
            if (_accounts.Any())
                return _accounts.First();
            
            return null;
        }

        public async Task AddAccountAsync(EmailAccount account)
        {
            // Set as default if it's the first account
            if (!_accounts.Any())
                account.IsDefault = true;
            
            _accounts.Add(account);
            await SaveAccountsAsync();
            _logger.LogInfo("Добавлен аккаунт", $"Email: {account.Email}, Имя: {account.Name}");
        }

        public async Task UpdateAccountAsync(EmailAccount account)
        {
            var existing = _accounts.FirstOrDefault(a => a.Id == account.Id);
            if (existing != null)
            {
                // Copy properties
                existing.Email = account.Email;
                existing.Name = account.Name;
                existing.SmtpServer = account.SmtpServer;
                existing.SmtpPort = account.SmtpPort;
                existing.SmtpUseSsl = account.SmtpUseSsl;
                existing.SmtpUsername = account.SmtpUsername;
                existing.SmtpPassword = account.SmtpPassword;
                existing.ImapServer = account.ImapServer;
                existing.ImapPort = account.ImapPort;
                existing.ImapUseSsl = account.ImapUseSsl;
                existing.ImapUsername = account.ImapUsername;
                existing.ImapPassword = account.ImapPassword;
                existing.IsDefault = account.IsDefault;
                
                await SaveAccountsAsync();
                _logger.LogInfo("Обновлен аккаунт", $"Email: {account.Email}");
            }
        }

        public async Task DeleteAccountAsync(string id)
        {
            var account = _accounts.FirstOrDefault(a => a.Id == id);
            if (account != null)
            {
                _accounts.Remove(account);
                
                // If deleted account was default, set first as default
                if (account.IsDefault && _accounts.Any())
                {
                    _accounts.First().IsDefault = true;
                }
                
                await SaveAccountsAsync();
                _logger.LogInfo("Удален аккаунт", $"Email: {account.Email}");
            }
        }

        public async Task SetDefaultAccountAsync(string id)
        {
            foreach (var account in _accounts)
            {
                account.IsDefault = account.Id == id;
            }
            
            await SaveAccountsAsync();
            _logger.LogInfo("Установлен аккаунт по умолчанию", $"ID: {id}");
        }
    }

    public static class AccountPresets
    {
        public static List<EmailAccount> GetCommonProviders()
        {
            return new List<EmailAccount>
            {
                new EmailAccount
                {
                    Name = "Gmail",
                    Email = "your.email@gmail.com",
                    SmtpServer = "smtp.gmail.com",
                    SmtpPort = 587,
                    SmtpUseSsl = true,
                    SmtpUsername = "your.email@gmail.com",
                    ImapServer = "imap.gmail.com",
                    ImapPort = 993,
                    ImapUseSsl = true,
                    ImapUsername = "your.email@gmail.com"
                },
                new EmailAccount
                {
                    Name = "Yandex",
                    Email = "your.email@yandex.ru",
                    SmtpServer = "smtp.yandex.ru",
                    SmtpPort = 587,
                    SmtpUseSsl = true,
                    SmtpUsername = "your.email@yandex.ru",
                    ImapServer = "imap.yandex.ru",
                    ImapPort = 993,
                    ImapUseSsl = true,
                    ImapUsername = "your.email@yandex.ru"
                },
                new EmailAccount
                {
                    Name = "Mail.ru",
                    Email = "your.email@mail.ru",
                    SmtpServer = "smtp.mail.ru",
                    SmtpPort = 587,
                    SmtpUseSsl = true,
                    SmtpUsername = "your.email@mail.ru",
                    ImapServer = "imap.mail.ru",
                    ImapPort = 993,
                    ImapUseSsl = true,
                    ImapUsername = "your.email@mail.ru"
                },
                new EmailAccount
                {
                    Name = "Outlook",
                    Email = "your.email@outlook.com",
                    SmtpServer = "smtp-mail.outlook.com",
                    SmtpPort = 587,
                    SmtpUseSsl = true,
                    SmtpUsername = "your.email@outlook.com",
                    ImapServer = "outlook.office365.com",
                    ImapPort = 993,
                    ImapUseSsl = true,
                    ImapUsername = "your.email@outlook.com"
                }
            };
        }
    }
}
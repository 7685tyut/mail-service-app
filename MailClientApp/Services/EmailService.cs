using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MailClientApp.Models;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MimeKit.Text;

namespace MailClientApp.Services
{
    public interface IEmailService
    {
        Task<EmailServiceResult> SendEmailAsync(EmailAccount account, EmailMessage email, CancellationToken cancellationToken = default);
        Task<EmailServiceResult> ReceiveEmailsAsync(EmailAccount account, string folder = "INBOX", int limit = 50, CancellationToken cancellationToken = default);
        Task<EmailServiceResult> SyncAccountAsync(EmailAccount account, Action<EmailMessage> onEmailReceived, Action<LogEntry> onLogEntry, CancellationToken cancellationToken = default);
        Task<EmailServiceResult> TestConnectionAsync(EmailAccount account, CancellationToken cancellationToken = default);
        Task<EmailServiceResult> DeleteEmailAsync(EmailAccount account, string messageId, string folder, CancellationToken cancellationToken = default);
        Task<EmailServiceResult> MoveEmailAsync(EmailAccount account, string messageId, string sourceFolder, string targetFolder, CancellationToken cancellationToken = default);
        Task<List<string>> GetFoldersAsync(EmailAccount account, CancellationToken cancellationToken = default);
    }

    public class EmailServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<EmailMessage> Emails { get; set; } = new List<EmailMessage>();
        public List<LogEntry> Logs { get; set; } = new List<LogEntry>();
        public Exception? Exception { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly ILoggerService _logger;

        public EmailService(ILoggerService logger)
        {
            _logger = logger;
        }

        public async Task<EmailServiceResult> SendEmailAsync(EmailAccount account, EmailMessage email, CancellationToken cancellationToken = default)
        {
            var result = new EmailServiceResult();
            var logEntry = LogEntry.CreateSendLog(account.Id, email.Id, "Начало отправки письма", $"От: {email.From}, Кому: {email.To}");
            
            try
            {
                _logger.Log(logEntry);
                result.Logs.Add(logEntry);

                var mimeMessage = new MimeMessage();
                mimeMessage.From.Add(new MailboxAddress(account.Name, account.Email));
                
                // To
                if (!string.IsNullOrEmpty(email.To))
                {
                    foreach (var to in email.To.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mimeMessage.To.Add(MailboxAddress.Parse(to.Trim()));
                    }
                }

                // Cc
                if (!string.IsNullOrEmpty(email.Cc))
                {
                    foreach (var cc in email.Cc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mimeMessage.Cc.Add(MailboxAddress.Parse(cc.Trim()));
                    }
                }

                // Bcc
                if (!string.IsNullOrEmpty(email.Bcc))
                {
                    foreach (var bcc in email.Bcc.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        mimeMessage.Bcc.Add(MailboxAddress.Parse(bcc.Trim()));
                    }
                }

                mimeMessage.Subject = email.Subject;

                // Body
                var bodyBuilder = new BodyBuilder();
                if (!string.IsNullOrEmpty(email.HtmlBody))
                {
                    bodyBuilder.HtmlBody = email.HtmlBody;
                }
                else if (!string.IsNullOrEmpty(email.Body))
                {
                    bodyBuilder.TextBody = email.Body;
                }

                // Attachments
                foreach (var attachment in email.Attachments)
                {
                    if (attachment.Content != null)
                    {
                        bodyBuilder.Attachments.Add(attachment.FileName, attachment.Content);
                    }
                }

                mimeMessage.Body = bodyBuilder.ToMessageBody();

                using var client = new SmtpClient();
                
                await client.ConnectAsync(account.SmtpServer, account.SmtpPort, account.SmtpUseSsl, cancellationToken);
                
                if (!string.IsNullOrEmpty(account.SmtpUsername))
                {
                    await client.AuthenticateAsync(account.SmtpUsername, account.SmtpPassword, cancellationToken);
                }

                await client.SendAsync(mimeMessage, cancellationToken);
                await client.DisconnectAsync(true, cancellationToken);

                logEntry = LogEntry.CreateSendLog(account.Id, email.Id, "Письмо успешно отправлено", $"ID: {mimeMessage.MessageId}");
                _logger.Log(logEntry);
                result.Logs.Add(logEntry);
                
                result.Success = true;
                result.Message = "Письмо успешно отправлено";
                email.MessageId = mimeMessage.MessageId;
            }
            catch (Exception ex)
            {
                logEntry = LogEntry.CreateErrorLog("EmailService.Send", ex.Message, ex.ToString(), account.Id, email.Id);
                _logger.Log(logEntry);
                result.Logs.Add(logEntry);
                
                result.Success = false;
                result.Message = "Ошибка при отправке письма";
                result.Exception = ex;
            }

            return result;
        }

        public async Task<EmailServiceResult> ReceiveEmailsAsync(EmailAccount account, string folder = "INBOX", int limit = 50, CancellationToken cancellationToken = default)
        {
            var result = new EmailServiceResult();
            var logEntry = LogEntry.CreateReceiveLog(account.Id, "", "Начало получения писем", $"Папка: {folder}, Лимит: {limit}");
            
            try
            {
                _logger.Log(logEntry);
                result.Logs.Add(logEntry);

                using var client = new ImapClient();
                
                await client.ConnectAsync(account.ImapServer, account.ImapPort, account.ImapUseSsl, cancellationToken);
                
                if (!string.IsNullOrEmpty(account.ImapUsername))
                {
                    await client.AuthenticateAsync(account.ImapUsername, account.ImapPassword, cancellationToken);
                }

                var inbox = await client.GetFolderAsync(folder, cancellationToken);
                await inbox.OpenAsync(FolderAccess.ReadOnly, cancellationToken);

                var messages = await inbox.FetchAsync(0, limit - 1, MessageSummaryItems.Full | MessageSummaryItems.UniqueId, cancellationToken);

                foreach (var summary in messages)
                {
                    var mimeMessage = await inbox.GetMessageAsync(summary.UniqueId, cancellationToken);
                    var email = EmailMessage.FromMimeMessage(mimeMessage, account.Id, folder);
                    result.Emails.Add(email);

                    logEntry = LogEntry.CreateReceiveLog(account.Id, email.Id, "Получено письмо", $"От: {email.From}, Тема: {email.Subject}");
                    _logger.Log(logEntry);
                    result.Logs.Add(logEntry);
                }

                await client.DisconnectAsync(true, cancellationToken);

                logEntry = LogEntry.CreateReceiveLog(account.Id, "", "Получение писем завершено", $"Получено: {result.Emails.Count} писем");
                _logger.Log(logEntry);
                result.Logs.Add(logEntry);
                
                result.Success = true;
                result.Message = $
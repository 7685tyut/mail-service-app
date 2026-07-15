using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MimeKit;

namespace MailClientApp.Models
{
    public class EmailMessage : INotifyPropertyChanged
    {
        private string _id;
        private string _messageId;
        private string _subject;
        private string _from;
        private string _to;
        private string _cc;
        private string _bcc;
        private DateTimeOffset _date;
        private string _body;
        private string _htmlBody;
        private bool _isHtml;
        private bool _isRead;
        private bool _isFlagged;
        private bool _hasAttachments;
        private long _size;
        private string _folder;
        private string _accountId;
        private List<EmailAttachment> _attachments;
        private List<string> _tags;

        public string Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }

        public string MessageId
        {
            get => _messageId;
            set { _messageId = value; OnPropertyChanged(); }
        }

        public string Subject
        {
            get => _subject;
            set { _subject = value; OnPropertyChanged(); }
        }

        public string From
        {
            get => _from;
            set { _from = value; OnPropertyChanged(); }
        }

        public string To
        {
            get => _to;
            set { _to = value; OnPropertyChanged(); }
        }

        public string Cc
        {
            get => _cc;
            set { _cc = value; OnPropertyChanged(); }
        }

        public string Bcc
        {
            get => _bcc;
            set { _bcc = value; OnPropertyChanged(); }
        }

        public DateTimeOffset Date
        {
            get => _date;
            set { _date = value; OnPropertyChanged(); }
        }

        public string Body
        {
            get => _body;
            set { _body = value; OnPropertyChanged(); }
        }

        public string HtmlBody
        {
            get => _htmlBody;
            set { _htmlBody = value; OnPropertyChanged(); }
        }

        public bool IsHtml
        {
            get => _isHtml;
            set { _isHtml = value; OnPropertyChanged(); }
        }

        public bool IsRead
        {
            get => _isRead;
            set { _isRead = value; OnPropertyChanged(); }
        }

        public bool IsFlagged
        {
            get => _isFlagged;
            set { _isFlagged = value; OnPropertyChanged(); }
        }

        public bool HasAttachments
        {
            get => _hasAttachments;
            set { _hasAttachments = value; OnPropertyChanged(); }
        }

        public long Size
        {
            get => _size;
            set { _size = value; OnPropertyChanged(); }
        }

        public string Folder
        {
            get => _folder;
            set { _folder = value; OnPropertyChanged(); }
        }

        public string AccountId
        {
            get => _accountId;
            set { _accountId = value; OnPropertyChanged(); }
        }

        public List<EmailAttachment> Attachments
        {
            get => _attachments ??= new List<EmailAttachment>();
            set { _attachments = value; OnPropertyChanged(); }
        }

        public List<string> Tags
        {
            get => _tags ??= new List<string>();
            set { _tags = value; OnPropertyChanged(); }
        }

        public EmailMessage()
        {
            Id = Guid.NewGuid().ToString();
            Date = DateTimeOffset.Now;
            Tags = new List<string>();
            Attachments = new List<EmailAttachment>();
        }

        public static EmailMessage FromMimeMessage(MimeMessage mimeMessage, string accountId, string folder)
        {
            var email = new EmailMessage
            {
                MessageId = mimeMessage.MessageId,
                Subject = mimeMessage.Subject,
                Date = mimeMessage.Date,
                AccountId = accountId,
                Folder = folder,
                Size = mimeMessage.Size
            };

            // From
            if (mimeMessage.From.Count > 0)
                email.From = string.Join(", ", mimeMessage.From);

            // To
            if (mimeMessage.To.Count > 0)
                email.To = string.Join(", ", mimeMessage.To);

            // Cc
            if (mimeMessage.Cc.Count > 0)
                email.Cc = string.Join(", ", mimeMessage.Cc);

            // Bcc
            if (mimeMessage.Bcc.Count > 0)
                email.Bcc = string.Join(", ", mimeMessage.Bcc);

            // Body
            if (mimeMessage.Body is TextPart textPart)
            {
                email.Body = textPart.Text;
                email.IsHtml = textPart.ContentType.MediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase);
            }
            else if (mimeMessage.Body is Multipart multipart)
            {
                foreach (var part in multipart)
                {
                    if (part is TextPart text)
                    {
                        if (text.ContentType.MediaType.Equals("text/html", StringComparison.OrdinalIgnoreCase))
                            email.HtmlBody = text.Text;
                        else if (text.ContentType.MediaType.Equals("text/plain", StringComparison.OrdinalIgnoreCase))
                            email.Body = text.Text;
                    }
                    else if (part is MimePart mimePart && mimePart.FileName != null)
                    {
                        email.HasAttachments = true;
                        email.Attachments.Add(new EmailAttachment
                        {
                            Id = Guid.NewGuid().ToString(),
                            FileName = mimePart.FileName,
                            ContentType = mimePart.ContentType.MediaType,
                            Size = mimePart.Content.Length
                        });
                    }
                }
            }

            return email;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class EmailAttachment
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long Size { get; set; }
        public byte[]? Content { get; set; }
    }
}
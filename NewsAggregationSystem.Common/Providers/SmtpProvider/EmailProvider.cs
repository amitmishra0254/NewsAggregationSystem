using Microsoft.Extensions.Options;
using NewsAggregationSystem.Common.DTOs.Providers;
using NewsAggregationSystem.Common.DTOs.SmtpProvider;
using System.Net;
using System.Net.Mail;

namespace NewsAggregationSystem.Common.Providers.SmtpProvider
{
    public class EmailProvider : IEmailProvider
    {
        private readonly EmailSettings _settings;
        public EmailProvider(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            using var client = new SmtpClient(_settings.SmtpServer, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            var mail = new MailMessage
            {
                From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mail.To.Add(to);
            await client.SendMailAsync(mail);
        }

        public async Task<int> SendBulkEmail(List<NotificationEmailDTO> emails)
        {
            var sendTasks = emails.Select(email => Task.Run(async () =>
            {
                using var client = new SmtpClient(_settings.SmtpServer, _settings.Port);
                client.UseDefaultCredentials = false;
                client.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
                client.EnableSsl = true;

                using var mail = new MailMessage
                {
                    From = new MailAddress(_settings.SenderEmail, _settings.SenderName),
                    Subject = email.Subject,
                    Body = email.Body,
                    IsBodyHtml = true
                };

                mail.To.Add(email.Email);

                await client.SendMailAsync(mail);
            }));

            await Task.WhenAll(sendTasks);
            return emails.Count;
        }
    }
}

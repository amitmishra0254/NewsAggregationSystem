using Microsoft.Extensions.Logging;
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
        private readonly ILogger<EmailProvider> logger;
        public EmailProvider(IOptions<EmailSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                logger.LogInformation("Sending email to: {To}, Subject: {Subject}", to, subject);
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
                logger.LogInformation("Email sent successfully to: {To}", to);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error occurred while sending email to: {To}", to);
                throw;
            }
        }

        public async Task<int> SendBulkEmail(List<NotificationEmailDTO> emails)
        {
            logger.LogInformation("Sending bulk emails. Count: {Count}", emails.Count);

            try
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
                        logger.LogInformation("Email sent to: {Email}", email.Email);
                    }));

                await Task.WhenAll(sendTasks);
                logger.LogInformation("Bulk email sending process completed. Attempted: {Count}", emails.Count);
                return emails.Count;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Bulk email sending process failed.");
                throw;
            }
        }
    }
}

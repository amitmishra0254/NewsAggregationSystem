using NewsAggregationSystem.Common.DTOs.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsAggregationSystem.Common.Providers.SmtpProvider
{
    public interface IEmailProvider
    {
        Task SendEmailAsync(string to, string subject, string body);
        Task<int> SendBulkEmail(List<NotificationEmailDTO> emails);
    }
}

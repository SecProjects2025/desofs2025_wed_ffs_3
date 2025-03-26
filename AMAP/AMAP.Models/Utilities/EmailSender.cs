using Microsoft.Extensions.Configuration;
using System.Net.Mail;
using System.Net;

namespace AMAP.Entities.Utilities
{
    public class EmailSender
    {
        private readonly string? _host;

        private readonly int _port;

        private readonly string? _username;

        private readonly string? _password;

        private readonly string _fromEmail;

        private readonly string? _fromName;

        public EmailSender(IConfiguration configuration)
        {
            var mailSettings = configuration.GetSection("MailSettings");
            _host = mailSettings["Host"];
            _port = int.Parse(mailSettings["Port"]);
            _username = mailSettings["Username"];
            _password = mailSettings["Password"];
            _fromEmail = mailSettings["FromEmail"];
            _fromName = mailSettings["FromName"];
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_username, _password),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_fromEmail, _fromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(toEmail);

            await client.SendMailAsync(mailMessage);
        }
    }
}

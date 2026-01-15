using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace LaoHR.API.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string body);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        try
        {
            var host = _configuration["SmtpSettings:Host"];
            var port = int.Parse(_configuration["SmtpSettings:Port"] ?? "587");
            var user = _configuration["SmtpSettings:User"];
            var pass = _configuration["SmtpSettings:Pass"];
            var fromEmail = _configuration["SmtpSettings:FromEmail"];
            var fromName = _configuration["SmtpSettings:FromName"];

            // Skip if no configuration (dev mode)
            if (string.IsNullOrEmpty(host) || host == "smtp.example.com")
            {
                _logger.LogWarning($"[Email Mock] To: {to}, Subject: {subject}");
                return;
            }

            var client = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(fromEmail ?? "noreply@laohr.local", fromName ?? "Lao HR System"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true
            };

            mailMessage.To.Add(to);

            await client.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email sent to {to}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to send email to {to}");
            // Don't throw, just log. Email failure shouldn't break the main flow.
        }
    }
}

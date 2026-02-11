using System.Net;
using System.Net.Mail;

namespace PentestHub.API.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var settings = _configuration.GetSection("EmailSettings");
        var host = settings["Host"];
        var port = int.Parse(settings["Port"] ?? "587");
        var senderEmail = settings["SenderEmail"];
        var senderPassword = settings["SenderPassword"];
        var enableSsl = bool.Parse(settings["EnableSsl"] ?? "true");

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(senderEmail))
        {
            _logger.LogError("SMTP settings are not configured properly.");
            return;
        }

        try
        {
            var smtpClient = new SmtpClient(host)
            {
                Port = port,
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = enableSsl,
            };

            var mailMessage = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };

            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
            _logger.LogInformation($"Email sent successfully to {to}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error sending email to {to}");
            if (host.Contains("gmail"))
            {
                _logger.LogError("Hint: If using Gmail, you MUST use an 'App Password', not your login password. Go to Google Account -> Security -> 2-Step Verification -> App Passwords.");
            }
            throw; // Keep rethrowing so the controller knows it failed, or maybe we should swallow it for production? 
                   // For this debugging session, rethrowing is better so the user sees the 500.
        }
    }
}

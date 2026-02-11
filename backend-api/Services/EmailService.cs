namespace PentestHub.API.Services;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendEmailAsync(string to, string subject, string body)
    {
        _logger.LogInformation($"[MOCK EMAIL SERVICE] Sending email to {to}");
        _logger.LogInformation($"Subject: {subject}");
        _logger.LogInformation($"Body: {body}");
        return Task.CompletedTask;
    }
}

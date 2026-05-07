using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using TaskManagement.Application.Common.Interfaces;

namespace TaskManagement.Infrastructure.Email;

public sealed class MailKitEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly ILogger<MailKitEmailSender> _logger;

    public MailKitEmailSender(IOptions<EmailOptions> options, ILogger<MailKitEmailSender> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(_options.FromName, _options.FromAddress));
        mime.To.Add(new MailboxAddress(message.ToDisplayName ?? message.To, message.To));
        mime.Subject = message.Subject;

        var body = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.PlainTextBody ?? StripHtml(message.HtmlBody)
        };
        mime.Body = body.ToMessageBody();

        if (string.Equals(_options.Mode, "PickupDirectory", StringComparison.OrdinalIgnoreCase))
        {
            Directory.CreateDirectory(_options.PickupPath);
            var path = Path.Combine(_options.PickupPath, $"{Guid.NewGuid():N}.eml");
            await using var fs = File.Create(path);
            await mime.WriteToAsync(fs, cancellationToken);
            _logger.LogInformation("Email written to pickup directory: {Path}", path);
            return;
        }

        using var client = new SmtpClient();
        try
        {
            var socket = _options.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;
            await client.ConnectAsync(_options.Host, _options.Port, socket, cancellationToken);
            if (!string.IsNullOrEmpty(_options.Username))
                await client.AuthenticateAsync(_options.Username, _options.Password, cancellationToken);
            await client.SendAsync(mime, cancellationToken);
        }
        finally
        {
            if (client.IsConnected)
                await client.DisconnectAsync(true, cancellationToken);
        }
    }

    private static string StripHtml(string html) =>
        System.Text.RegularExpressions.Regex.Replace(html, "<.*?>", string.Empty);
}

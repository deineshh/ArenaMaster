using MailKit.Net.Smtp;
using MimeKit;

namespace ArenaMaster.Api.Helpers;

public class SmtpSettings
{
    public string Host { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string User { get; set; } = string.Empty;
    public string Pass { get; set; } = string.Empty;
    public string FromName { get; set; } = "ArenaMaster";
}

public class EmailSender(SmtpSettings smtp, IConfiguration config)
{
    public async Task SendConfirmEmailAsync(string toEmail, string token)
    {
        var frontend = config["FRONTEND_URL"] ?? "http://localhost:5173";
        var link = $"{frontend}/confirm-email?token={Uri.EscapeDataString(token)}";
        var body = $"""
            <h2>Підтвердження email — ArenaMaster</h2>
            <p>Натисніть посилання для підтвердження:</p>
            <p><a href="{link}">{link}</a></p>
            """;

        await SendAsync(toEmail, "Підтвердження email — ArenaMaster", body);
    }

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        if (string.IsNullOrWhiteSpace(smtp.User))
        {
            Console.WriteLine($"[Email skipped] To: {to}, Subject: {subject}");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(smtp.FromName, smtp.User));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        using var client = new SmtpClient();
        await client.ConnectAsync(smtp.Host, smtp.Port, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(smtp.User, smtp.Pass);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}

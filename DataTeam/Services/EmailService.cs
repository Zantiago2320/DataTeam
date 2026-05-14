using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DataTeam.Services;

public interface IEmailService
{
    Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml);
    Task EnviarCorreoMultipleAsync(List<string> destinatarios, string asunto, string cuerpoHtml);
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

    public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
    {
        await EnviarCorreoMultipleAsync(new List<string> { destinatario }, asunto, cuerpoHtml);
    }

    public async Task EnviarCorreoMultipleAsync(List<string> destinatarios, string asunto, string cuerpoHtml)
    {
        try
        {
            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(
                _configuration["Email:FromName"] ?? "DateTeam",
                _configuration["Email:FromAddress"] ?? "noreply@dateteam.com"));

            foreach (var destinatario in destinatarios)
            {
                mensaje.To.Add(MailboxAddress.Parse(destinatario));
            }

            mensaje.Subject = asunto;

            var builder = new BodyBuilder
            {
                HtmlBody = cuerpoHtml
            };
            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];

            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);

            if (!string.IsNullOrEmpty(smtpUser) && !string.IsNullOrEmpty(smtpPassword))
            {
                await smtp.AuthenticateAsync(smtpUser, smtpPassword);
            }

            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation($"Correo enviado exitosamente a {string.Join(", ", destinatarios)}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar correo a {string.Join(", ", destinatarios)}");
            throw;
        }
    }
}

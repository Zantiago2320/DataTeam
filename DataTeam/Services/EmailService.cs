using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace DataTeam.Services;

public interface IEmailService
{
    Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml);
    Task EnviarCorreoMultipleAsync(List<string> destinatarios, string asunto, string cuerpoHtml);
    Task EnviarExcelPorCorreoAsync(string destinatario, string asunto, string cuerpoHtml, byte[] archivoExcel, string nombreArchivo);
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

    public async Task EnviarExcelPorCorreoAsync(string destinatario, string asunto, string cuerpoHtml, byte[] archivoExcel, string nombreArchivo)
    {
        try
        {
            // Validar que el archivo tenga contenido
            if (archivoExcel == null || archivoExcel.Length == 0)
            {
                throw new ArgumentException("El archivo Excel está vacío", nameof(archivoExcel));
            }

            // Validar configuración de correo
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
            {
                throw new InvalidOperationException("Las credenciales de correo no están configuradas. Verifique appsettings.json o User Secrets.");
            }

            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(
                _configuration["Email:FromName"] ?? "DateTeam",
                _configuration["Email:FromAddress"] ?? "noreply@dateteam.com"));

            mensaje.To.Add(MailboxAddress.Parse(destinatario));
            mensaje.Subject = asunto;

            // Construir cuerpo con attachment
            var builder = new BodyBuilder
            {
                HtmlBody = cuerpoHtml
            };

            // Agregar el archivo Excel como adjunto
            builder.Attachments.Add(nombreArchivo, archivoExcel, ContentType.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));

            mensaje.Body = builder.ToMessageBody();

            // Enviar correo
            using var smtp = new SmtpClient();

            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");

            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser, smtpPassword);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("Correo con Excel enviado exitosamente a {Destinatario}. Archivo: {Archivo} ({Tamaño} KB)", 
                destinatario, nombreArchivo, archivoExcel.Length / 1024);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo con Excel a {Destinatario}", destinatario);
            throw;
        }
    }
}

using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace DataTeam.Services;

public interface IEmailService
{
    Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml);
    Task EnviarCorreoMultipleAsync(List<string> destinatarios, string asunto, string cuerpoHtml);
    Task EnviarExcelPorCorreoAsync(string destinatario, string asunto, string cuerpoHtml, byte[] archivoExcel, string nombreArchivo);
    Task<bool> EnviarExcelPorCorreoMultipleAsync(List<string> destinatarios, string asunto, string cuerpoHtml, byte[] archivoExcel, string nombreArchivo);
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

    public async Task<bool> EnviarExcelPorCorreoMultipleAsync(List<string> destinatarios, string asunto, string cuerpoHtml, byte[] archivoExcel, string nombreArchivo)
    {
        try
        {
            // Validar entrada
            if (destinatarios == null || !destinatarios.Any())
            {
                throw new ArgumentException("Debe proporcionar al menos un destinatario", nameof(destinatarios));
            }

            if (archivoExcel == null || archivoExcel.Length == 0)
            {
                throw new ArgumentException("El archivo Excel está vacío", nameof(archivoExcel));
            }

            // Verificar si usar SendGrid o SMTP
            var useSendGrid = _configuration.GetValue<bool>("Email:UseSendGrid");
            var sendGridApiKey = _configuration["Email:SendGridApiKey"];

            if (useSendGrid && !string.IsNullOrEmpty(sendGridApiKey))
            {
                _logger.LogInformation("Enviando correo con SendGrid a {Count} destinatarios", destinatarios.Count);
                return await EnviarConSendGridAsync(destinatarios, asunto, cuerpoHtml, archivoExcel, nombreArchivo, sendGridApiKey);
            }
            else
            {
                _logger.LogInformation("Enviando correo con SMTP a {Count} destinatarios", destinatarios.Count);
                return await EnviarConSmtpAsync(destinatarios, asunto, cuerpoHtml, archivoExcel, nombreArchivo);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correo con Excel a múltiples destinatarios");
            return false;
        }
    }

    private async Task<bool> EnviarConSendGridAsync(List<string> destinatarios, string asunto, string cuerpoHtml, byte[] archivoExcel, string nombreArchivo, string apiKey)
    {
        try
        {
            var client = new SendGridClient(apiKey);

            var from = new EmailAddress(
                _configuration["Email:FromAddress"] ?? "noreply@datateam.com",
                _configuration["Email:FromName"] ?? "DataTeam"
            );

            // Crear lista de destinatarios para SendGrid
            var tos = destinatarios.Select(email => new EmailAddress(email)).ToList();

            var msg = new SendGridMessage
            {
                From = from,
                Subject = asunto,
                HtmlContent = cuerpoHtml
            };

            // SendGrid requiere agregar destinatarios individualmente para listas grandes
            msg.AddTos(tos);

            // Agregar attachment
            var attachmentBase64 = Convert.ToBase64String(archivoExcel);
            msg.AddAttachment(nombreArchivo, attachmentBase64, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation("✅ Email enviado exitosamente vía SendGrid a {Count} destinatarios. Archivo: {Archivo} ({Size} KB)",
                    destinatarios.Count, nombreArchivo, archivoExcel.Length / 1024);
                return true;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                _logger.LogError("❌ Error en SendGrid: {StatusCode} - {Body}", response.StatusCode, body);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Excepción al enviar con SendGrid");
            return false;
        }
    }

    private async Task<bool> EnviarConSmtpAsync(List<string> destinatarios, string asunto, string cuerpoHtml, byte[] archivoExcel, string nombreArchivo)
    {
        try
        {
            var smtpUser = _configuration["Email:SmtpUser"];
            var smtpPassword = _configuration["Email:SmtpPassword"];

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPassword))
            {
                _logger.LogWarning("⚠️ Credenciales SMTP no configuradas");
                return false;
            }

            var mensaje = new MimeMessage();
            mensaje.From.Add(new MailboxAddress(
                _configuration["Email:FromName"] ?? "DataTeam",
                _configuration["Email:FromAddress"] ?? "noreply@datateam.com"));

            // Agregar todos los destinatarios
            foreach (var dest in destinatarios)
            {
                mensaje.To.Add(MailboxAddress.Parse(dest));
            }

            mensaje.Subject = asunto;

            var builder = new BodyBuilder
            {
                HtmlBody = cuerpoHtml
            };

            builder.Attachments.Add(nombreArchivo, archivoExcel, ContentType.Parse("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));

            mensaje.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");

            await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(smtpUser, smtpPassword);
            await smtp.SendAsync(mensaje);
            await smtp.DisconnectAsync(true);

            _logger.LogInformation("✅ Email enviado exitosamente vía SMTP a {Count} destinatarios. Archivo: {Archivo} ({Size} KB)",
                destinatarios.Count, nombreArchivo, archivoExcel.Length / 1024);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Excepción al enviar con SMTP");
            return false;
        }
    }
}

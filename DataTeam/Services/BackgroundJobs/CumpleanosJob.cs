using DataTeam.Data;
using DataTeam.Models;
using DataTeam.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Services.BackgroundJobs;

public class CumpleanosJob
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<CumpleanosJob> _logger;

    public CumpleanosJob(ApplicationDbContext context, IEmailService emailService, ILogger<CumpleanosJob> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task EnviarCorreosCumpleanosDelMesAsync()
    {
        try
        {
            var primerDiaHabilDelMes = ObtenerPrimerDiaHabilDelMes();

            if (DateTime.Today != primerDiaHabilDelMes)
            {
                _logger.LogInformation("No es el primer día hábil del mes. No se enviarán correos de cumpleaños.");
                return;
            }

            var mesActual = DateTime.Today.Month;
            var consultoresConCumpleanos = await _context.Consultores
                .Include(c => c.Celula)
                .Where(c => c.Estado == EstadoConsultor.Activo && c.FechaNacimiento.Month == mesActual)
                .OrderBy(c => c.FechaNacimiento.Day)
                .ToListAsync();

            if (!consultoresConCumpleanos.Any())
            {
                _logger.LogInformation("No hay cumpleaños este mes.");
                return;
            }

            var cuerpoCorreo = GenerarCuerpoCorreoCumpleanos(consultoresConCumpleanos);

            // Obtener todos los correos de consultores activos
            var todosLosCorreos = await _context.Consultores
                .Where(c => c.Estado == EstadoConsultor.Activo)
                .Select(c => c.Correo)
                .ToListAsync();

            await _emailService.EnviarCorreoMultipleAsync(
                todosLosCorreos,
                $"🎂 Cumpleaños del mes - {DateTime.Today:MMMM yyyy}",
                cuerpoCorreo
            );

            _logger.LogInformation($"Correos de cumpleaños enviados exitosamente. Total: {consultoresConCumpleanos.Count} cumpleañeros.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar correos de cumpleaños");
            throw;
        }
    }

    private DateTime ObtenerPrimerDiaHabilDelMes()
    {
        var primerDia = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        while (primerDia.DayOfWeek == DayOfWeek.Saturday || primerDia.DayOfWeek == DayOfWeek.Sunday)
        {
            primerDia = primerDia.AddDays(1);
        }

        return primerDia;
    }

    private string GenerarCuerpoCorreoCumpleanos(List<Consultor> consultores)
    {
        var html = @"
<!DOCTYPE html>
<html>
<head>
    <style>
        body { font-family: Arial, sans-serif; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .header { background-color: #4CAF50; color: white; padding: 20px; text-align: center; }
        .content { padding: 20px; background-color: #f9f9f9; }
        .cumpleanero { background-color: white; margin: 10px 0; padding: 15px; border-left: 4px solid #4CAF50; }
        .fecha { color: #666; font-weight: bold; }
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🎂 Cumpleaños de " + DateTime.Today.ToString("MMMM yyyy") + @"</h1>
        </div>
        <div class='content'>
            <p>¡Hola equipo!</p>
            <p>Les recordamos los cumpleaños de este mes:</p>";

        foreach (var consultor in consultores)
        {
            html += $@"
            <div class='cumpleanero'>
                <strong>{consultor.Nombre}</strong> - {consultor.Cargo}<br/>
                <span class='fecha'>📅 {consultor.FechaNacimiento:dd 'de' MMMM}</span><br/>
                Célula: {consultor.Celula?.Nombre ?? "Sin asignar"}
            </div>";
        }

        html += @"
            <p>¡No olviden felicitarlos! 🎉</p>
        </div>
    </div>
</body>
</html>";

        return html;
    }
}

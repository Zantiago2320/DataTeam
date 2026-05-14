using DataTeam.Data;
using DataTeam.Services;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Services.BackgroundJobs;

public class ReporteMensualJob
{
    private readonly ApplicationDbContext _context;
    private readonly IExcelService _excelService;
    private readonly IEmailService _emailService;
    private readonly ILogger<ReporteMensualJob> _logger;

    public ReporteMensualJob(
        ApplicationDbContext context, 
        IExcelService excelService, 
        IEmailService emailService, 
        ILogger<ReporteMensualJob> logger)
    {
        _context = context;
        _excelService = excelService;
        _emailService = emailService;
        _logger = logger;
    }

    [AutomaticRetry(Attempts = 3)]
    public async Task EnviarReporteMensualAsync()
    {
        try
        {
            if (DateTime.Today.Day != 15)
            {
                _logger.LogInformation("No es el día 15 del mes. No se enviará reporte mensual.");
                return;
            }

            // Obtener estadísticas
            var totalConsultores = await _context.Consultores.CountAsync(c => c.Estado == Models.EstadoConsultor.Activo);
            var totalRetirados = await _context.Consultores.CountAsync(c => c.Estado == Models.EstadoConsultor.Retirado);
            var totalCelulas = await _context.Celulas.CountAsync(c => c.Activa);

            var consultoresPorCelula = await _context.Celulas
                .Include(c => c.Consultores)
                .Where(c => c.Activa)
                .Select(c => new
                {
                    c.Nombre,
                    TotalActivos = c.Consultores.Count(co => co.Estado == Models.EstadoConsultor.Activo)
                })
                .ToListAsync();

            var nuevosMes = await _context.Consultores
                .Where(c => c.FechaIngreso.Month == DateTime.Today.Month && c.FechaIngreso.Year == DateTime.Today.Year)
                .CountAsync();

            // Generar Excel con todos los consultores
            var excelBytes = await _excelService.ExportarConsultoresAsync();

            // Generar HTML del reporte
            var cuerpoHtml = GenerarCuerpoReporte(totalConsultores, totalRetirados, totalCelulas, consultoresPorCelula, nuevosMes);

            // TODO: Aquí deberías obtener los correos de administradores o lista específica
            var correosAdministradores = await _context.Consultores
                .Where(c => c.Estado == Models.EstadoConsultor.Activo)
                .Select(c => c.Correo)
                .Take(5) // Limitar para pruebas
                .ToListAsync();

            // Por ahora enviar sin adjunto (MailKit requiere más configuración para adjuntos)
            await _emailService.EnviarCorreoMultipleAsync(
                correosAdministradores,
                $"📊 Reporte Mensual de Consultores - {DateTime.Today:MMMM yyyy}",
                cuerpoHtml
            );

            _logger.LogInformation("Reporte mensual enviado exitosamente.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al enviar reporte mensual");
            throw;
        }
    }

    private string GenerarCuerpoReporte(int totalActivos, int totalRetirados, int totalCelulas, 
        object consultoresPorCelula, int nuevosMes)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #2196F3; color: white; padding: 20px; text-align: center; }}
        .content {{ padding: 20px; background-color: #f9f9f9; }}
        .stat-box {{ background-color: white; margin: 10px 0; padding: 15px; border-left: 4px solid #2196F3; }}
        .number {{ font-size: 24px; font-weight: bold; color: #2196F3; }}
        table {{ width: 100%; border-collapse: collapse; margin: 20px 0; background-color: white; }}
        th, td {{ padding: 12px; text-align: left; border-bottom: 1px solid #ddd; }}
        th {{ background-color: #2196F3; color: white; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>📊 Reporte Mensual de Consultores</h1>
            <p>{DateTime.Today:MMMM yyyy}</p>
        </div>
        <div class='content'>
            <h2>Resumen General</h2>

            <div class='stat-box'>
                <div class='number'>{totalActivos}</div>
                Consultores Activos
            </div>

            <div class='stat-box'>
                <div class='number'>{totalRetirados}</div>
                Consultores Retirados
            </div>

            <div class='stat-box'>
                <div class='number'>{totalCelulas}</div>
                Células Activas
            </div>

            <div class='stat-box'>
                <div class='number'>{nuevosMes}</div>
                Nuevos Ingresos Este Mes
            </div>

            <h2>Consultores por Célula</h2>
            <table>
                <thead>
                    <tr>
                        <th>Célula</th>
                        <th>Consultores Activos</th>
                    </tr>
                </thead>
                <tbody>";

        var celulas = consultoresPorCelula as IEnumerable<dynamic> ?? new List<dynamic>();
        foreach (var celula in celulas)
        {
            html += $@"
                    <tr>
                        <td>{celula.Nombre}</td>
                        <td>{celula.TotalActivos}</td>
                    </tr>";
        }

        html += @"
                </tbody>
            </table>

            <p><em>El reporte completo en Excel está disponible en el sistema.</em></p>
        </div>
    </div>
</body>
</html>";

        return html;
    }
}

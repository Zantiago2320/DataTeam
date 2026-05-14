using DataTeam.Data;
using DataTeam.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Services;

public interface IAuditoriaService
{
    Task RegistrarCambioAsync(string entidad, int entidadId, string accion, string? usuario, 
        object? valoresAnteriores, object? valoresNuevos, string? direccionIP, int? consultorId = null);
    Task<List<AuditoriaLog>> ObtenerAuditoriaPorEntidadAsync(string entidad, int entidadId);
    Task<List<AuditoriaLog>> ObtenerAuditoriaRecienteAsync(int cantidad = 50);
}

public class AuditoriaService : IAuditoriaService
{
    private readonly ApplicationDbContext _context;

    public AuditoriaService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RegistrarCambioAsync(string entidad, int entidadId, string accion, string? usuario,
        object? valoresAnteriores, object? valoresNuevos, string? direccionIP, int? consultorId = null)
    {
        var auditoria = new AuditoriaLog
        {
            Entidad = entidad,
            EntidadId = entidadId,
            Accion = accion,
            Usuario = usuario,
            ValoresAnteriores = valoresAnteriores != null ? System.Text.Json.JsonSerializer.Serialize(valoresAnteriores) : null,
            ValoresNuevos = valoresNuevos != null ? System.Text.Json.JsonSerializer.Serialize(valoresNuevos) : null,
            DireccionIP = direccionIP,
            ConsultorId = consultorId,
            Fecha = DateTime.Now
        };

        _context.AuditoriaLogs.Add(auditoria);
        await _context.SaveChangesAsync();
    }

    public async Task<List<AuditoriaLog>> ObtenerAuditoriaPorEntidadAsync(string entidad, int entidadId)
    {
        return await _context.AuditoriaLogs
            .Where(a => a.Entidad == entidad && a.EntidadId == entidadId)
            .OrderByDescending(a => a.Fecha)
            .ToListAsync();
    }

    public async Task<List<AuditoriaLog>> ObtenerAuditoriaRecienteAsync(int cantidad = 50)
    {
        return await _context.AuditoriaLogs
            .Include(a => a.Consultor)
            .OrderByDescending(a => a.Fecha)
            .Take(cantidad)
            .ToListAsync();
    }
}

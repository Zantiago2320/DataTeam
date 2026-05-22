using DataTeam.Data;
using DataTeam.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Services;

public interface IAuditoriaService
{
    Task RegistrarCambioAsync(string entidad, int entidadId, string accion, string? usuario, 
        object? valoresAnteriores, object? valoresNuevos, string? direccionIP, int? consultorId = null);
    Task<List<AuditoriaLog>> ObtenerAuditoriaPorEntidadAsync(string entidad, int entidadId);
    Task<List<AuditoriaLog>> ObtenerAuditoriaRecienteAsync(int cantidad = 50, int skip = 0);
}

public class AuditoriaService : IAuditoriaService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;

    public AuditoriaService(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task RegistrarCambioAsync(string entidad, int entidadId, string accion, string? usuario,
        object? valoresAnteriores, object? valoresNuevos, string? direccionIP, int? consultorId = null)
    {
        // Buscar el rol del usuario
        string? rolUsuario = null;
        if (!string.IsNullOrEmpty(usuario))
        {
            var user = await _userManager.FindByNameAsync(usuario);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                rolUsuario = roles.FirstOrDefault();
            }
        }

        var auditoria = new AuditoriaLog
        {
            Entidad = entidad,
            EntidadId = entidadId,
            Accion = accion,
            Usuario = usuario ?? "Sistema",
            RolUsuario = rolUsuario,
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

    public async Task<List<AuditoriaLog>> ObtenerAuditoriaRecienteAsync(int cantidad = 50, int skip = 0)
    {
        return await _context.AuditoriaLogs
            .Include(a => a.Consultor)
            .OrderByDescending(a => a.Fecha)
            .Skip(skip)
            .Take(cantidad)
            .ToListAsync();
    }
}

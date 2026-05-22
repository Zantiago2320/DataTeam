using DataTeam.Data;
using DataTeam.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Services;

public class EquipoService : IEquipoService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EquipoService> _logger;

    public EquipoService(ApplicationDbContext context, ILogger<EquipoService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Equipo>> ObtenerTodosAsync()
    {
        return await _context.Equipos
            .Include(e => e.EquipoLideres)
                .ThenInclude(el => el.Consultor)
            .Include(e => e.Consultores)
            .OrderBy(e => e.Nombre)
            .ToListAsync();
    }

    public async Task<List<Equipo>> ObtenerActivosAsync()
    {
        return await _context.Equipos
            .Include(e => e.EquipoLideres)
                .ThenInclude(el => el.Consultor)
            .Include(e => e.Consultores)
            .Where(e => e.Activo)
            .OrderBy(e => e.Nombre)
            .ToListAsync();
    }

    public async Task<Equipo?> ObtenerPorIdAsync(int id)
    {
        return await _context.Equipos
            .Include(e => e.EquipoLideres)
                .ThenInclude(el => el.Consultor)
            .Include(e => e.Consultores)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Equipo> CrearAsync(Equipo equipo, List<int>? lideresIds = null)
    {
        equipo.FechaCreacion = DateTime.Now;
        _context.Equipos.Add(equipo);
        await _context.SaveChangesAsync();

        if (lideresIds != null && lideresIds.Any())
        {
            foreach (var liderId in lideresIds)
            {
                var esLiderPrincipal = liderId == lideresIds.First();
                await AsignarLiderAsync(equipo.Id, liderId, esLiderPrincipal);
            }
        }

        _logger.LogInformation("Equipo {EquipoNombre} creado con ID {EquipoId}", equipo.Nombre, equipo.Id);
        return equipo;
    }

    public async Task<Equipo> ActualizarAsync(Equipo equipo, List<int>? lideresIds = null)
    {
        var equipoExistente = await _context.Equipos
            .Include(e => e.EquipoLideres)
            .FirstOrDefaultAsync(e => e.Id == equipo.Id);

        if (equipoExistente == null)
        {
            throw new InvalidOperationException($"Equipo con ID {equipo.Id} no encontrado");
        }

        equipoExistente.Nombre = equipo.Nombre;
        equipoExistente.Descripcion = equipo.Descripcion;
        equipoExistente.Color = equipo.Color;
        equipoExistente.Activo = equipo.Activo;
        equipoExistente.FechaModificacion = DateTime.Now;

        if (lideresIds != null)
        {
            _context.EquipoLideres.RemoveRange(equipoExistente.EquipoLideres);
            await _context.SaveChangesAsync();

            foreach (var liderId in lideresIds)
            {
                var esLiderPrincipal = liderId == lideresIds.First();
                await AsignarLiderAsync(equipo.Id, liderId, esLiderPrincipal);
            }
        }

        await _context.SaveChangesAsync();
        _logger.LogInformation("Equipo {EquipoNombre} actualizado", equipoExistente.Nombre);
        return equipoExistente;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        var equipo = await _context.Equipos.FindAsync(id);
        if (equipo == null) return false;

        _context.Equipos.Remove(equipo);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Equipo {EquipoNombre} eliminado", equipo.Nombre);
        return true;
    }

    public async Task<bool> CambiarEstadoAsync(int id, bool activo)
    {
        var equipo = await _context.Equipos.FindAsync(id);
        if (equipo == null) return false;

        equipo.Activo = activo;
        equipo.FechaModificacion = DateTime.Now;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Equipo {EquipoNombre} cambiado a {Estado}", equipo.Nombre, activo ? "Activo" : "Inactivo");
        return true;
    }

    public async Task<List<Consultor>> ObtenerMiembrosAsync(int equipoId)
    {
        return await _context.Consultores
            .Where(c => c.EquipoId == equipoId)
            .OrderBy(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<List<Consultor>> ObtenerLideresAsync(int equipoId)
    {
        return await _context.EquipoLideres
            .Where(el => el.EquipoId == equipoId)
            .Include(el => el.Consultor)
            .Select(el => el.Consultor!)
            .OrderByDescending(c => c.Nombre)
            .ToListAsync();
    }

    public async Task<bool> AsignarLiderAsync(int equipoId, int consultorId, bool esLiderPrincipal = false)
    {
        var equipoExiste = await _context.Equipos.AnyAsync(e => e.Id == equipoId);
        var consultorExiste = await _context.Consultores.AnyAsync(c => c.Id == consultorId);

        if (!equipoExiste || !consultorExiste) return false;

        var yaEsLider = await _context.EquipoLideres
            .AnyAsync(el => el.EquipoId == equipoId && el.ConsultorId == consultorId);

        if (yaEsLider) return false;

        if (esLiderPrincipal)
        {
            var lideresActuales = await _context.EquipoLideres
                .Where(el => el.EquipoId == equipoId && el.EsLiderPrincipal)
                .ToListAsync();

            foreach (var lider in lideresActuales)
            {
                lider.EsLiderPrincipal = false;
            }
        }

        var equipoLider = new EquipoLider
        {
            EquipoId = equipoId,
            ConsultorId = consultorId,
            EsLiderPrincipal = esLiderPrincipal,
            FechaAsignacion = DateTime.Now
        };

        _context.EquipoLideres.Add(equipoLider);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Líder {ConsultorId} asignado al equipo {EquipoId}", consultorId, equipoId);
        return true;
    }

    public async Task<bool> RemoverLiderAsync(int equipoId, int consultorId)
    {
        var equipoLider = await _context.EquipoLideres
            .FirstOrDefaultAsync(el => el.EquipoId == equipoId && el.ConsultorId == consultorId);

        if (equipoLider == null) return false;

        _context.EquipoLideres.Remove(equipoLider);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Líder {ConsultorId} removido del equipo {EquipoId}", consultorId, equipoId);
        return true;
    }

    public async Task<bool> AsignarMiembroAsync(int equipoId, int consultorId)
    {
        var consultor = await _context.Consultores.FindAsync(consultorId);
        if (consultor == null) return false;

        consultor.EquipoId = equipoId;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Miembro {ConsultorId} asignado al equipo {EquipoId}", consultorId, equipoId);
        return true;
    }

    public async Task<bool> RemoverMiembroAsync(int equipoId, int consultorId)
    {
        var consultor = await _context.Consultores
            .FirstOrDefaultAsync(c => c.Id == consultorId && c.EquipoId == equipoId);

        if (consultor == null) return false;

        consultor.EquipoId = null;
        await _context.SaveChangesAsync();
        _logger.LogInformation("Miembro {ConsultorId} removido del equipo {EquipoId}", consultorId, equipoId);
        return true;
    }
}

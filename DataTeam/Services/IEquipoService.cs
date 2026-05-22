using DataTeam.Models;

namespace DataTeam.Services;

public interface IEquipoService
{
    Task<List<Equipo>> ObtenerTodosAsync();
    Task<List<Equipo>> ObtenerActivosAsync();
    Task<Equipo?> ObtenerPorIdAsync(int id);
    Task<Equipo> CrearAsync(Equipo equipo, List<int>? lideresIds = null);
    Task<Equipo> ActualizarAsync(Equipo equipo, List<int>? lideresIds = null);
    Task<bool> EliminarAsync(int id);
    Task<bool> CambiarEstadoAsync(int id, bool activo);
    Task<List<Consultor>> ObtenerMiembrosAsync(int equipoId);
    Task<List<Consultor>> ObtenerLideresAsync(int equipoId);
    Task<bool> AsignarLiderAsync(int equipoId, int consultorId, bool esLiderPrincipal = false);
    Task<bool> RemoverLiderAsync(int equipoId, int consultorId);
    Task<bool> AsignarMiembroAsync(int equipoId, int consultorId);
    Task<bool> RemoverMiembroAsync(int equipoId, int consultorId);
}

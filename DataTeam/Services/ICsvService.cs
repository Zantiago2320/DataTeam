using DataTeam.Models;

namespace DataTeam.Services;

public interface ICsvService
{
    /// <summary>
    /// Lee todos los empleados del archivo CSV
    /// </summary>
    Task<List<EmpleadoDataTeam>> LeerEmpleadosAsync();

    /// <summary>
    /// Obtiene un empleado por su cédula
    /// </summary>
    Task<EmpleadoDataTeam?> ObtenerEmpleadoPorCedulaAsync(string cedula);

    /// <summary>
    /// Guarda todos los empleados al archivo CSV (sobrescribe)
    /// </summary>
    Task GuardarEmpleadosAsync(List<EmpleadoDataTeam> empleados);

    /// <summary>
    /// Actualiza un empleado específico en el CSV
    /// </summary>
    Task ActualizarEmpleadoAsync(EmpleadoDataTeam empleadoActualizado);

    /// <summary>
    /// Agrega un nuevo empleado al CSV
    /// </summary>
    Task AgregarEmpleadoAsync(EmpleadoDataTeam nuevoEmpleado);

    /// <summary>
    /// Obtiene empleados con paginación
    /// </summary>
    Task<(List<EmpleadoDataTeam> empleados, int total)> ObtenerEmpleadosPaginadosAsync(
        int pagina = 1, 
        int porPagina = 50, 
        string? filtro = null,
        string? celula = null);

    /// <summary>
    /// Obtiene la lista única de células (considerando que un empleado puede estar en varias)
    /// </summary>
    Task<List<string>> ObtenerCelulasUnicasAsync();
}

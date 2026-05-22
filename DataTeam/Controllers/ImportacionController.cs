using DataTeam.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using SystemFile = System.IO.File;

namespace DataTeam.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class ImportacionController : Controller
{
    private readonly ICsvService _csvService;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ILogger<ImportacionController> _logger;
    private readonly IWebHostEnvironment _environment;

    public ImportacionController(
        ICsvService csvService,
        IAuditoriaService auditoriaService,
        ILogger<ImportacionController> logger,
        IWebHostEnvironment environment)
    {
        _csvService = csvService;
        _auditoriaService = auditoriaService;
        _logger = logger;
        _environment = environment;
    }

    // GET: Importacion
    public IActionResult Index()
    {
        return View();
    }

    // POST: Importacion/SubirCsv
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubirCsv(IFormFile archivo)
    {
        if (archivo == null || archivo.Length == 0)
        {
            TempData["Error"] = "Por favor selecciona un archivo CSV válido";
            return RedirectToAction(nameof(Index));
        }

        if (!archivo.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
        {
            TempData["Error"] = "El archivo debe tener extensión .csv";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            // Crear backup del archivo actual
            var csvPath = Path.Combine(_environment.ContentRootPath, "DATE TEAM 1.1.csv");
            if (SystemFile.Exists(csvPath))
            {
                var backupPath = Path.Combine(
                    _environment.ContentRootPath, 
                    "Backups", 
                    $"DATE_TEAM_backup_{DateTime.Now:yyyyMMdd_HHmmss}.csv");

                Directory.CreateDirectory(Path.GetDirectoryName(backupPath)!);
                SystemFile.Copy(csvPath, backupPath, true);
                _logger.LogInformation($"Backup creado en: {backupPath}");
            }

            // Guardar el nuevo archivo
            using (var stream = new FileStream(csvPath, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            // Verificar que se puede leer
            var empleados = await _csvService.LeerEmpleadosAsync();

            // Registrar en auditoría
            await _auditoriaService.RegistrarCambioAsync(
                entidad: "Sistema",
                entidadId: 0,
                accion: "Importar CSV",
                usuario: User.Identity?.Name ?? "Anónimo",
                valoresAnteriores: null,
                valoresNuevos: $"Archivo: {archivo.FileName}, Registros: {empleados.Count}",
                direccionIP: HttpContext.Connection.RemoteIpAddress?.ToString() ?? "N/A"
            );

            TempData["Success"] = $"✅ Archivo CSV importado exitosamente. Se cargaron {empleados.Count} empleados.";
            _logger.LogInformation($"CSV importado: {archivo.FileName} con {empleados.Count} registros");

            return RedirectToAction("Index", "Empleados");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al importar archivo CSV");
            TempData["Error"] = $"Error al importar el archivo: {ex.Message}";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: Importacion/DescargarPlantilla
    public IActionResult DescargarPlantilla()
    {
        try
        {
            var plantilla = new StringBuilder();

            // Encabezado con todas las columnas esperadas
            plantilla.AppendLine("\"Cédula\",\"Nombre\",\"Correo\",\"Nombre del Cargo Oficial\",\"Desarrollo\",\"Rol\",\"% participación\",\"Célula\",\"Lider\",\"Empresa\",\"Udemy\",\"ARL\",\"Ciudad\",\"Fecha de Cumpleaños\",\"Mes Cumple\",\"Dirección\",\"Barrio\",\"Telefono Fijo\",\"Tel Celular\",\"Contacto adicional\",\"N° contacto adicional\",\"Fecha Ingreso\",\"Número de Renovaciones\",\"Fecha renovación actual\",\"Fecha Vto Contrato\",\"Inducción\",\"Plan Entrenamiento\",\"Fecha Vto PP\",\"PP\",\"Visual\",\"Estado\",\"VAC - Inicio\",\"VAC - Final\",\"VAC - Reintegro\",\"Mes Vacaciones\",\"Saldo Vacaciones 2025\",\"Días tomados 2025\",\"Días pendientes 2025\",\"Horario de trabajo\",\"Observaciones\"");

            // Fila de ejemplo
            plantilla.AppendLine("\"1234567890\",\"Juan Pérez\",\"jperez@example.com\",\"Desarrollador Senior\",\"x\",\"Ingeniero\",\"100%\",\"Backend\",\"María González\",\"AEL\",\"Sí\",\"Positiva\",\"Bogotá\",\"15/03/1990\",\"Marzo\",\"Calle 123 #45-67\",\"Centro\",\"6011234567\",\"3001234567\",\"María Pérez\",\"3009876543\",\"01/01/2020\",\"2\",\"01/01/2023\",\"31/12/2023\",\"OK\",\"Completado\",\"15/06/2023\",\"OK\",\"Indefinido\",\"Activo\",\"01/07/2023\",\"15/07/2023\",\"16/07/2023\",\"Julio\",\"15\",\"5\",\"10\",\"8:00 - 17:00\",\"\"");

            var bytes = Encoding.UTF8.GetBytes(plantilla.ToString());
            return File(bytes, "text/csv", $"Plantilla_Empleados_{DateTime.Now:yyyyMMdd}.csv");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar plantilla CSV");
            TempData["Error"] = "Error al generar la plantilla";
            return RedirectToAction(nameof(Index));
        }
    }
}

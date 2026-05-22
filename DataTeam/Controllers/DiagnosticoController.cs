using DataTeam.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Controllers;

[Authorize]
public class DiagnosticoController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DiagnosticoController> _logger;

    public DiagnosticoController(ApplicationDbContext context, ILogger<DiagnosticoController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: /Diagnostico
    public async Task<IActionResult> Index()
    {
        var diagnostico = new
        {
            TotalConsultores = await _context.Consultores.CountAsync(),
            ConsultoresActivos = await _context.Consultores.Where(c => c.Estado == Models.EstadoConsultor.Activo).CountAsync(),
            ConsultoresEliminados = await _context.Consultores.Where(c => c.Eliminado).CountAsync(),
            TotalCelulas = await _context.Celulas.CountAsync(),
            CelulasActivas = await _context.Celulas.Where(c => c.Activa).CountAsync(),
            TotalEquipos = await _context.Equipos.CountAsync(),
            TotalUsuarios = await _context.Users.CountAsync(),
            DistribucionPorCelula = await _context.Consultores
                .Where(c => !c.Eliminado)
                .GroupBy(c => c.Celula!.Nombre)
                .Select(g => new { Celula = g.Key, Cantidad = g.Count() })
                .ToListAsync(),
            Top10Consultores = await _context.Consultores
                .Where(c => !c.Eliminado)
                .OrderBy(c => c.Nombre)
                .Take(10)
                .Select(c => new { c.Cedula, c.Nombre, c.Correo, Celula = c.Celula!.Nombre })
                .ToListAsync()
        };

        return Json(diagnostico);
    }

    // GET: /Diagnostico/ReiniciarDatos
    [HttpGet]
    public IActionResult ReiniciarDatos()
    {
        return View();
    }

    // POST: /Diagnostico/ReiniciarDatos
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReiniciarDatosConfirmado()
    {
        try
        {
            // Limpiar todas las tablas
            _context.Consultores.RemoveRange(_context.Consultores);
            _context.Celulas.RemoveRange(_context.Celulas);
            _context.Equipos.RemoveRange(_context.Equipos);
            _context.CelulaLideres.RemoveRange(_context.CelulaLideres);
            _context.EquipoLideres.RemoveRange(_context.EquipoLideres);
            _context.EquipoMiembros.RemoveRange(_context.EquipoMiembros);

            await _context.SaveChangesAsync();

            _logger.LogWarning("🔄 Base de datos limpiada. Se recomienda reiniciar la aplicación.");

            TempData["Mensaje"] = "Base de datos limpiada. Por favor, REINICIA LA APLICACIÓN para cargar los datos nuevamente.";
            TempData["TipoMensaje"] = "warning";

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al limpiar la base de datos");
            TempData["Error"] = $"Error al limpiar: {ex.Message}";
            return RedirectToAction(nameof(ReiniciarDatos));
        }
    }
}

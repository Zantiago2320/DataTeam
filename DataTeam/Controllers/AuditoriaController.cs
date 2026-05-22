using DataTeam.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DataTeam.Controllers;

[Authorize(Roles = "SuperAdmin,Admin")]
public class AuditoriaController : Controller
{
    private readonly IAuditoriaService _auditoriaService;
    private readonly IAuditoriaLogFormatterService _formatterService;
    private readonly ILogger<AuditoriaController> _logger;

    public AuditoriaController(
        IAuditoriaService auditoriaService,
        IAuditoriaLogFormatterService formatterService,
        ILogger<AuditoriaController> logger)
    {
        _auditoriaService = auditoriaService;
        _formatterService = formatterService;
        _logger = logger;
    }

    // GET: Auditoria
    public async Task<IActionResult> Index(int pagina = 1, int registrosPorPagina = 50)
    {
        try
        {
            var logs = await _auditoriaService.ObtenerAuditoriaRecienteAsync(registrosPorPagina, (pagina - 1) * registrosPorPagina);

            ViewBag.PaginaActual = pagina;
            ViewBag.RegistrosPorPagina = registrosPorPagina;
            ViewBag.FormatterService = _formatterService;

            return View(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar auditoría");
            TempData["Error"] = "Error al cargar el registro de auditoría";
            return RedirectToAction("Index", "Home");
        }
    }

    // GET: Auditoria/Entidad?tipo=Consultor&id=5
    public async Task<IActionResult> Entidad(string tipo, int id)
    {
        if (string.IsNullOrEmpty(tipo) || id <= 0)
        {
            return BadRequest("Debe especificar tipo de entidad e ID válido");
        }

        try
        {
            var logs = await _auditoriaService.ObtenerAuditoriaPorEntidadAsync(tipo, id);

            ViewBag.TipoEntidad = tipo;
            ViewBag.EntidadId = id;

            return View(logs);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cargar auditoría de entidad {Tipo} {Id}", tipo, id);
            TempData["Error"] = $"Error al cargar auditoría de {tipo} {id}";
            return RedirectToAction("Index");
        }
    }
}

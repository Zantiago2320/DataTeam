using DataTeam.Data;
using DataTeam.Models;
using DataTeam.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Controllers;

[Authorize]
public class ProcesoContratacionController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ILogger<ProcesoContratacionController> _logger;

    public ProcesoContratacionController(
        ApplicationDbContext context,
        IAuditoriaService auditoriaService,
        ILogger<ProcesoContratacionController> logger)
    {
        _context = context;
        _auditoriaService = auditoriaService;
        _logger = logger;
    }

    // GET: ProcesoContratacion
    public async Task<IActionResult> Index(string? status, string? celula)
    {
        var procesos = _context.ProcesosContratacion.AsQueryable();

        if (!string.IsNullOrEmpty(status))
        {
            procesos = procesos.Where(p => p.Status == status);
        }

        if (!string.IsNullOrEmpty(celula))
        {
            procesos = procesos.Where(p => p.Celula == celula);
        }

        var lista = await procesos.OrderByDescending(p => p.FechaCreacion).ToListAsync();

        ViewBag.StatusList = await _context.ProcesosContratacion
            .Where(p => p.Status != null)
            .Select(p => p.Status)
            .Distinct()
            .ToListAsync();

        ViewBag.CelulaList = await _context.ProcesosContratacion
            .Where(p => p.Celula != null)
            .Select(p => p.Celula)
            .Distinct()
            .ToListAsync();

        return View(lista);
    }

    // GET: ProcesoContratacion/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var proceso = await _context.ProcesosContratacion.FindAsync(id);
        if (proceso == null) return NotFound();

        return View(proceso);
    }

    // GET: ProcesoContratacion/Create
    [Authorize(Roles = "SuperAdmin,Admin")]
    public IActionResult Create()
    {
        var proceso = new ProcesoContratacion
        {
            FechaIngreso = DateTime.Today.AddMonths(1)
        };
        return View(proceso);
    }

    // POST: ProcesoContratacion/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Create(ProcesoContratacion proceso)
    {
        if (ModelState.IsValid)
        {
            try
            {
                proceso.FechaCreacion = DateTime.Now;
                _context.Add(proceso);
                await _context.SaveChangesAsync();

                await _auditoriaService.RegistrarCambioAsync(
                    "ProcesoContratacion",
                    proceso.Id,
                    "Crear",
                    User.Identity?.Name,
                    null,
                    proceso,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                TempData["Success"] = "Proceso de contratación creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear proceso de contratación");
                ModelState.AddModelError("", "Error al crear el proceso");
            }
        }

        return View(proceso);
    }

    // GET: ProcesoContratacion/Edit/5
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null) return NotFound();

        var proceso = await _context.ProcesosContratacion.FindAsync(id);
        if (proceso == null) return NotFound();

        return View(proceso);
    }

    // POST: ProcesoContratacion/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> Edit(int id, ProcesoContratacion proceso)
    {
        if (id != proceso.Id) return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                var procesoActual = await _context.ProcesosContratacion.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

                proceso.FechaActualizacion = DateTime.Now;
                _context.Update(proceso);
                await _context.SaveChangesAsync();

                await _auditoriaService.RegistrarCambioAsync(
                    "ProcesoContratacion",
                    proceso.Id,
                    "Editar",
                    User.Identity?.Name,
                    procesoActual,
                    proceso,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                TempData["Success"] = "Proceso actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await ProcesoExists(proceso.Id))
                {
                    return NotFound();
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar proceso");
                ModelState.AddModelError("", "Error al actualizar el proceso");
            }
        }

        return View(proceso);
    }

    // GET: ProcesoContratacion/Delete/5
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null) return NotFound();

        var proceso = await _context.ProcesosContratacion
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(p => p.Id == id);

        if (proceso == null) return NotFound();

        return View(proceso);
    }

    // POST: ProcesoContratacion/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var proceso = await _context.ProcesosContratacion
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proceso != null)
            {
                var estadoAnterior = new { proceso.Eliminado, proceso.FechaEliminacion, proceso.EliminadoPor };

                proceso.Eliminado = true;
                proceso.FechaEliminacion = DateTime.Now;
                proceso.EliminadoPor = User.Identity?.Name ?? "Sistema";

                await _context.SaveChangesAsync();

                await _auditoriaService.RegistrarCambioAsync(
                    "ProcesoContratacion",
                    proceso.Id,
                    "Deshabilitar",
                    User.Identity?.Name,
                    estadoAnterior,
                    new { proceso.Eliminado, proceso.FechaEliminacion, proceso.EliminadoPor },
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                TempData["Success"] = "Proceso deshabilitado exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al deshabilitar proceso");
            TempData["Error"] = "Error al deshabilitar el proceso";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }

    private async Task<bool> ProcesoExists(int id)
    {
        return await _context.ProcesosContratacion.AnyAsync(e => e.Id == id);
    }
}

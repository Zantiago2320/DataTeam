using DataTeam.Data;
using DataTeam.Models;
using DataTeam.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Controllers;

[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
public class EquiposController : Controller
{
    private readonly IEquipoService _equipoService;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<EquiposController> _logger;

    public EquiposController(
        IEquipoService equipoService,
        ApplicationDbContext context,
        ILogger<EquiposController> logger)
    {
        _equipoService = equipoService;
        _context = context;
        _logger = logger;
    }

    // GET: Equipos
    public async Task<IActionResult> Index()
    {
        var equipos = await _equipoService.ObtenerTodosAsync();
        return View(equipos);
    }

    // GET: Equipos/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var equipo = await _equipoService.ObtenerPorIdAsync(id.Value);
        if (equipo == null)
        {
            return NotFound();
        }

        ViewBag.Lideres = await _equipoService.ObtenerLideresAsync(id.Value);
        ViewBag.Miembros = await _equipoService.ObtenerMiembrosAsync(id.Value);

        return View(equipo);
    }

    // GET: Equipos/Create
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Create()
    {
        await CargarConsultoresDisponibles();
        return View();
    }

    // POST: Equipos/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Create(Equipo equipo, List<int>? lideresIds)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _equipoService.CrearAsync(equipo, lideresIds);
                TempData["Success"] = $"Equipo '{equipo.Nombre}' creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear equipo");
                ModelState.AddModelError("", "Error al crear el equipo. Por favor, intente nuevamente.");
            }
        }

        await CargarConsultoresDisponibles();
        return View(equipo);
    }

    // GET: Equipos/Edit/5
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var equipo = await _equipoService.ObtenerPorIdAsync(id.Value);
        if (equipo == null)
        {
            return NotFound();
        }

        await CargarConsultoresDisponibles();

        // Cargar líderes actuales
        var lideresActuales = equipo.EquipoLideres.Select(el => el.ConsultorId).ToList();
        ViewBag.LideresActuales = lideresActuales;

        return View(equipo);
    }

    // POST: Equipos/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Edit(int id, Equipo equipo, List<int>? lideresIds)
    {
        if (id != equipo.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _equipoService.ActualizarAsync(equipo, lideresIds);
                TempData["Success"] = $"Equipo '{equipo.Nombre}' actualizado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EquipoExists(equipo.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar equipo");
                ModelState.AddModelError("", "Error al actualizar el equipo. Por favor, intente nuevamente.");
            }
        }

        await CargarConsultoresDisponibles();
        var lideresActuales = (await _equipoService.ObtenerPorIdAsync(id))?.EquipoLideres.Select(el => el.ConsultorId).ToList();
        ViewBag.LideresActuales = lideresActuales ?? new List<int>();

        return View(equipo);
    }

    // GET: Equipos/Delete/5
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var equipo = await _equipoService.ObtenerPorIdAsync(id.Value);
        if (equipo == null)
        {
            return NotFound();
        }

        ViewBag.Lideres = await _equipoService.ObtenerLideresAsync(id.Value);
        ViewBag.Miembros = await _equipoService.ObtenerMiembrosAsync(id.Value);

        return View(equipo);
    }

    // POST: Equipos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var equipo = await _equipoService.ObtenerPorIdAsync(id);
            if (equipo == null)
            {
                return NotFound();
            }

            // Verificar si tiene miembros asignados
            var miembros = await _equipoService.ObtenerMiembrosAsync(id);
            if (miembros.Any())
            {
                TempData["Error"] = $"No se puede eliminar el equipo '{equipo.Nombre}' porque tiene {miembros.Count} miembro(s) asignado(s). Primero reasigne o elimine los miembros.";
                return RedirectToAction(nameof(Index));
            }

            await _equipoService.EliminarAsync(id);
            TempData["Success"] = $"Equipo '{equipo.Nombre}' eliminado exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar equipo");
            TempData["Error"] = "Error al eliminar el equipo. Por favor, intente nuevamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Equipos/CambiarEstado/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> CambiarEstado(int id, bool activo)
    {
        try
        {
            var resultado = await _equipoService.CambiarEstadoAsync(id, activo);
            if (resultado)
            {
                TempData["Success"] = $"Estado del equipo cambiado a {(activo ? "Activo" : "Inactivo")}.";
            }
            else
            {
                TempData["Error"] = "No se encontró el equipo.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado del equipo");
            TempData["Error"] = "Error al cambiar el estado del equipo.";
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Equipos/AsignarMiembros/5
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> AsignarMiembros(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var equipo = await _equipoService.ObtenerPorIdAsync(id.Value);
        if (equipo == null)
        {
            return NotFound();
        }

        // Consultores disponibles (sin equipo o en este equipo)
        var consultoresDisponibles = await _context.Consultores
            .Where(c => !c.Eliminado && (c.EquipoId == null || c.EquipoId == id))
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        ViewBag.Equipo = equipo;
        ViewBag.MiembrosActuales = await _equipoService.ObtenerMiembrosAsync(id.Value);
        ViewBag.ConsultoresDisponibles = consultoresDisponibles;

        return View();
    }

    // POST: Equipos/AsignarMiembro
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> AsignarMiembro(int equipoId, int consultorId)
    {
        try
        {
            var resultado = await _equipoService.AsignarMiembroAsync(equipoId, consultorId);
            if (resultado)
            {
                TempData["Success"] = "Miembro asignado exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo asignar el miembro.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar miembro");
            TempData["Error"] = "Error al asignar el miembro.";
        }

        return RedirectToAction(nameof(AsignarMiembros), new { id = equipoId });
    }

    // POST: Equipos/RemoverMiembro
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> RemoverMiembro(int equipoId, int consultorId)
    {
        try
        {
            var resultado = await _equipoService.RemoverMiembroAsync(equipoId, consultorId);
            if (resultado)
            {
                TempData["Success"] = "Miembro removido exitosamente.";
            }
            else
            {
                TempData["Error"] = "No se pudo remover el miembro.";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover miembro");
            TempData["Error"] = "Error al remover el miembro.";
        }

        return RedirectToAction(nameof(AsignarMiembros), new { id = equipoId });
    }

    private async Task CargarConsultoresDisponibles()
    {
        var consultores = await _context.Consultores
            .Where(c => !c.Eliminado && c.Estado == EstadoConsultor.Activo)
            .OrderBy(c => c.Nombre)
            .Select(c => new { c.Id, c.Nombre, c.Cargo })
            .ToListAsync();

        ViewBag.Consultores = new MultiSelectList(
            consultores,
            "Id",
            "Nombre"
        );
    }

    private async Task<bool> EquipoExists(int id)
    {
        return await _context.Equipos.AnyAsync(e => e.Id == id);
    }
}

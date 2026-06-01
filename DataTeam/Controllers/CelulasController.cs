using DataTeam.Data;
using DataTeam.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Controllers;

[Authorize(Roles = $"{AppRoles.SuperAdmin},{AppRoles.Admin}")]
public class CelulasController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CelulasController> _logger;
    private readonly IWebHostEnvironment _env;

    public CelulasController(ApplicationDbContext context, ILogger<CelulasController> logger, IWebHostEnvironment env)
    {
        _context = context;
        _logger = logger;
        _env = env;
    }

    // GET: Celulas
    public async Task<IActionResult> Index()
    {
        var celulas = await _context.Celulas
            .Include(c => c.CelulaLideres)
                .ThenInclude(cl => cl.Consultor)
            .Include(c => c.Consultores)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return View(celulas);
    }

    // GET: Celulas/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var celula = await _context.Celulas
            .Include(c => c.CelulaLideres)
                .ThenInclude(cl => cl.Consultor)
            .Include(c => c.CelulaMiembros)
                .ThenInclude(cm => cm.Consultor)
            .Include(c => c.Consultores)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (celula == null)
        {
            return NotFound();
        }

        return View(celula);
    }

    // GET: Celulas/Create
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Create()
    {
        await CargarConsultoresDisponibles();
        return View();
    }

    // POST: Celulas/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Create(Celula celula, List<int>? lideresIds, IFormFile? imagenFile)
    {
        if (ModelState.IsValid)
        {
            try
            {
                celula.FechaCreacion = DateTime.Now;

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    celula.ImagenUrl = await GuardarImagenAsync(imagenFile);
                }

                _context.Celulas.Add(celula);
                await _context.SaveChangesAsync();

                // Asignar líderes si se proporcionaron
                if (lideresIds != null && lideresIds.Any())
                {
                    for (int i = 0; i < lideresIds.Count; i++)
                    {
                        var celulaLider = new CelulaLider
                        {
                            CelulaId = celula.Id,
                            ConsultorId = lideresIds[i],
                            EsLiderPrincipal = i == 0, // El primero es el líder principal
                            FechaAsignacion = DateTime.Now
                        };
                        _context.CelulaLideres.Add(celulaLider);
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["Success"] = $"Célula '{celula.Nombre}' creada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear célula");
                ModelState.AddModelError("", "Error al crear la célula. Por favor, intente nuevamente.");
            }
        }

        await CargarConsultoresDisponibles();
        return View(celula);
    }

    // GET: Celulas/Edit/5
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var celula = await _context.Celulas
            .Include(c => c.CelulaLideres)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (celula == null)
        {
            return NotFound();
        }

        await CargarConsultoresDisponibles();

        // Cargar líderes actuales
        var lideresActuales = celula.CelulaLideres.Select(cl => cl.ConsultorId).ToList();
        ViewBag.LideresActuales = lideresActuales;

        return View(celula);
    }

    // POST: Celulas/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Edit(int id, Celula celula, List<int>? lideresIds, IFormFile? imagenFile)
    {
        if (id != celula.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var celulaExistente = await _context.Celulas
                    .Include(c => c.CelulaLideres)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (celulaExistente == null)
                {
                    return NotFound();
                }

                celulaExistente.Nombre = celula.Nombre;
                celulaExistente.Descripcion = celula.Descripcion;
                celulaExistente.Color = celula.Color;
                celulaExistente.Activa = celula.Activa;
                celulaExistente.FechaModificacion = DateTime.Now;

                if (imagenFile != null && imagenFile.Length > 0)
                {
                    celulaExistente.ImagenUrl = await GuardarImagenAsync(imagenFile);
                }

                // Actualizar líderes si se proporcionaron
                if (lideresIds != null)
                {
                    // Remover líderes actuales
                    _context.CelulaLideres.RemoveRange(celulaExistente.CelulaLideres);
                    await _context.SaveChangesAsync();

                    // Agregar nuevos líderes
                    for (int i = 0; i < lideresIds.Count; i++)
                    {
                        var celulaLider = new CelulaLider
                        {
                            CelulaId = celula.Id,
                            ConsultorId = lideresIds[i],
                            EsLiderPrincipal = i == 0, // El primero es el líder principal
                            FechaAsignacion = DateTime.Now
                        };
                        _context.CelulaLideres.Add(celulaLider);
                    }
                }

                await _context.SaveChangesAsync();
                TempData["Success"] = $"Célula '{celula.Nombre}' actualizada exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CelulaExists(celula.Id))
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
                _logger.LogError(ex, "Error al actualizar célula");
                ModelState.AddModelError("", "Error al actualizar la célula. Por favor, intente nuevamente.");
            }
        }

        await CargarConsultoresDisponibles();
        var lideresActuales = (await _context.Celulas
            .Include(c => c.CelulaLideres)
            .FirstOrDefaultAsync(c => c.Id == id))?.CelulaLideres.Select(cl => cl.ConsultorId).ToList();
        ViewBag.LideresActuales = lideresActuales ?? new List<int>();

        return View(celula);
    }

    // GET: Celulas/Delete/5
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var celula = await _context.Celulas
            .Include(c => c.CelulaLideres)
                .ThenInclude(cl => cl.Consultor)
            .Include(c => c.Consultores)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (celula == null)
        {
            return NotFound();
        }

        return View(celula);
    }

    // POST: Celulas/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var celula = await _context.Celulas
                .Include(c => c.Consultores)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (celula == null)
            {
                return NotFound();
            }

            // Verificar si tiene miembros asignados
            if (celula.Consultores.Any())
            {
                TempData["Error"] = $"No se puede eliminar la célula '{celula.Nombre}' porque tiene {celula.Consultores.Count} miembro(s) asignado(s). Primero reasigne o elimine los miembros.";
                return RedirectToAction(nameof(Index));
            }

            _context.Celulas.Remove(celula);
            await _context.SaveChangesAsync();
            TempData["Success"] = $"Célula '{celula.Nombre}' eliminada exitosamente.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar célula");
            TempData["Error"] = "Error al eliminar la célula. Por favor, intente nuevamente.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: Celulas/CambiarEstado/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> CambiarEstado(int id, bool activa)
    {
        try
        {
            var celula = await _context.Celulas.FindAsync(id);
            if (celula == null)
            {
                TempData["Error"] = "No se encontró la célula.";
                return RedirectToAction(nameof(Index));
            }

            celula.Activa = activa;
            celula.FechaModificacion = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Estado de la célula cambiado a {(activa ? "Activa" : "Inactiva")}.";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al cambiar estado de la célula");
            TempData["Error"] = "Error al cambiar el estado de la célula.";
        }

        return RedirectToAction(nameof(Index));
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

    // GET: Celulas/AsignarMiembro/5
    public async Task<IActionResult> AsignarMiembro(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var celula = await _context.Celulas.FindAsync(id);
        if (celula == null)
        {
            return NotFound();
        }

        ViewBag.CelulaId = id;
        ViewBag.CelulaNombre = celula.Nombre;

        // Cargar consultores que aún no están en la célula
        var consultoresYaAsignados = await _context.CelulaMiembros
            .Where(cm => cm.CelulaId == id)
            .Select(cm => cm.ConsultorId)
            .ToListAsync();

        var consultoresDisponibles = await _context.Consultores
            .Where(c => !c.Eliminado && c.Estado == EstadoConsultor.Activo && !consultoresYaAsignados.Contains(c.Id))
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        ViewBag.ConsultoresDisponibles = new SelectList(consultoresDisponibles, "Id", "Nombre");

        return View();
    }

    // POST: Celulas/AsignarMiembro
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AsignarMiembro(int celulaId, int consultorId, string rol)
    {
        if (string.IsNullOrWhiteSpace(rol))
        {
            TempData["Error"] = "El rol es obligatorio";
            return RedirectToAction(nameof(AsignarMiembro), new { id = celulaId });
        }

        // Verificar que no exista ya esta asignación
        var existe = await _context.CelulaMiembros
            .AnyAsync(cm => cm.CelulaId == celulaId && cm.ConsultorId == consultorId);

        if (existe)
        {
            TempData["Error"] = "Este consultor ya está asignado a la célula";
            return RedirectToAction(nameof(AsignarMiembro), new { id = celulaId });
        }

        var celulaMiembro = new CelulaMiembro
        {
            CelulaId = celulaId,
            ConsultorId = consultorId,
            Rol = rol,
            FechaAsignacion = DateTime.Now
        };

        _context.CelulaMiembros.Add(celulaMiembro);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Miembro asignado exitosamente a la célula";
        return RedirectToAction(nameof(Details), new { id = celulaId });
    }

    // POST: Celulas/RemoverMiembro/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoverMiembro(int id, int celulaId)
    {
        var celulaMiembro = await _context.CelulaMiembros.FindAsync(id);
        if (celulaMiembro != null)
        {
            _context.CelulaMiembros.Remove(celulaMiembro);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Miembro removido de la célula exitosamente";
        }

        return RedirectToAction(nameof(Details), new { id = celulaId });
    }

    private async Task<string> GuardarImagenAsync(IFormFile file)
    {
        var uploadsPath = Path.Combine(_env.WebRootPath, "images", "celulas");
        Directory.CreateDirectory(uploadsPath);
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = $"celula_{Guid.NewGuid():N}{ext}";
        var filePath = Path.Combine(uploadsPath, fileName);
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        return $"/images/celulas/{fileName}";
    }

    private bool CelulaExists(int id)
    {
        return _context.Celulas.Any(c => c.Id == id);
    }
}

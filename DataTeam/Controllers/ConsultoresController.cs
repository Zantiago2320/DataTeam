using DataTeam.Data;
using DataTeam.Models;
using DataTeam.Services;
using DataTeam.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Controllers;

[Authorize]
public class ConsultoresController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IFileService _fileService;
    private readonly IAuditoriaService _auditoriaService;
    private readonly IExcelService _excelService;
    private readonly ILogger<ConsultoresController> _logger;

    public ConsultoresController(
        ApplicationDbContext context,
        IFileService fileService,
        IAuditoriaService auditoriaService,
        IExcelService excelService,
        ILogger<ConsultoresController> logger)
    {
        _context = context;
        _fileService = fileService;
        _auditoriaService = auditoriaService;
        _excelService = excelService;
        _logger = logger;
    }

    // GET: Consultores
    public async Task<IActionResult> Index(string? buscar, int? celulaId, EstadoConsultor? estado)
    {
        var query = _context.Consultores
            .Include(c => c.Celula)
            .AsQueryable();

        // Filtrar por búsqueda
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            query = query.Where(c =>
                c.Nombre.Contains(buscar) ||
                c.Cedula.Contains(buscar) ||
                c.Correo.Contains(buscar) ||
                c.Cargo.Contains(buscar));
        }

        // Filtrar por célula
        if (celulaId.HasValue && celulaId.Value > 0)
        {
            query = query.Where(c => c.CelulaId == celulaId.Value);
        }

        // Filtrar por estado
        if (estado.HasValue)
        {
            query = query.Where(c => c.Estado == estado.Value);
        }

        var consultores = await query
            .OrderBy(c => c.Celula!.Nombre)
            .ThenBy(c => c.Nombre)
            .ToListAsync();

        // Cargar células para el filtro
        ViewBag.Celulas = await _context.Celulas
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        ViewBag.BuscarActual = buscar;
        ViewBag.CelulaIdActual = celulaId;
        ViewBag.EstadoActual = estado;

        return View(consultores);
    }

    // GET: Consultores/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var consultor = await _context.Consultores
            .Include(c => c.Celula)
            .Include(c => c.Auditorias)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (consultor == null)
        {
            return NotFound();
        }

        return View(consultor);
    }

    // GET: Consultores/Create
    public async Task<IActionResult> Create()
    {
        var viewModel = new ConsultorViewModel
        {
            FechaIngreso = DateTime.Today,
            FechaNacimiento = DateTime.Today.AddYears(-25),
            Estado = EstadoConsultor.Activo
        };

        ViewBag.Celulas = await _context.Celulas
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return View(viewModel);
    }

    // POST: Consultores/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ConsultorViewModel viewModel)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Verificar si la cédula ya existe
                if (await _context.Consultores.AnyAsync(c => c.Cedula == viewModel.Cedula))
                {
                    ModelState.AddModelError("Cedula", "Ya existe un consultor con esta cédula");
                    ViewBag.Celulas = await _context.Celulas.Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync();
                    return View(viewModel);
                }

                // Verificar si el correo ya existe
                if (await _context.Consultores.AnyAsync(c => c.Correo == viewModel.Correo))
                {
                    ModelState.AddModelError("Correo", "Ya existe un consultor con este correo");
                    ViewBag.Celulas = await _context.Celulas.Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync();
                    return View(viewModel);
                }

                // Procesar foto
                string? rutaFoto = null;
                if (viewModel.FotoFile != null && viewModel.FotoFile.Length > 0)
                {
                    rutaFoto = await _fileService.GuardarFotoAsync(viewModel.FotoFile, viewModel.Cedula);
                }
                else
                {
                    rutaFoto = _fileService.ObtenerRutaFotoPorDefecto();
                }

                // Crear consultor
                var consultor = new Consultor
                {
                    Cedula = viewModel.Cedula,
                    Nombre = viewModel.Nombre,
                    Correo = viewModel.Correo,
                    Cargo = viewModel.Cargo,
                    RutaFoto = rutaFoto,
                    FechaIngreso = viewModel.FechaIngreso,
                    FechaNacimiento = viewModel.FechaNacimiento,
                    CelulaId = viewModel.CelulaId,
                    Rol = viewModel.Rol,
                    Capacidad = viewModel.Capacidad,
                    Empresa = viewModel.Empresa,
                    Direccion = viewModel.Direccion,
                    Barrio = viewModel.Barrio,
                    Celular = viewModel.Celular,
                    ContactoEmergencia = viewModel.ContactoEmergencia,
                    CelularEmergencia = viewModel.CelularEmergencia,
                    Estado = viewModel.Estado,
                    FechaCreacion = DateTime.Now
                };

                _context.Add(consultor);
                await _context.SaveChangesAsync();

                // Registrar auditoría
                await _auditoriaService.RegistrarCambioAsync(
                    "Consultor",
                    consultor.Id,
                    "Crear",
                    User.Identity?.Name,
                    null,
                    consultor,
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    consultor.Id
                );

                TempData["Success"] = "Consultor creado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear consultor");
                ModelState.AddModelError("", "Error al crear el consultor. Por favor, intente nuevamente.");
            }
        }

        ViewBag.Celulas = await _context.Celulas.Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync();
        return View(viewModel);
    }

    // GET: Consultores/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var consultor = await _context.Consultores.FindAsync(id);
        if (consultor == null)
        {
            return NotFound();
        }

        var viewModel = new ConsultorViewModel
        {
            Id = consultor.Id,
            Cedula = consultor.Cedula,
            Nombre = consultor.Nombre,
            Correo = consultor.Correo,
            Cargo = consultor.Cargo,
            RutaFoto = consultor.RutaFoto,
            FechaIngreso = consultor.FechaIngreso,
            FechaNacimiento = consultor.FechaNacimiento,
            CelulaId = consultor.CelulaId,
            Rol = consultor.Rol,
            Capacidad = consultor.Capacidad,
            Empresa = consultor.Empresa,
            Direccion = consultor.Direccion,
            Barrio = consultor.Barrio,
            Celular = consultor.Celular,
            ContactoEmergencia = consultor.ContactoEmergencia,
            CelularEmergencia = consultor.CelularEmergencia,
            Estado = consultor.Estado
        };

        ViewBag.Celulas = await _context.Celulas.Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync();
        return View(viewModel);
    }

    // POST: Consultores/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ConsultorViewModel viewModel)
    {
        if (id != viewModel.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                var consultor = await _context.Consultores.FindAsync(id);
                if (consultor == null)
                {
                    return NotFound();
                }

                // Guardar valores anteriores para auditoría
                var valoresAnteriores = new
                {
                    consultor.Cedula,
                    consultor.Nombre,
                    consultor.Correo,
                    consultor.Cargo,
                    consultor.CelulaId,
                    consultor.Rol,
                    consultor.Capacidad,
                    consultor.Estado
                };

                // Verificar si la cédula ya existe (excepto el actual)
                if (await _context.Consultores.AnyAsync(c => c.Cedula == viewModel.Cedula && c.Id != id))
                {
                    ModelState.AddModelError("Cedula", "Ya existe un consultor con esta cédula");
                    ViewBag.Celulas = await _context.Celulas.Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync();
                    return View(viewModel);
                }

                // Verificar si el correo ya existe (excepto el actual)
                if (await _context.Consultores.AnyAsync(c => c.Correo == viewModel.Correo && c.Id != id))
                {
                    ModelState.AddModelError("Correo", "Ya existe un consultor con este correo");
                    ViewBag.Celulas = await _context.Celulas.Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync();
                    return View(viewModel);
                }

                // Procesar foto si hay una nueva
                if (viewModel.FotoFile != null && viewModel.FotoFile.Length > 0)
                {
                    // Eliminar foto anterior si no es la por defecto
                    if (!string.IsNullOrEmpty(consultor.RutaFoto))
                    {
                        await _fileService.EliminarFotoAsync(consultor.RutaFoto);
                    }

                    consultor.RutaFoto = await _fileService.GuardarFotoAsync(viewModel.FotoFile, viewModel.Cedula);
                }

                // Actualizar datos
                consultor.Cedula = viewModel.Cedula;
                consultor.Nombre = viewModel.Nombre;
                consultor.Correo = viewModel.Correo;
                consultor.Cargo = viewModel.Cargo;
                consultor.FechaIngreso = viewModel.FechaIngreso;
                consultor.FechaNacimiento = viewModel.FechaNacimiento;
                consultor.CelulaId = viewModel.CelulaId;
                consultor.Rol = viewModel.Rol;
                consultor.Capacidad = viewModel.Capacidad;
                consultor.Empresa = viewModel.Empresa;
                consultor.Direccion = viewModel.Direccion;
                consultor.Barrio = viewModel.Barrio;
                consultor.Celular = viewModel.Celular;
                consultor.ContactoEmergencia = viewModel.ContactoEmergencia;
                consultor.CelularEmergencia = viewModel.CelularEmergencia;
                consultor.Estado = viewModel.Estado;
                consultor.FechaActualizacion = DateTime.Now;

                _context.Update(consultor);
                await _context.SaveChangesAsync();

                // Registrar auditoría
                await _auditoriaService.RegistrarCambioAsync(
                    "Consultor",
                    consultor.Id,
                    "Editar",
                    User.Identity?.Name,
                    valoresAnteriores,
                    new
                    {
                        consultor.Cedula,
                        consultor.Nombre,
                        consultor.Correo,
                        consultor.Cargo,
                        consultor.CelulaId,
                        consultor.Rol,
                        consultor.Capacidad,
                        consultor.Estado
                    },
                    HttpContext.Connection.RemoteIpAddress?.ToString(),
                    consultor.Id
                );

                TempData["Success"] = "Consultor actualizado exitosamente";
                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConsultorExists(viewModel.Id))
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
                _logger.LogError(ex, "Error al actualizar consultor");
                ModelState.AddModelError("", "Error al actualizar el consultor. Por favor, intente nuevamente.");
            }
        }

        ViewBag.Celulas = await _context.Celulas.Where(c => c.Activa).OrderBy(c => c.Nombre).ToListAsync();
        return View(viewModel);
    }

    // GET: Consultores/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var consultor = await _context.Consultores
            .Include(c => c.Celula)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (consultor == null)
        {
            return NotFound();
        }

        return View(consultor);
    }

    // POST: Consultores/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            var consultor = await _context.Consultores.FindAsync(id);
            if (consultor != null)
            {
                // Eliminar foto si existe
                if (!string.IsNullOrEmpty(consultor.RutaFoto))
                {
                    await _fileService.EliminarFotoAsync(consultor.RutaFoto);
                }

                // Registrar auditoría antes de eliminar
                await _auditoriaService.RegistrarCambioAsync(
                    "Consultor",
                    consultor.Id,
                    "Eliminar",
                    User.Identity?.Name,
                    consultor,
                    null,
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                _context.Consultores.Remove(consultor);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Consultor eliminado exitosamente";
            }

            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar consultor");
            TempData["Error"] = "Error al eliminar el consultor. Puede que tenga datos relacionados.";
            return RedirectToAction(nameof(Delete), new { id });
        }
    }

    // GET: Consultores/ExportarExcel
    public async Task<IActionResult> ExportarExcel(int? celulaId)
    {
        try
        {
            byte[] excelBytes;
            string fileName;

            if (celulaId.HasValue && celulaId.Value > 0)
            {
                excelBytes = await _excelService.ExportarConsultoresPorCelulaAsync(celulaId.Value);
                var celula = await _context.Celulas.FindAsync(celulaId.Value);
                fileName = $"Consultores_{celula?.Nombre}_{DateTime.Now:yyyyMMdd}.xlsx";
            }
            else
            {
                excelBytes = await _excelService.ExportarConsultoresAsync();
                fileName = $"Consultores_{DateTime.Now:yyyyMMdd}.xlsx";
            }

            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al exportar consultores a Excel");
            TempData["Error"] = "Error al generar el archivo Excel";
            return RedirectToAction(nameof(Index));
        }
    }

    private bool ConsultorExists(int id)
    {
        return _context.Consultores.Any(e => e.Id == id);
    }
}

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
    private readonly IEmailService _emailService;
    private readonly ILogger<ConsultoresController> _logger;

    public ConsultoresController(
        ApplicationDbContext context,
        IFileService fileService,
        IAuditoriaService auditoriaService,
        IExcelService excelService,
        IEmailService emailService,
        ILogger<ConsultoresController> logger)
    {
        _context = context;
        _fileService = fileService;
        _auditoriaService = auditoriaService;
        _excelService = excelService;
        _emailService = emailService;
        _logger = logger;
    }

    // GET: Consultores
    public async Task<IActionResult> Index(string? buscar, int? celulaId, EstadoConsultor? estado, string? cargo, string? ordenarPor)
    {
        // SEGURIDAD: Validar parámetro de búsqueda para prevenir inyección
        if (!string.IsNullOrWhiteSpace(buscar) && buscar.Length > 100)
        {
            _logger.LogWarning("Búsqueda rechazada por longitud excesiva: {Length}", buscar.Length);
            ModelState.AddModelError("buscar", "El término de búsqueda es demasiado largo");
            buscar = null;
        }

        // SEGURIDAD: Validar ID de célula
        if (celulaId.HasValue && celulaId.Value < 0)
        {
            _logger.LogWarning("ID de célula inválido: {CelulaId}", celulaId.Value);
            celulaId = null;
        }

        var query = _context.Consultores
            .Include(c => c.Celula)
            .Include(c => c.CelulasMiembro)
                .ThenInclude(cm => cm.Celula)
            .AsQueryable();

        // Filtrar por búsqueda
        if (!string.IsNullOrWhiteSpace(buscar))
        {
            // SEGURIDAD: Sanitizar término de búsqueda
            var buscarSanitizado = buscar.Trim();
            query = query.Where(c =>
                c.Nombre.Contains(buscarSanitizado) ||
                c.Cedula.Contains(buscarSanitizado) ||
                c.Correo.Contains(buscarSanitizado) ||
                c.Cargo.Contains(buscarSanitizado));
        }

        // Filtrar por célula (buscar en célula principal O en células miembro)
        if (celulaId.HasValue && celulaId.Value > 0)
        {
            query = query.Where(c => 
                c.CelulaId == celulaId.Value || 
                c.CelulasMiembro.Any(cm => cm.CelulaId == celulaId.Value));
        }

        // Filtrar por estado
        if (estado.HasValue)
        {
            query = query.Where(c => c.Estado == estado.Value);
        }

        // Filtrar por cargo
        if (!string.IsNullOrWhiteSpace(cargo))
        {
            // SEGURIDAD: Validar longitud de cargo
            var cargoSanitizado = cargo.Trim();
            if (cargoSanitizado.Length <= 100)
            {
                query = query.Where(c => c.Cargo == cargoSanitizado);
            }
        }

        // SEGURIDAD: Validar parámetro de ordenamiento contra lista permitida
        var ordenamientosPermitidos = new[] { "celula", "estado", "cargo", "fecha_nuevo", "fecha_antiguo" };
        if (!string.IsNullOrWhiteSpace(ordenarPor) && !ordenamientosPermitidos.Contains(ordenarPor.ToLowerInvariant()))
        {
            _logger.LogWarning("Ordenamiento inválido detectado: {Ordenamiento}", ordenarPor);
            ordenarPor = null;
        }

        // Aplicar ordenamiento
        query = ordenarPor?.ToLowerInvariant() switch
        {
            "celula" => query.OrderBy(c => c.Celula!.Nombre).ThenBy(c => c.Nombre),
            "estado" => query.OrderBy(c => c.Estado).ThenBy(c => c.Nombre),
            "cargo" => query.OrderBy(c => c.Cargo).ThenBy(c => c.Nombre),
            "fecha_nuevo" => query.OrderByDescending(c => c.FechaIngreso).ThenBy(c => c.Nombre),
            "fecha_antiguo" => query.OrderBy(c => c.FechaIngreso).ThenBy(c => c.Nombre),
            _ => query.OrderBy(c => c.Celula!.Nombre).ThenBy(c => c.Nombre) // Por defecto
        };

        var consultores = await query.ToListAsync();

        // Cargar células para el filtro
        ViewBag.Celulas = await _context.Celulas
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        // Cargar lista de cargos únicos para el filtro
        ViewBag.Cargos = await _context.Consultores
            .Where(c => !c.Eliminado)
            .Select(c => c.Cargo)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        ViewBag.BuscarActual = buscar;
        ViewBag.CelulaIdActual = celulaId;
        ViewBag.EstadoActual = estado;
        ViewBag.CargoActual = cargo;
        ViewBag.OrdenarPorActual = ordenarPor;

        return View(consultores);
    }

    // GET: Consultores/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        // SEGURIDAD: Validar ID
        if (id == null || id.Value <= 0)
        {
            _logger.LogWarning("Intento de acceder a detalles con ID inválido: {Id}", id);
            return NotFound();
        }

        var consultor = await _context.Consultores
            .Include(c => c.Celula)
            .Include(c => c.CelulasMiembro)
                .ThenInclude(cm => cm.Celula)
            .Include(c => c.CelulasQueLidera)
                .ThenInclude(cl => cl.Celula)
            .Include(c => c.Auditorias)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (consultor == null)
        {
            return NotFound();
        }

        return View(consultor);
    }

    // GET: Consultores/Create
    [Authorize(Roles = "SuperAdmin,Admin")]
    public IActionResult Create()
    {
        var viewModel = new ConsultorViewModel
        {
            FechaIngreso = DateTime.Today,
            FechaNacimiento = DateTime.Today.AddYears(-25),
            Estado = EstadoConsultor.Activo
        };

        return View(viewModel);
    }

    // POST: Consultores/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
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
                    return View(viewModel);
                }

                // Verificar si el correo ya existe
                if (await _context.Consultores.AnyAsync(c => c.Correo == viewModel.Correo))
                {
                    ModelState.AddModelError("Correo", "Ya existe un consultor con este correo");
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
    [Authorize(Roles = "SuperAdmin,Admin")]
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
    [Authorize(Roles = "SuperAdmin,Admin")]
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
                var consultor = await _context.Consultores
                    .Include(c => c.Celula)
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (consultor == null)
                {
                    return NotFound();
                }

                // Obtener nombre de célula anterior para auditoría
                var celulaNombreAnterior = consultor.Celula?.Nombre;

                // Guardar valores anteriores para auditoría
                var valoresAnteriores = new
                {
                    consultor.Cedula,
                    consultor.Nombre,
                    consultor.Correo,
                    consultor.Cargo,
                    Celula = celulaNombreAnterior,
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

                // Obtener nombre de célula nueva para auditoría
                var celulaNombreNueva = viewModel.CelulaId.HasValue 
                    ? (await _context.Celulas.FindAsync(viewModel.CelulaId.Value))?.Nombre
                    : null;

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
                        Celula = celulaNombreNueva,
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
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var consultor = await _context.Consultores
            .IgnoreQueryFilters() // Incluir consultores eliminados
            .Include(c => c.Celula)
            .FirstOrDefaultAsync(m => m.Id == id);

        if (consultor == null)
        {
            return NotFound();
        }

        return View(consultor);
    }

    // POST: Consultores/Delete/5 (Soft Delete)
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        // Redirigir a la nueva página de deshabilitación con motivo
        return RedirectToAction(nameof(Deshabilitar), new { id });
    }

    // GET: Consultores/Deshabilitar/5
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Deshabilitar(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var consultor = await _context.Consultores
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultor == null)
        {
            return NotFound();
        }

        var viewModel = new DeshabilitarConsultorViewModel
        {
            ConsultorId = consultor.Id,
            ConsultorNombre = consultor.Nombre,
            ConsultorCedula = consultor.Cedula,
            FechaRetiro = DateTime.Now
        };

        return View(viewModel);
    }

    // POST: Consultores/ConfirmarDeshabilitar
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> ConfirmarDeshabilitar(DeshabilitarConsultorViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View("Deshabilitar", model);
        }

        try
        {
            var consultor = await _context.Consultores
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == model.ConsultorId);

            if (consultor == null)
            {
                return NotFound();
            }

            // Capturar estado anterior para auditoría
            var estadoAnterior = new
            {
                consultor.Eliminado,
                consultor.Estado,
                consultor.FechaEliminacion,
                consultor.EliminadoPor,
                consultor.FechaRetiro,
                consultor.TipoDesvinculacion,
                consultor.MotivoRetiro
            };

            // Marcar como eliminado y retirado
            consultor.Eliminado = true;
            consultor.Estado = EstadoConsultor.Retirado;
            consultor.FechaEliminacion = DateTime.Now;
            consultor.EliminadoPor = User.Identity?.Name ?? "Sistema";
            consultor.FechaRetiro = model.FechaRetiro;
            consultor.TipoDesvinculacion = model.TipoDesvinculacion;
            consultor.MotivoRetiro = $"[{model.TipoDesvinculacion}] {model.MotivoDetallado}";

            await _context.SaveChangesAsync();

            // Registrar auditoría
            await _auditoriaService.RegistrarCambioAsync(
                "Consultor",
                consultor.Id,
                "Deshabilitar",
                User.Identity?.Name,
                estadoAnterior,
                new 
                { 
                    consultor.Eliminado, 
                    consultor.Estado,
                    consultor.FechaEliminacion, 
                    consultor.EliminadoPor,
                    consultor.FechaRetiro,
                    consultor.TipoDesvinculacion,
                    consultor.MotivoRetiro
                },
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            TempData["Success"] = $"Consultor {consultor.Nombre} deshabilitado exitosamente. Motivo: {model.TipoDesvinculacion}";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al deshabilitar consultor");
            TempData["Error"] = "Error al deshabilitar el consultor.";
            return View("Deshabilitar", model);
        }
    }

    // GET: Consultores/Eliminados (Ver consultores deshabilitados)
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Eliminados()
    {
        var consultores = await _context.Consultores
            .IgnoreQueryFilters()
            .Where(c => c.Eliminado)
            .Include(c => c.Celula)
            .OrderByDescending(c => c.FechaEliminacion)
            .ToListAsync();

        return View(consultores);
    }

    // POST: Consultores/Restaurar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> Restaurar(int id)
    {
        try
        {
            var consultor = await _context.Consultores
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.Id == id && c.Eliminado);

            if (consultor != null)
            {
                var estadoAnterior = new
                {
                    consultor.Eliminado,
                    consultor.FechaEliminacion,
                    consultor.EliminadoPor
                };

                consultor.Eliminado = false;
                consultor.FechaEliminacion = null;
                consultor.EliminadoPor = null;

                await _context.SaveChangesAsync();

                // Registrar auditoría
                await _auditoriaService.RegistrarCambioAsync(
                    "Consultor",
                    consultor.Id,
                    "Restaurar",
                    User.Identity?.Name,
                    estadoAnterior,
                    new { consultor.Eliminado },
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                TempData["Success"] = "Consultor restaurado exitosamente";
            }

            return RedirectToAction(nameof(Eliminados));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al restaurar consultor");
            TempData["Error"] = "Error al restaurar el consultor.";
            return RedirectToAction(nameof(Eliminados));
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

    // API: Consultores/PrevisualizarExcel
    [HttpGet]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> PrevisualizarExcel(int? celulaId)
    {
        try
        {
            // Generar Excel
            byte[] excelBytes;
            if (celulaId.HasValue && celulaId.Value > 0)
            {
                excelBytes = await _excelService.ExportarConsultoresPorCelulaAsync(celulaId.Value);
            }
            else
            {
                excelBytes = await _excelService.ExportarConsultoresAsync();
            }

            // Leer Excel y extraer primeras filas para previsualización
            using var stream = new MemoryStream(excelBytes);
            using var workbook = new ClosedXML.Excel.XLWorkbook(stream);
            var worksheet = workbook.Worksheets.First();

            var preview = new
            {
                TotalFilas = worksheet.RowsUsed().Count() - 1, // Menos el header
                Columnas = worksheet.Row(1).CellsUsed().Select(c => c.Value.ToString()).ToList(),
                Filas = worksheet.RowsUsed()
                    .Skip(1) // Saltar header
                    .Take(20) // Primeras 20 filas
                    .Select(row => row.CellsUsed().Select(c => c.Value.ToString()).ToList())
                    .ToList(),
                TamanoArchivo = $"{excelBytes.Length / 1024} KB",
                FechaGeneracion = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            };

            return Json(preview);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar previsualización del Excel");
            return Json(new { error = "Error al generar previsualización: " + ex.Message });
        }
    }

    // GET: Consultores/EnviarExcel
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> EnviarExcel(int? celulaId)
    {
        ViewBag.CelulaId = celulaId;

        // Cargar célula si se especificó
        if (celulaId.HasValue && celulaId.Value > 0)
        {
            var celula = await _context.Celulas.FindAsync(celulaId.Value);
            ViewBag.CelulaNombre = celula?.Nombre;
        }

        return View();
    }

    // POST: Consultores/EnviarExcel
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> EnviarExcel(string destinatariosJson, string? destinatarioNombre, string asunto, string mensaje, int? celulaId, bool guardarArchivo = false)
    {
        try
        {
            // Parsear lista de destinatarios desde JSON
            List<string> destinatarios;
            try
            {
                destinatarios = System.Text.Json.JsonSerializer.Deserialize<List<string>>(destinatariosJson) ?? new List<string>();
            }
            catch
            {
                TempData["Error"] = "Error al procesar la lista de destinatarios";
                return RedirectToAction(nameof(EnviarExcel), new { celulaId });
            }

            // SEGURIDAD: Validar que haya al menos un destinatario
            if (!destinatarios.Any())
            {
                TempData["Error"] = "Debe proporcionar al menos un destinatario";
                return RedirectToAction(nameof(EnviarExcel), new { celulaId });
            }

            // SEGURIDAD: Validar formato de emails
            var emailRegex = new System.Text.RegularExpressions.Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            var emailsInvalidos = destinatarios.Where(e => !emailRegex.IsMatch(e)).ToList();
            if (emailsInvalidos.Any())
            {
                TempData["Error"] = $"Emails inválidos: {string.Join(", ", emailsInvalidos)}";
                return RedirectToAction(nameof(EnviarExcel), new { celulaId });
            }

            // Generar Excel
            byte[] excelBytes;
            string fileName;
            string tipoFiltro;

            if (celulaId.HasValue && celulaId.Value > 0)
            {
                excelBytes = await _excelService.ExportarConsultoresPorCelulaAsync(celulaId.Value);
                var celula = await _context.Celulas.FindAsync(celulaId.Value);
                fileName = $"Consultores_{celula?.Nombre}_{DateTime.Now:yyyyMMdd}.xlsx";
                tipoFiltro = "Celula";
            }
            else
            {
                excelBytes = await _excelService.ExportarConsultoresAsync();
                fileName = $"Consultores_Todos_{DateTime.Now:yyyyMMdd}.xlsx";
                tipoFiltro = "Todos";
            }

            // Contar registros en el Excel
            var cantidadRegistros = celulaId.HasValue 
                ? await _context.Consultores.CountAsync(c => c.CelulaId == celulaId.Value && !c.Eliminado)
                : await _context.Consultores.CountAsync(c => !c.Eliminado);

            // Preparar cuerpo del correo
            var destinatariosTexto = destinatarios.Count == 1 
                ? destinatarios.First() 
                : $"{destinatarios.Count} destinatarios";

            var cuerpoHtml = $@"
<html>
<body style='font-family: Arial, sans-serif;'>
    <h2 style='color: #2E7D32;'>Exportación de Consultores - DataTeam</h2>
    <p>{mensaje}</p>
    <p><strong>Archivo adjunto:</strong> {fileName}</p>
    <p><strong>Cantidad de registros:</strong> {cantidadRegistros}</p>
    <p><strong>Fecha de generación:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
    <hr style='border: 1px solid #ddd;' />
    <p style='color: #666; font-size: 12px;'>
        Este correo fue enviado automáticamente por el sistema DataTeam.<br/>
        Usuario: {User.Identity?.Name ?? "Desconocido"}
    </p>
</body>
</html>";

            // Enviar correo a múltiples destinatarios
            bool envioExitoso = await _emailService.EnviarExcelPorCorreoMultipleAsync(
                destinatarios,
                asunto,
                cuerpoHtml,
                excelBytes,
                fileName
            );

            string? mensajeError = envioExitoso ? null : "Error al enviar el correo. Verifique la configuración.";

            // Registrar en historial
            // Registrar en historial
            var historial = new HistorialEnvioExcel
            {
                DestinatarioEmail = destinatarios.First(), // Primer email por compatibilidad
                DestinatarioNombre = destinatarioNombre,
                Asunto = asunto,
                Mensaje = mensaje,
                NombreArchivo = fileName,
                TamanoArchivo = excelBytes.Length,
                CantidadRegistros = cantidadRegistros,
                FechaEnvio = DateTime.Now,
                UsuarioEnvio = User.Identity?.Name ?? "Desconocido",
                EnvioExitoso = envioExitoso,
                MensajeError = mensajeError,
                TipoFiltro = tipoFiltro,
                CelulaIdFiltro = celulaId,
                ArchivoBytes = guardarArchivo ? excelBytes : null
            };

            // Guardar lista completa de destinatarios
            historial.SetDestinatarios(destinatarios);

            _context.HistorialEnviosExcel.Add(historial);
            await _context.SaveChangesAsync();

            if (envioExitoso)
            {
                var countText = destinatarios.Count == 1 
                    ? destinatarios.First() 
                    : $"{destinatarios.Count} destinatarios";
                TempData["Success"] = $"✅ Excel enviado exitosamente a {countText}";
            }
            else
            {
                TempData["Error"] = $"❌ Error al enviar correo: {mensajeError}. El registro se guardó en el historial.";
            }

            return RedirectToAction(nameof(HistorialEnvios));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar envío de Excel por correo");
            TempData["Error"] = "Error al procesar el envío del Excel";
            return RedirectToAction(nameof(EnviarExcel), new { celulaId });
        }
    }    // GET: Consultores/HistorialEnvios
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> HistorialEnvios(int? pageNumber)
    {
        const int pageSize = 20;
        var currentPage = pageNumber ?? 1;

        var historialQuery = _context.HistorialEnviosExcel
            .OrderByDescending(h => h.FechaEnvio)
            .AsQueryable();

        var totalRegistros = await historialQuery.CountAsync();
        var totalPaginas = (int)Math.Ceiling(totalRegistros / (double)pageSize);

        var historial = await historialQuery
            .Skip((currentPage - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        ViewBag.CurrentPage = currentPage;
        ViewBag.TotalPages = totalPaginas;
        ViewBag.TotalRegistros = totalRegistros;

        return View(historial);
    }

    // GET: Consultores/DescargarExcelHistorial/5
    [Authorize(Roles = AppRoles.SuperAdmin)]
    public async Task<IActionResult> DescargarExcelHistorial(int id)
    {
        var historial = await _context.HistorialEnviosExcel.FindAsync(id);

        if (historial == null)
        {
            TempData["Error"] = "Registro de historial no encontrado";
            return RedirectToAction(nameof(HistorialEnvios));
        }

        if (historial.ArchivoBytes == null || historial.ArchivoBytes.Length == 0)
        {
            TempData["Error"] = "El archivo no fue almacenado en el historial";
            return RedirectToAction(nameof(HistorialEnvios));
        }

        return File(historial.ArchivoBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", historial.NombreArchivo);
    }

    // GET: Consultores/AsignarCelula/5
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AsignarCelula(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var consultor = await _context.Consultores
            .Include(c => c.CelulasMiembro)
                .ThenInclude(cm => cm.Celula)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (consultor == null)
        {
            return NotFound();
        }

        // Obtener células activas que el consultor NO tiene asignadas aún
        var celulasAsignadas = consultor.CelulasMiembro.Select(cm => cm.CelulaId).ToList();
        var celulasDisponibles = await _context.Celulas
            .Where(c => c.Activa && !celulasAsignadas.Contains(c.Id))
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        ViewBag.Consultor = consultor;
        ViewBag.CelulasDisponibles = celulasDisponibles;

        return View();
    }

    // POST: Consultores/AsignarCelula
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> AsignarCelula(int consultorId, int celulaId, string rol)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(rol))
            {
                TempData["Error"] = "El rol es requerido";
                return RedirectToAction(nameof(AsignarCelula), new { id = consultorId });
            }

            var consultor = await _context.Consultores
                .Include(c => c.CelulasMiembro)
                .FirstOrDefaultAsync(c => c.Id == consultorId);

            if (consultor == null)
            {
                return NotFound();
            }

            var celula = await _context.Celulas.FindAsync(celulaId);
            if (celula == null || !celula.Activa)
            {
                TempData["Error"] = "Célula no encontrada o inactiva";
                return RedirectToAction(nameof(AsignarCelula), new { id = consultorId });
            }

            // Validar que no esté ya asignado a esta célula
            var yaAsignado = consultor.CelulasMiembro.Any(cm => cm.CelulaId == celulaId);
            if (yaAsignado)
            {
                TempData["Warning"] = "El consultor ya está asignado a esta célula";
                return RedirectToAction(nameof(Details), new { id = consultorId });
            }

            // Crear nueva asignación
            var nuevaAsignacion = new CelulaMiembro
            {
                ConsultorId = consultorId,
                CelulaId = celulaId,
                Rol = rol,
                FechaAsignacion = DateTime.Now
            };

            _context.CelulaMiembros.Add(nuevaAsignacion);
            await _context.SaveChangesAsync();

            // Registrar auditoría
            await _auditoriaService.RegistrarCambioAsync(
                "CelulaMiembro",
                nuevaAsignacion.Id,
                "Crear",
                User.Identity?.Name,
                null,
                nuevaAsignacion,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            TempData["Success"] = $"Consultor asignado exitosamente a la célula {celula.Nombre} como {rol}";
            return RedirectToAction(nameof(Details), new { id = consultorId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al asignar célula a consultor");
            TempData["Error"] = "Error al asignar la célula";
            return RedirectToAction(nameof(AsignarCelula), new { id = consultorId });
        }
    }

    // POST: Consultores/RemoverCelula
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> RemoverCelula(int consultorId, int celulaId)
    {
        try
        {
            var miembro = await _context.CelulaMiembros
                .Include(cm => cm.Celula)
                .FirstOrDefaultAsync(cm => cm.ConsultorId == consultorId && cm.CelulaId == celulaId);

            if (miembro == null)
            {
                TempData["Warning"] = "Asignación no encontrada";
                return RedirectToAction(nameof(Details), new { id = consultorId });
            }

            var estadoAnterior = new { miembro.ConsultorId, miembro.CelulaId, miembro.Rol };
            var nombreCelula = miembro.Celula?.Nombre ?? "Desconocida";

            _context.CelulaMiembros.Remove(miembro);
            await _context.SaveChangesAsync();

            // Registrar auditoría
            await _auditoriaService.RegistrarCambioAsync(
                "CelulaMiembro",
                miembro.Id,
                "Eliminar",
                User.Identity?.Name,
                estadoAnterior,
                null,
                HttpContext.Connection.RemoteIpAddress?.ToString()
            );

            TempData["Success"] = $"Consultor removido de la célula {nombreCelula}";
            return RedirectToAction(nameof(Details), new { id = consultorId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al remover célula de consultor");
            TempData["Error"] = "Error al remover la célula";
            return RedirectToAction(nameof(Details), new { id = consultorId });
        }
    }

    private bool ConsultorExists(int id)
    {
        return _context.Consultores.Any(e => e.Id == id);
    }
}

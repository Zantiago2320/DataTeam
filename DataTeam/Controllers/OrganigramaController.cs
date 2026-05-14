using DataTeam.Data;
using DataTeam.Models;
using DataTeam.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Controllers;

[Authorize]
public class OrganigramaController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrganigramaController> _logger;

    public OrganigramaController(ApplicationDbContext context, ILogger<OrganigramaController> logger)
    {
        _context = context;
        _logger = logger;
    }

    // GET: Organigrama
    public async Task<IActionResult> Index()
    {
        var viewModel = new OrganigramaViewModel();

        var celulas = await _context.Celulas
            .Include(c => c.Consultores)
            .Where(c => c.Activa)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        foreach (var celula in celulas)
        {
            var celulaVm = new CelulaConConsultores
            {
                Id = celula.Id,
                Nombre = celula.Nombre,
                Descripcion = celula.Descripcion,
                Color = celula.Color ?? "#3498db"
            };

            // Buscar líder si existe
            if (celula.LiderConsultorId.HasValue)
            {
                var lider = celula.Consultores.FirstOrDefault(c => c.Id == celula.LiderConsultorId.Value);
                celulaVm.NombreLider = lider?.Nombre;
            }

            // Agregar consultores activos
            celulaVm.Consultores = celula.Consultores
                .Where(c => c.Estado == EstadoConsultor.Activo)
                .Select(c => new ConsultorResumen
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Cargo = c.Cargo,
                    RutaFoto = c.RutaFoto,
                    Correo = c.Correo,
                    Rol = c.Rol,
                    Estado = c.Estado
                })
                .OrderBy(c => c.Nombre)
                .ToList();

            viewModel.Celulas.Add(celulaVm);
        }

        return View(viewModel);
    }
}

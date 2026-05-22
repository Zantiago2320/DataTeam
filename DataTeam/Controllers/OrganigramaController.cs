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
            .Include(c => c.CelulaLideres)
                .ThenInclude(cl => cl.Consultor)
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

            // Buscar líder principal si existe
            var liderPrincipal = celula.CelulaLideres
                .Where(cl => cl.EsLiderPrincipal)
                .Select(cl => cl.Consultor)
                .FirstOrDefault();

            if (liderPrincipal != null)
            {
                celulaVm.NombreLider = liderPrincipal.Nombre;
            }
            else
            {
                // Si no hay líder principal, tomar el primer líder asignado
                var primerLider = celula.CelulaLideres
                    .Select(cl => cl.Consultor)
                    .FirstOrDefault();
                celulaVm.NombreLider = primerLider?.Nombre;
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

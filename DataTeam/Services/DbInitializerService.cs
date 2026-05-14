using DataTeam.Data;
using DataTeam.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Services;

public class DbInitializerService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<DbInitializerService> _logger;

    public DbInitializerService(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        ILogger<DbInitializerService> logger)
    {
        _context = context;
        _userManager = userManager;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Crear usuario administrador si no existe
            await CreateAdminUserAsync();

            // Crear células de ejemplo
            await CreateCelulasAsync();

            // Crear consultores de ejemplo
            await CreateConsultoresAsync();

            _logger.LogInformation("Base de datos inicializada correctamente");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al inicializar la base de datos");
            throw;
        }
    }

    private async Task CreateAdminUserAsync()
    {
        const string adminEmail = "alex@apor.com";
        const string adminPassword = "1234";

        var adminUser = await _userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true // Confirmar email automáticamente para desarrollo
            };

            var result = await _userManager.CreateAsync(adminUser, adminPassword);

            if (result.Succeeded)
            {
                _logger.LogInformation($"Usuario administrador creado: {adminEmail}");
            }
            else
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogError($"Error al crear usuario administrador: {errors}");
            }
        }
        else
        {
            _logger.LogInformation($"Usuario administrador ya existe: {adminEmail}");
        }
    }

    private async Task CreateCelulasAsync()
    {
        if (await _context.Celulas.AnyAsync())
        {
            _logger.LogInformation("Ya existen células en la base de datos");
            return;
        }

        var celulas = new List<Celula>
        {
            new Celula
            {
                Nombre = "Backend",
                Descripcion = "Desarrollo de APIs y servicios backend",
                Color = "#3498db",
                Activa = true,
                FechaCreacion = DateTime.Now
            },
            new Celula
            {
                Nombre = "Frontend",
                Descripcion = "Desarrollo de interfaces de usuario",
                Color = "#e74c3c",
                Activa = true,
                FechaCreacion = DateTime.Now
            },
            new Celula
            {
                Nombre = "DevOps",
                Descripcion = "Infraestructura y despliegue continuo",
                Color = "#2ecc71",
                Activa = true,
                FechaCreacion = DateTime.Now
            },
            new Celula
            {
                Nombre = "QA",
                Descripcion = "Aseguramiento de calidad y testing",
                Color = "#f39c12",
                Activa = true,
                FechaCreacion = DateTime.Now
            },
            new Celula
            {
                Nombre = "Sin Asignar",
                Descripcion = "Consultores sin célula asignada",
                Color = "#95a5a6",
                Activa = true,
                FechaCreacion = DateTime.Now
            }
        };

        _context.Celulas.AddRange(celulas);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Se crearon {celulas.Count} células de ejemplo");
    }

    private async Task CreateConsultoresAsync()
    {
        if (await _context.Consultores.AnyAsync())
        {
            _logger.LogInformation("Ya existen consultores en la base de datos");
            return;
        }

        var celulas = await _context.Celulas.ToListAsync();
        if (!celulas.Any())
        {
            _logger.LogWarning("No hay células disponibles para asignar consultores");
            return;
        }

        var backendCelula = celulas.FirstOrDefault(c => c.Nombre == "Backend");
        var frontendCelula = celulas.FirstOrDefault(c => c.Nombre == "Frontend");
        var devOpsCelula = celulas.FirstOrDefault(c => c.Nombre == "DevOps");

        var consultores = new List<Consultor>
        {
            new Consultor
            {
                Cedula = "001-1234567-8",
                Nombre = "Juan Pérez García",
                Correo = "juan.perez@apor.com",
                Cargo = "Desarrollador Senior",
                RutaFoto = "/images/default-avatar.svg",
                FechaIngreso = DateTime.Now.AddYears(-3),
                FechaNacimiento = new DateTime(1990, 5, 15),
                CelulaId = backendCelula?.Id ?? celulas.First().Id,
                Rol = "Tech Lead",
                Capacidad = 100,
                Empresa = "APOR",
                Direccion = "Av. Principal 123",
                Barrio = "Naco",
                Celular = "809-555-0001",
                ContactoEmergencia = "María Pérez",
                CelularEmergencia = "809-555-0002",
                Estado = EstadoConsultor.Activo,
                FechaCreacion = DateTime.Now
            },
            new Consultor
            {
                Cedula = "001-2345678-9",
                Nombre = "Ana María Rodríguez",
                Correo = "ana.rodriguez@apor.com",
                Cargo = "Desarrolladora Frontend",
                RutaFoto = "/images/default-avatar.svg",
                FechaIngreso = DateTime.Now.AddYears(-2),
                FechaNacimiento = new DateTime(1992, 8, 20),
                CelulaId = frontendCelula?.Id ?? celulas.First().Id,
                Rol = "Developer",
                Capacidad = 100,
                Empresa = "APOR",
                Direccion = "Calle Secundaria 456",
                Barrio = "Piantini",
                Celular = "809-555-0003",
                ContactoEmergencia = "Carlos Rodríguez",
                CelularEmergencia = "809-555-0004",
                Estado = EstadoConsultor.Activo,
                FechaCreacion = DateTime.Now
            },
            new Consultor
            {
                Cedula = "001-3456789-0",
                Nombre = "Carlos Martínez López",
                Correo = "carlos.martinez@apor.com",
                Cargo = "Ingeniero DevOps",
                RutaFoto = "/images/default-avatar.svg",
                FechaIngreso = DateTime.Now.AddYears(-1),
                FechaNacimiento = new DateTime(1988, 3, 10),
                CelulaId = devOpsCelula?.Id ?? celulas.First().Id,
                Rol = "DevOps Engineer",
                Capacidad = 80,
                Empresa = "APOR",
                Direccion = "Av. Independencia 789",
                Barrio = "Gazcue",
                Celular = "809-555-0005",
                ContactoEmergencia = "Laura Martínez",
                CelularEmergencia = "809-555-0006",
                Estado = EstadoConsultor.Activo,
                FechaCreacion = DateTime.Now
            },
            new Consultor
            {
                Cedula = "001-4567890-1",
                Nombre = "María Fernández Santos",
                Correo = "maria.fernandez@apor.com",
                Cargo = "Desarrolladora Backend",
                RutaFoto = "/images/default-avatar.svg",
                FechaIngreso = DateTime.Now.AddMonths(-6),
                FechaNacimiento = new DateTime(1995, 11, 25),
                CelulaId = backendCelula?.Id ?? celulas.First().Id,
                Rol = "Junior Developer",
                Capacidad = 100,
                Empresa = "APOR",
                Direccion = "Calle Tercera 321",
                Barrio = "Los Cacicazgos",
                Celular = "809-555-0007",
                ContactoEmergencia = "Pedro Fernández",
                CelularEmergencia = "809-555-0008",
                Estado = EstadoConsultor.Activo,
                FechaCreacion = DateTime.Now
            },
            new Consultor
            {
                Cedula = "001-5678901-2",
                Nombre = "Pedro Gómez Reyes",
                Correo = "pedro.gomez@apor.com",
                Cargo = "Desarrollador Full Stack",
                RutaFoto = "/images/default-avatar.svg",
                FechaIngreso = DateTime.Now.AddYears(-5).AddMonths(-6),
                FechaNacimiento = new DateTime(1985, 12, 31),
                CelulaId = backendCelula?.Id ?? celulas.First().Id,
                Rol = "Senior Developer",
                Capacidad = 100,
                Empresa = "APOR",
                Direccion = "Av. Lope de Vega 654",
                Barrio = "Bella Vista",
                Celular = "809-555-0009",
                ContactoEmergencia = "Sofía Gómez",
                CelularEmergencia = "809-555-0010",
                Estado = EstadoConsultor.Retirado,
                FechaCreacion = DateTime.Now.AddYears(-5).AddMonths(-6)
            }
        };

        _context.Consultores.AddRange(consultores);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Se crearon {consultores.Count} consultores de ejemplo");
    }
}

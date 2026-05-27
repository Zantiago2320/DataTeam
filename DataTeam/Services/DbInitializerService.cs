using DataTeam.Data;
using DataTeam.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace DataTeam.Services;

public class DbInitializerService
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<DbInitializerService> _logger;
    private readonly IConfiguration _configuration;

    public DbInitializerService(
        ApplicationDbContext context,
        UserManager<IdentityUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<DbInitializerService> logger,
        IConfiguration configuration)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task InitializeAsync()
    {
        try
        {
            // Crear roles del sistema
            await CreateRolesAsync();

            // Crear usuarios con diferentes roles
            await CreateUsersAsync();

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

    private async Task CreateRolesAsync()
    {
        foreach (var roleName in AppRoles.AllRoles)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var result = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (result.Succeeded)
                {
                    _logger.LogInformation($"Rol creado: {roleName}");
                }
                else
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogError($"Error al crear rol {roleName}: {errors}");
                }
            }
        }
    }

    private async Task CreateUsersAsync()
    {
        // ⚠️ CREDENCIALES DE PRUEBA - NO USAR EN PRODUCCIÓN
        var testPassword = "1234";

        _logger.LogWarning("🔐 INICIALIZANDO USUARIOS DE PRUEBA...");
        _logger.LogWarning("⚠️ USUARIO: alexander");
        _logger.LogWarning("⚠️ CONTRASEÑA: {Password}", testPassword);
        _logger.LogWarning("⚠️ ESTO ES SOLO PARA DESARROLLO/PRUEBAS");

        // Usuario SuperAdmin: alexander con contraseña 1234
        var adminCreated = await CreateUserWithRoleAsync(
            "alexander@apor.com",
            testPassword,
            AppRoles.SuperAdmin,
            "Alexander"
        );

        if (adminCreated != null)
        {
            _logger.LogWarning("✅ USUARIO SUPERADMIN CREADO:");
            _logger.LogWarning("   📧 Email: alexander@apor.com");
            _logger.LogWarning("   👤 Nombre de usuario: alexander");
            _logger.LogWarning("   🔑 Contraseña: {Password}", testPassword);
            _logger.LogWarning("⚠️ RECUERDE: Esta es una configuración de prueba NO segura");
        }
    }

    private string GenerateSecurePassword()
    {
        // Verificar si hay contraseña configurada en variables de entorno (producción)
        var configPassword = _configuration["DefaultAdminPassword"];
        if (!string.IsNullOrWhiteSpace(configPassword))
        {
            return configPassword;
        }

        // Generar contraseña aleatoria segura para desarrollo
        const string upperChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowerChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string specialChars = "!@#$%^&*";

        var password = new StringBuilder();

        // Garantizar al menos un carácter de cada tipo (requisitos de Identity)
        password.Append(GetRandomChar(upperChars));
        password.Append(GetRandomChar(lowerChars));
        password.Append(GetRandomChar(digitChars));
        password.Append(GetRandomChar(specialChars));

        // Rellenar hasta 16 caracteres con caracteres aleatorios
        var allChars = upperChars + lowerChars + digitChars + specialChars;
        for (int i = 4; i < 16; i++)
        {
            password.Append(GetRandomChar(allChars));
        }

        // Mezclar caracteres para evitar patrones predecibles
        return new string(password.ToString().OrderBy(_ => RandomNumberGenerator.GetInt32(0, int.MaxValue)).ToArray());
    }

    private static char GetRandomChar(string chars)
    {
        var index = RandomNumberGenerator.GetInt32(0, chars.Length);
        return chars[index];
    }

    private async Task<string?> CreateUserWithRoleAsync(string email, string password, string role, string displayName)
    {
        var user = await _userManager.FindByEmailAsync(email);

        if (user == null)
        {
            // Extraer username del email (parte antes del @) o usar displayName
            var username = displayName.ToLowerInvariant().Replace(" ", "");

            user = new IdentityUser
            {
                UserName = username, // Usar nombre corto en lugar del email
                Email = email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, role);
                _logger.LogInformation("Usuario creado: {Username} ({Email}) con rol {Role}", username, email, role);
                return password; // Retornar contraseña solo para logging inicial
            }
            else
            {
                _logger.LogError("Error al crear usuario {Email}: {Errors}", email, string.Join(", ", result.Errors.Select(e => e.Description)));
            }
        }
        else
        {
            // Asegurar que el usuario tenga el rol correcto
            if (!await _userManager.IsInRoleAsync(user, role))
            {
                await _userManager.AddToRoleAsync(user, role);
                _logger.LogInformation("Rol {Role} asignado a usuario existente: {Email}", role, email);
            }
        }

        return null; // Usuario ya existía
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
            new Celula { Nombre = "Enterprise Team", Descripcion = "Equipo empresarial de desarrollo", Color = "#1E3A8A", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Nova", Descripcion = "Equipo de innovación", Color = "#10B981", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Bon Voyage", Descripcion = "Equipo de soluciones de viaje", Color = "#F59E0B", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "MindShift", Descripcion = "Equipo de transformación digital", Color = "#8B5CF6", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Wakanda", Descripcion = "Equipo de desarrollo avanzado", Color = "#EF4444", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "DEVSECOPS", Descripcion = "Equipo de seguridad y operaciones", Color = "#6366F1", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "DevSecOps", Descripcion = "Equipo de seguridad (variante)", Color = "#6366F1", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Data Stargazers", Descripcion = "Equipo de datos y analytics", Color = "#EC4899", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Maya", Descripcion = "Equipo de plataformas", Color = "#14B8A6", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Aurora", Descripcion = "Equipo de aplicaciones", Color = "#F97316", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Polaris Software Team", Descripcion = "Equipo de software", Color = "#3B82F6", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Seguridad", Descripcion = "Equipo de seguridad", Color = "#DC2626", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Administrativo", Descripcion = "Equipo administrativo", Color = "#64748B", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Transversal Calidad", Descripcion = "Equipo transversal de calidad", Color = "#A855F7", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Direccion Desarrollo", Descripcion = "Dirección de desarrollo", Color = "#0EA5E9", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Facturador", Descripcion = "Equipo de facturación", Color = "#22C55E", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Bon voyage", Descripcion = "Soluciones de viaje (variante)", Color = "#F59E0B", Activa = true, FechaCreacion = DateTime.Now },
            new Celula { Nombre = "Sin Asignar", Descripcion = "Sin célula asignada", Color = "#95a5a6", Activa = true, FechaCreacion = DateTime.Now }
        };

        _context.Celulas.AddRange(celulas);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Se crearon {celulas.Count} células");
    }

    private async Task CreateConsultoresAsync()
    {
        if (await _context.Consultores.AnyAsync())
        {
            _logger.LogInformation("Ya existen consultores en la base de datos");
            return;
        }

        var celulas = await _context.Celulas.ToDictionaryAsync(c => c.Nombre, c => c.Id);
        if (!celulas.Any())
        {
            _logger.LogWarning("No hay células disponibles para asignar consultores");
            return;
        }

        var sinAsignarId = celulas.GetValueOrDefault("Sin Asignar", celulas.First().Value);

        // Helper para buscar célula (maneja variantes)
        int GetCelulaId(string nombreCelula)
        {
            if (string.IsNullOrWhiteSpace(nombreCelula)) return sinAsignarId;

            // Búsqueda exacta
            if (celulas.TryGetValue(nombreCelula, out var id)) return id;

            // Búsqueda normalizada
            var normalizado = nombreCelula.Trim();
            var match = celulas.FirstOrDefault(c => 
                c.Key.Equals(normalizado, StringComparison.OrdinalIgnoreCase));

            return match.Value != 0 ? match.Value : sinAsignarId;
        }

        DateTime ParseFecha(string fecha)
        {
            if (string.IsNullOrWhiteSpace(fecha)) return DateTime.Now.AddYears(-30);

            try
            {
                var partes = fecha.Split('/');
                if (partes.Length == 3)
                {
                    return new DateTime(int.Parse(partes[2]), int.Parse(partes[1]), int.Parse(partes[0]));
                }
            }
            catch { }

            return DateTime.Now.AddYears(-30);
        }

        var consultores = new List<Consultor>
        {
            new Consultor { Cedula = "1023928928", Nombre = "Yuri Andrea Espinoza Serrato", Correo = "yespinoza@aportesenlinea.com", Cargo = "ANALISTA III INGENIERO DE DESARROLLO SENIOR", Rol = "Ingeniero/LT", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "AEL", FechaIngreso = ParseFecha("02/05/2023"), FechaNacimiento = ParseFecha("20/10/2023"), Direccion = "Calle 150A 96A 71 Torre 2 Apto 301", Barrio = "Suba - La Campiña", Celular = "3046546804", ContactoEmergencia = "Oscar Pulido", CelularEmergencia = "3505654004", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1018471934", Nombre = "Yenny Liliana Sánchez Alfonso", Correo = "ysanchez@aportesenlinea.com", Cargo = "ANALISTA III PRODUCT OWNER TECNICO", Rol = "PO Técnico", CelulaId = GetCelulaId("Nova"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("15/04/2025"), FechaNacimiento = ParseFecha("04/11/1994"), Direccion = "Dg 77b #116-51", Barrio = "Gran granada", Celular = "3209010380", ContactoEmergencia = "Yeraldy Sánchez", CelularEmergencia = "3235843491", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1010093635", Nombre = "Sergio Alejandro Genoy Cepeda", Correo = "sgenoy@aportesenlinea.com", Cargo = "ANALISTA II ADMINISTRATIVO DE DESARROLLO", Rol = "Analista", CelulaId = GetCelulaId("Administrativo"), Empresa = "AEL", FechaIngreso = ParseFecha("10/11/2025"), FechaNacimiento = ParseFecha("09/05/2000"), Direccion = "Calle 76 #28A - 41", Barrio = "La Aurora Norte", Celular = "3163536872", ContactoEmergencia = "Beatriz Merchan", CelularEmergencia = "3163520351", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1030635496", Nombre = "Amy Johanna Leal Camacho", Correo = "aleal@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "SQA", FechaIngreso = ParseFecha("08/11/2022"), FechaNacimiento = ParseFecha("20/01/1994"), Direccion = "Carrera 95A N° 26-38 Sur", Barrio = "Tierra buena - Kennedy", Celular = "3183105901", ContactoEmergencia = "Luis Alberto Bernal Pico", CelularEmergencia = "3212043828", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80174594", Nombre = "Wilmar Fernando Ramirez Esquivel", Correo = "wramirez@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("MindShift"), Empresa = "SQA", FechaIngreso = ParseFecha("25/09/2023"), FechaNacimiento = ParseFecha("05/03/1983"), Direccion = "Carrera 116 b No 72 f 70", Barrio = "Gran Granada", Celular = "3112074908", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1140870751", Nombre = "Wilmar Andres Mendoza Polo", Correo = "wmendoza@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero/LT", CelulaId = GetCelulaId("Wakanda"), Empresa = "AEL", FechaIngreso = ParseFecha("21/06/2021"), FechaNacimiento = ParseFecha("25/08/2023"), Direccion = "Carrera 45 #70-116 Apto 1", Barrio = "Boston", Celular = "3057244867", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1085297237", Nombre = "Viviana Andrea López Rodriguez", Correo = "vlopez@aportesenlinea.com", Cargo = "ANALISTA III PRODUCT OWNER TÉCNICO", Rol = "PO Técnico", CelulaId = GetCelulaId("MindShift"), Empresa = "AEL", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now, FechaIngreso = DateTime.Now.AddYears(-1), FechaNacimiento = ParseFecha("01/01/1990") },
            new Consultor { Cedula = "779729-ET", Nombre = "Cecilio Rafael de la Trinidad Maraima Nava", Correo = "ctrinidad@aportesenlinea.com", Cargo = "ANALISTA III SCRUM MASTER", Rol = "Agil Coach", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("17/08/2023"), FechaNacimiento = ParseFecha("03/05/1974"), Direccion = "CR35A, #77SUR-71. Torre 1, Apto.415. Fuente Clara", Barrio = "Lomas de San José", Celular = "3234894092", ContactoEmergencia = "Milagros Guerra", CelularEmergencia = "3142926978", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "779729-WK", Nombre = "Cecilio Rafael de la Trinidad Maraima Nava", Correo = "ctrinidad@aportesenlinea.com", Cargo = "ANALISTA III SCRUM MASTER", Rol = "Agil Coach", CelulaId = GetCelulaId("Wakanda"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("17/08/2023"), FechaNacimiento = ParseFecha("03/05/1974"), Direccion = "CR35A, #77SUR-71. Torre 1, Apto.415. Fuente Clara", Barrio = "Lomas de San José", Celular = "3234894092", ContactoEmergencia = "Milagros Guerra", CelularEmergencia = "3142926978", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1033720903", Nombre = "Sneider Giovanny Rios Arboleda", Correo = "srios@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Nova"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("28/06/2024"), FechaNacimiento = ParseFecha("27/03/1990"), Direccion = "Carrera 87a # 128b - 80", Barrio = "ciudad Hunza", Celular = "3228749734", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1065817149", Nombre = "Brayan Alberto Badillo Diaz", Correo = "bbadillo@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("04/09/2024"), FechaNacimiento = ParseFecha("07/08/1995"), Direccion = "Manzana E casa 39 urbanización casa carmelo", Barrio = "Urbanización casa carmelo etapa 1", Celular = "3158596128", ContactoEmergencia = "Yuri Cárdenas Afanador", CelularEmergencia = "3127668099", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80075269", Nombre = "Saulo Ferney Barbosa Pulido", Correo = "sbarbosa@aportesenlinea.com", Cargo = "COORDINADOR ARQUITECTO DE SOFTWARE", Rol = "Arquitecto", CelulaId = GetCelulaId("MindShift"), Empresa = "AEL", FechaIngreso = ParseFecha("01/11/2019"), FechaNacimiento = ParseFecha("01/01/1980"), Direccion = "Calle 92 # 11 - 32 Apt 304 Edificio Cervantes IV", Barrio = "Chicó Norte", Celular = "3102359084", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1000874819", Nombre = "Santiago Eusse Gil", Correo = "seusse@aportesenlinea.com", Cargo = "ANALISTA II DEVSECOPS SEMI SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("DEVSECOPS"), Empresa = "AEL", FechaIngreso = ParseFecha("10/04/2023"), FechaNacimiento = ParseFecha("03/01/2003"), Direccion = "Carrera 47a #44-46 apto 301. Edificio \"Genus\"", Barrio = "Manchester", Celular = "3197994175", ContactoEmergencia = "Adriana Gil Arango", CelularEmergencia = "3015971650", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "52964246", Nombre = "Sandra Rocio Tovar Avendano", Correo = "stovar@aportesenlinea.com", Cargo = "DIRECTOR SERVICIOS ESPECIALIZADOS", Rol = "Sponsor", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "AEL", FechaIngreso = ParseFecha("11/08/2008"), FechaNacimiento = ParseFecha("01/01/1975"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1082864596", Nombre = "Rubiden Stiven Diaz Granados", Correo = "rdiaz@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero/LT", CelulaId = GetCelulaId("Seguridad"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("10/02/2022"), FechaNacimiento = ParseFecha("05/02/1987"), Direccion = "Calle 1sur #50g-37 Apto 201", Barrio = "Cristo rey", Celular = "3003246924", ContactoEmergencia = "Sebastián Mora", CelularEmergencia = "3004717897", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80920988", Nombre = "Alex Fernando Caro López", Correo = "acaro@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "ingeniero", CelulaId = GetCelulaId("Nova"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("21/03/2025"), FechaNacimiento = ParseFecha("16/10/1985"), Direccion = "Calle 26 # 50 - 43", Barrio = "Alejandría", Celular = "3212704388", ContactoEmergencia = "Camila Briñez Velásquez", CelularEmergencia = "3212464673", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80127568", Nombre = "Robert Ricardo Ramirez Rojas", Correo = "rramirez@aportesenlinea.com", Cargo = "GERENTE DE TRANSFORMACIÓN DIGITAL", Rol = "Sponsor", CelulaId = GetCelulaId("DEVSECOPS"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2020"), FechaNacimiento = ParseFecha("01/01/1975"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1012317652", Nombre = "Diego Moreno Arce", Correo = "damoreno@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Aurora"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("02/01/2025"), FechaNacimiento = ParseFecha("14/02/1986"), Direccion = "Calle 78 Bis Sur 94 - 27 Torre 24 Apto 4091", Barrio = "Bosa Parques de Bogotá", Celular = "3123810394", ContactoEmergencia = "Diana Restrepo Casas", CelularEmergencia = "3204060334", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1083023296", Nombre = "Ricardo Jose Alcala", Correo = "ralcala@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Data Stargazers"), Empresa = "AEL", FechaIngreso = ParseFecha("26/04/2021"), FechaNacimiento = ParseFecha("16/03/2023"), Direccion = "Transversal 9C #34-161", Barrio = "Urbanización Alejandrina", Celular = "3046453177", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1023026324", Nombre = "Paula Andrea Rojas Suarez", Correo = "gyc09705@aportesenlinea.com", Cargo = "ANALISTA II PRODUCT OWNER TÉCNICO", Rol = "PO Técnico", CelulaId = sinAsignarId, Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2023"), FechaNacimiento = ParseFecha("01/01/1995"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1007165946", Nombre = "Orlando Delgado Pinzon", Correo = "odelgado@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Wakanda"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("16/01/2025"), FechaNacimiento = ParseFecha("26/10/1999"), Direccion = "Cll126 # 9 - 43", Barrio = "Conjunto Monte bonito Torre E - 502", Celular = "3227045897", ContactoEmergencia = "Luz Stella Pinzon Sierra", CelularEmergencia = "3112687034", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1023918569", Nombre = "Nestor Eduardo Sanchez Torres", Correo = "nsanchez@aportesenlinea.com", Cargo = "ANALISTA III INGENIERO DE DESARROLLO SENIOR", Rol = "Ingeniero/LT", CelulaId = GetCelulaId("MindShift"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("15/07/2021"), FechaNacimiento = ParseFecha("16/08/1992"), Direccion = "CRA 103A# 131A -66", Barrio = "LAGO DE SUBA", Celular = "3005487391", ContactoEmergencia = "Laura Andrea Muñoz", CelularEmergencia = "3022883237", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1012462196", Nombre = "Nelson Eduardo Pabon Pabon", Correo = "npabon@aportesenlinea.com", Cargo = "ANALISTA I MONITOREO DE OPERACIÓN", Rol = "StakeHolder Infraestructura", CelulaId = sinAsignarId, Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2023"), FechaNacimiento = ParseFecha("01/01/1990"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "52765886", Nombre = "Monica Astrid Gutierrez Gutierrez", Correo = "mgutierrez@aportesenlinea.com", Cargo = "ANALISTA III PRODUCT OWNER TÉCNICO", Rol = "PO Técnico", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1033776027", Nombre = "Miguel Angel Martinez Mendoza", Correo = "mmartinez@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Transversal Calidad"), Empresa = "AEL", FechaIngreso = ParseFecha("01/11/2019"), FechaNacimiento = ParseFecha("01/01/1990"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1234097180", Nombre = "Michael Andrés Martínez Quevedo", Correo = "amartinez@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Maya"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("09/10/2024"), FechaNacimiento = ParseFecha("25/10/1999"), Direccion = "Calle 59C #13D16", Barrio = "Nuevo Milenio", Celular = "3002723160", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1082843183", Nombre = "Karol Briyette Rubiano Rojas", Correo = "ebarros@aportesenlinea.com", Cargo = "DIRECTOR AGILISMO Y GESTION DEL CONOCIMIENTO", Rol = "PO Funcional", CelulaId = GetCelulaId("Aurora"), Empresa = "AEL", FechaIngreso = ParseFecha("01/11/2019"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1040327881", Nombre = "Manuel Alejandro Bastidas Ospina", Correo = "mbastidas@aportesenlinea.com", Cargo = "ANALISTA I DEVSECOPS", Rol = "Automatizador", CelulaId = GetCelulaId("DevSecOps"), Empresa = "SQA", FechaIngreso = ParseFecha("28/06/2023"), FechaNacimiento = ParseFecha("23/09/1998"), Direccion = "Carrera 86 # 47DD - 44", Barrio = "Santa Lucía", Celular = "3104299332", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1003557777", Nombre = "Manuel Alberto Torres Vergara", Correo = "mtorres@aportesenlinea.com", Cargo = "ANALISTA I DEVSECOPS", Rol = "Ingeniero", CelulaId = GetCelulaId("DEVSECOPS"), Empresa = "AEL", FechaIngreso = ParseFecha("18/07/2022"), FechaNacimiento = ParseFecha("09/04/2023"), Direccion = "Finca Santa Cecilia", Barrio = "Vereda San Juan", Celular = "3006064535", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "43186711", Nombre = "Lyda Maria Echeverri Garces", Correo = "lecheverri@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "AEL", FechaIngreso = ParseFecha("21/08/2024"), FechaNacimiento = ParseFecha("06/06/1984"), Direccion = "Carrera 50A 24-51 interior 117 Conjunto residencial Suramérica Park", Barrio = "Yarumito", Celular = "3007822349", ContactoEmergencia = "Ricardo Isaza", CelularEmergencia = "3207273709", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1014199932", Nombre = "David Guillermo Peña", Correo = "dpena@aportesenlinea.com", Cargo = "ANALISTA III PRODUCT OWNER FUNCIONAL OPERACIONES", Rol = "PO Funcional", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1022353132", Nombre = "Leidy Marcela Franco Morales", Correo = "lfranco@aportesenlinea.com", Cargo = "ANALISTA III INNOVACION Y PRODUCTIVIDAD", Rol = "PO Funcional", CelulaId = GetCelulaId("DEVSECOPS"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2023"), FechaNacimiento = ParseFecha("01/01/1990"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1073324641", Nombre = "Jorge Luis Vera Vera", Correo = "jvera@aportesenlinea.com", Cargo = "COORDINADOR LÍDER DE DESARROLLO", Rol = "Coordinador", CelulaId = GetCelulaId("Direccion Desarrollo"), Empresa = "AEL", FechaIngreso = ParseFecha("10/11/2019"), FechaNacimiento = ParseFecha("02/12/2025"), Direccion = "Calle 23 # 68 - 59 / Conjunto Adarves del salitre / interior 14 apto 101", Barrio = "Salitre", Celular = "3503379214", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1053804044", Nombre = "Leandro Muñoz Murcia", Correo = "lmunoz@aportesenlinea.com", Cargo = "ANALISTA I DEVSECOPS", Rol = "Automatizador", CelulaId = GetCelulaId("DevSecOps"), Empresa = "SQA", FechaIngreso = ParseFecha("12/09/2024"), FechaNacimiento = ParseFecha("25/07/2006"), Direccion = "Carrera 7c nro 59 42", Barrio = "Barrio La cumbre", Celular = "3226092458", ContactoEmergencia = "Hilda Maria Murcia Ruiz", CelularEmergencia = "3176262999", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1010215539", Nombre = "Laura Andrea Avila", Correo = "lavila@aportesenlinea.com", Cargo = "DIRECTOR DE INNOVACIÓN", Rol = "Sponsor", CelulaId = GetCelulaId("Maya"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2020"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80088963", Nombre = "Diego Guillermo Montenegro Revelo", Correo = "dmontenegro@aportesenlinea.com", Cargo = "GERENTE DE SOLUCIONES CLIENTES CORPORATIVOS", Rol = "Sponsor", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "AEL", FechaIngreso = ParseFecha("15/12/2014"), FechaNacimiento = ParseFecha("01/01/1975"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1073710856", Nombre = "Karen Lorena Herrera Infante", Correo = "kherrera@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("MindShift"), Empresa = "SQA", FechaIngreso = ParseFecha("30/11/2021"), FechaNacimiento = ParseFecha("22/04/1997"), Direccion = "cll 6 sur 18 H 12", Celular = "3506618106", ContactoEmergencia = "Dayana Lizeth Herrera Infante", CelularEmergencia = "3224622940", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1016098923", Nombre = "Julián Enrique Muñoz Castro", Correo = "jmunoz@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Maya"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("23/01/2025"), FechaNacimiento = ParseFecha("18/12/1997"), Direccion = "Calle 25F #80C - 47", Barrio = "Modelia", Celular = "3204995437", ContactoEmergencia = "Diego Alejandro Muñoz Castro", CelularEmergencia = "3134674687", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1192794693", Nombre = "Juan Diego Quintero", Correo = "jquintero@aportesenlinea.com", Cargo = "ANALISTA III INGENIERO DE DESARROLLO SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Data Stargazers"), Empresa = "AEL", FechaIngreso = ParseFecha("01/04/2025"), FechaNacimiento = ParseFecha("05/09/2001"), Direccion = "Carrera 17 #4-33. Edificio La Quinta apto 811", Barrio = "Barrio La Francia", Celular = "3025285487", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1030697393", Nombre = "Juan David Ibarra Ochoa", Correo = "jibarra@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "AEL", FechaIngreso = ParseFecha("08/11/2021"), FechaNacimiento = ParseFecha("25/06/1999"), Direccion = "Cra 71 B bis # 5B-06", Barrio = "Nueva Marsella", Celular = "3102277535", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1016082141", Nombre = "Juan Camilo Sánchez", Correo = "jcsanchez@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("MindShift"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("14/08/2024"), FechaNacimiento = ParseFecha("04/12/1995"), Direccion = "Cra 58#119a 98 apto 803", Barrio = "Lagos de Córdoba", Celular = "3176578000", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1026288243", Nombre = "Juan Camilo Hurtado Orjuela", Correo = "jchurtado@aportesenlinea.com", Cargo = "ANALISTA III PRODUCT OWNER FUNCIONAL SERVICIO", Rol = "PO Funcional", CelulaId = GetCelulaId("Wakanda"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1990"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1000538611", Nombre = "José Sandro Serna Vargas", Correo = "Sandro.serna@sqasa.co", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "SQA", FechaIngreso = ParseFecha("05/05/2025"), FechaNacimiento = ParseFecha("13/12/2000"), Direccion = "Cr. 50B # 110-29 int ( 302)", Barrio = "Andalucía la Francia", Celular = "3122620146", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1031176153", Nombre = "José Esteban Colorado Montenegro", Correo = "jcolorado@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("04/02/2025"), FechaNacimiento = ParseFecha("28/07/1998"), Direccion = "Cl 71 a # 26-23 sur", Barrio = "Tunal", Celular = "3209874292", ContactoEmergencia = "Daniela", CelularEmergencia = "3132541794", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1105786797", Nombre = "Leidy Johana Ruiz Gutierrez", Correo = "gyc06197@aportesenlinea.com", Cargo = "ANALISTA II ASEGURAMIENTO DE CALIDAD SEMI SENIOR", Rol = "QA", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "AEL", FechaIngreso = ParseFecha("03/12/2018"), FechaNacimiento = ParseFecha("06/12/2025"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1018491399", Nombre = "Jorge Luis Garzon Mejia", Correo = "jgarzon@aportesenlinea.com", Cargo = "ANALISTA I INGENIERO DE DESARROLLO TRAINEE", Rol = "Ingeniero", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "AEL", FechaIngreso = ParseFecha("10/02/2025"), FechaNacimiento = ParseFecha("20/11/1996"), Direccion = "Calle 138 #49-65", Barrio = "Spring", Celular = "3028307374", ContactoEmergencia = "Sirley Murcia", CelularEmergencia = "3219674550", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1110484616", Nombre = "Jorge Eduardo Rojas Moreno", Correo = "jerojas@aportesenlinea.com", Cargo = "ANALISTA II DEVSECOPS SEMI SENIOR", Rol = "QA", CelulaId = GetCelulaId("Transversal Calidad"), Empresa = "AEL", FechaIngreso = ParseFecha("01/11/2019"), FechaNacimiento = ParseFecha("12/04/2025"), Direccion = "Carrera 123 #131-66 Bl 49 Apt 303, conjunto nueva Tibabuyes sector B, barrio Villamaría, localidad de Suba", Barrio = "Villamaria", Celular = "3163813397", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1233907650", Nombre = "Jordan Andres Garcia Rodriguez", Correo = "jagarcia@aportesenlinea.com", Cargo = "ANALISTA III INGENIERO DE DESARROLLO SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("MindShift"), Empresa = "AEL", FechaIngreso = ParseFecha("19/07/2021"), FechaNacimiento = ParseFecha("04/06/2023"), Direccion = "Carrera 98b #131c - 41", Barrio = "Aures - La chucua", Celular = "3204153939", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1053773898", Nombre = "Jhon James Grisales Parra", Correo = "jgrisales@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("DevSecOps"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("26/12/2024"), FechaNacimiento = ParseFecha("22/03/1986"), Direccion = "CRA 29 # 40A-134, Molivento 2, Torre 1, Apto 407", Barrio = "VILLAVENTO", Celular = "3173669738", ContactoEmergencia = "DIEGO GRISALES", CelularEmergencia = "3148114563", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1023009270", Nombre = "Jesús David Ortiz Galeón", Correo = "jortiz@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "AEL", FechaIngreso = ParseFecha("16/11/2021"), FechaNacimiento = ParseFecha("16/12/2023"), Direccion = "carrera 5 # 76 c 11 su", Barrio = "betania", Celular = "3115878415", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "72002153", Nombre = "Gustavo Adolfo Hernández Cabrera", Correo = "ghernandez@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "ingeniero", CelulaId = GetCelulaId("Maya"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("13/06/2025"), FechaNacimiento = ParseFecha("12/02/1978"), Direccion = "Diagonal 77B #119A-68 Torre Salamanca Apto 807", Barrio = "Bogotá", Celular = "3002876514", ContactoEmergencia = "Fabiola De La Hoz", CelularEmergencia = "3002421740", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1030608602", Nombre = "Jeisson Duban Juanias Villarraga", Correo = "jjuanias@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Nova"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("13/01/2025"), FechaNacimiento = ParseFecha("11/11/1991"), Direccion = "KR 2 Este # 5-20 Torre 8 Apto 501, conjunto Arboled", Barrio = "Casa blanca", Celular = "3203296290", ContactoEmergencia = "Jorge Juanias", CelularEmergencia = "3124253739", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1016091477", Nombre = "Jeferson Stiben Pedraza Echeverry", Correo = "jpedraza@aportesenlinea.com", Cargo = "ANALISTA I DEVSECOPS", Rol = "Automatizador", CelulaId = GetCelulaId("Maya"), Empresa = "SQA", FechaIngreso = ParseFecha("26/11/2024"), FechaNacimiento = ParseFecha("14/01/1997"), Direccion = "CLL 64F #105D-57", Barrio = "Muelle", Celular = "3504515805", ContactoEmergencia = "Leidy Paola Mahecha", CelularEmergencia = "3014130251", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1235245612", Nombre = "Javier Enrique Macías Díaz", Correo = "jmacias@aportesenlinea.com", Cargo = "ESPECIALISTA ARQUITECTO DE SOFTWARE SENIOR", Rol = "Arquitecto", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "AEL", FechaIngreso = ParseFecha("03/01/2022"), FechaNacimiento = ParseFecha("08/01/2023"), Direccion = "Calle9, #9-80 Conj. Residencial Wayra Torre 2 Apto 202", Barrio = "Funza", Celular = "3209420175", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1214715394", Nombre = "Jean Carlo Hincapie Monsalve", Correo = "jhincapie@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("04/11/2022"), FechaNacimiento = ParseFecha("13/08/1992"), Direccion = "Transversal 34D sur # 30a-19", Barrio = "Manuel Uribe Angel", Celular = "3126118766", ContactoEmergencia = "Andrea Arroyave", CelularEmergencia = "3128797781", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1015428579", Nombre = "Ivonne Cabiativa Gonzalez", Correo = "icabiativa@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("01/03/2024"), FechaNacimiento = ParseFecha("14/01/1992"), Direccion = "Carrera 123 # 130 c -95 Bloque 2 Apto 103", Barrio = "VillaMaria", Celular = "3003479871", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1030569638", Nombre = "Ismael Ruiz Ovalle", Correo = "iruizo@aportesenlinea.com", Cargo = "ANALISTA III SOPORTE", Rol = "PO Funcional", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1990"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "52414874", Nombre = "Ingrid Milena Manrique Porras", Correo = "imanrique@aportesenlinea.com", Cargo = "GERENTE DE SERVICIOS", Rol = "Sponsor", CelulaId = GetCelulaId("MindShift"), Empresa = "AEL", FechaIngreso = ParseFecha("11/05/2020"), FechaNacimiento = ParseFecha("01/01/1975"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "79568718-ET", Nombre = "Hugo Bermudez Diaz", Correo = "hbermudez@aportesenlinea.com", Cargo = "COORDINADOR ARQUITECTO DE DATOS", Rol = "Arquitecto", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "AEL", FechaIngreso = ParseFecha("07/02/2022"), FechaNacimiento = ParseFecha("01/01/1980"), Direccion = "Calle 148 No. 94 A 60 Apto 710", Barrio = "La Campiña", Celular = "3013685229", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "79568718-WK", Nombre = "Hugo Bermudez Diaz", Correo = "hbermudez@aportesenlinea.com", Cargo = "COORDINADOR ARQUITECTO DE DATOS", Rol = "Arquitecto", CelulaId = GetCelulaId("Wakanda"), Empresa = "AEL", FechaIngreso = ParseFecha("07/02/2022"), FechaNacimiento = ParseFecha("01/01/1980"), Direccion = "Calle 148 No. 94 A 60 Apto 710", Barrio = "La Campiña", Celular = "3013685229", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1067946006", Nombre = "Francisco Javier Palencia", Correo = "fpalencia@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Nova"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("07/12/2023"), FechaNacimiento = ParseFecha("14/12/1995"), Direccion = "Mzna 5 Lote 7 , Sur Por la gloria", Barrio = "Sueño Real", Celular = "3044191133", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1072666410-PS", Nombre = "Esneider Gualtero Hernández", Correo = "egualtero@aportesenlinea.com", Cargo = "ANALISTA III SCRUM MASTER", Rol = "Scrum Master", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "AEL", FechaIngreso = ParseFecha("01/11/2019"), FechaNacimiento = ParseFecha("25/07/2023"), Direccion = "calle 59 # 13-30 Torre norte apto 1401", Barrio = "Chapinero Central", Celular = "3118003660", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1072666410-MY", Nombre = "Esneider Gualtero Hernández", Correo = "egualtero@aportesenlinea.com", Cargo = "ANALISTA III SCRUM MASTER", Rol = "Scrum Master", CelulaId = GetCelulaId("Maya"), Empresa = "AEL", FechaIngreso = ParseFecha("01/11/2019"), FechaNacimiento = ParseFecha("25/07/2023"), Direccion = "calle 59 # 13-30 Torre norte apto 1401", Barrio = "Chapinero Central", Celular = "3118003660", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1032416617", Nombre = "Javier Eduardo Becerra Bernal", Correo = "becerra.bernal.javier@gmail.com", Cargo = "ANALISTA III PRODUCT OWNER TECNICO", Rol = "PO Técnico", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("16/10/2025"), FechaNacimiento = ParseFecha("26/07/1988"), Direccion = "Carrera 69H # 63A-70", Barrio = "Bosque popular", Celular = "3168347191", ContactoEmergencia = "Daniel Becerra", CelularEmergencia = "3168347184", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1140876545", Nombre = "Jesus David Osorio Pimienta", Correo = "josorio@aportesenlinea.com", Cargo = "ANALISTA III INGENIERO DE DESARROLLO SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "AEL", FechaIngreso = ParseFecha("01/11/2019"), FechaNacimiento = ParseFecha("09/04/2023"), Direccion = "Calle 117 # 42B - 25 Conjunto turpial torre 8 AP 601", Barrio = "Alameda del rio", Celular = "3195262997", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1006661952", Nombre = "Edwar Estiven Burbano Cortes", Correo = "eburbano@aportesenlinea.com", Cargo = "ANALISTA I INGENIERO DE DESARROLLO", Rol = "Ingeniero", CelulaId = GetCelulaId("Direccion Desarrollo"), Empresa = "AEL", FechaIngreso = ParseFecha("11/09/2023"), FechaNacimiento = ParseFecha("28/07/2000"), Direccion = "Calle 26 Sur 8 06 CS 12", Barrio = "Barrio las gaviotas", Celular = "3134017963", ContactoEmergencia = "Luz Dary Cortes", CelularEmergencia = "3134017963", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1016051882", Nombre = "Linda Carolina Rodriguez Molina", Correo = "lrodriguezm@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Maya"), Empresa = "SQA", FechaIngreso = ParseFecha("08/01/2025"), FechaNacimiento = ParseFecha("09/01/1993"), Direccion = "Cra 80 #71a 15 sur", Barrio = "Bosa Naranjos", Celular = "3142728931", CelularEmergencia = "3125534374", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80127568-AU", Nombre = "Robert Ricardo Ramirez Rojas", Correo = "rramirez@aportesenlinea.com", Cargo = "GERENTE DE TRANSFORMACIÓN DIGITAL", Rol = "Sponsor", CelulaId = GetCelulaId("Aurora"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2020"), FechaNacimiento = ParseFecha("01/01/1975"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80088963-DS", Nombre = "Diego Guillermo Montenegro Revelo", Correo = "dmontenegro@aportesenlinea.com", Cargo = "GERENTE DE SOLUCIONES CLIENTES CORPORATIVOS", Rol = "Sponsor", CelulaId = GetCelulaId("Data Stargazers"), Empresa = "AEL", FechaIngreso = ParseFecha("15/12/2014"), FechaNacimiento = ParseFecha("01/01/1975"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "53006451-ET", Nombre = "Diana Milena Saavedra Ferrer", Correo = "dsaavedra@aportesenlinea.com", Cargo = "ANALISTA III GESTION DE PRODUCTO", Rol = "PO Técnico", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "53006451-WK", Nombre = "Diana Milena Saavedra Ferrer", Correo = "dsaavedra@aportesenlinea.com", Cargo = "ANALISTA III GESTION DE PRODUCTO", Rol = "PO Técnico", CelulaId = GetCelulaId("Wakanda"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1985"), Direccion = "calle 47#8 - 63p", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1041611184", Nombre = "David Mauricio Mejía Arias", Correo = "dmejia@aportesenlinea.com", Cargo = "ANALISTA III INGENIERO DEVSECOPS SENIOR", Rol = "ScrumM/Ingeniero/LT", CelulaId = GetCelulaId("DEVSECOPS"), Empresa = "AEL", FechaIngreso = ParseFecha("11/11/2021"), FechaNacimiento = ParseFecha("08/12/2024"), Direccion = "calle 7 # 11 - 21", Barrio = "Buenos Aires", Celular = "3016296563", ContactoEmergencia = "Maria Rocio Flores", CelularEmergencia = "3117919942", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1036934864", Nombre = "Milena Cardenas Alzate", Correo = "mcardenas@aportesenlinea.com", Cargo = "ANALISTA III SCRUM MASTER", Rol = "Scrum Master", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("17/08/2023"), FechaNacimiento = ParseFecha("10/05/1989"), Direccion = "CLL 71 # 58 102 apto 2021", Barrio = "ciudadela del parque", Celular = "3116086657", ContactoEmergencia = "Sebastián Mora", CelularEmergencia = "3013172821", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1012403476", Nombre = "David Fernando Delgado Guacaneme", Correo = "gyc03252@aportesenlinea.com", Cargo = "ANALISTA II ASEGURAMIENTO DE CALIDAD SEMI SENIOR", Rol = "QA", CelulaId = GetCelulaId("Wakanda"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1990"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1026294015", Nombre = "Harold Steven Lopez Rubio", Correo = "dordonez@aportesenlinea.com", Cargo = "ANALISTA III PRODUCT OWNER FUNCIONAL SERVICIO", Rol = "PO Funcional", CelulaId = GetCelulaId("MindShift"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1990"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1023900544", Nombre = "Daniel Fernando Parra", Correo = "dparra@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE CALIDAD Y AUTOMATIZACIÓN SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Transversal Calidad"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1990"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1032458577", Nombre = "Cristhian David Amezquita Castro", Correo = "camezquita@aportesenlinea.com", Cargo = "COORDINADOR QA Y DEVSECOPS", Rol = "Coordinador", CelulaId = GetCelulaId("DEVSECOPS"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2020"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1016042511", Nombre = "Cristhian Camilo Reyes Pardo", Correo = "creyes@aportesenlinea.com", Cargo = "ANALISTA III INGENIERO DE DESARROLLO SENIOR", Rol = "Ingeniero/LT", CelulaId = GetCelulaId("Data Stargazers"), Empresa = "AEL", FechaIngreso = ParseFecha("01/06/2022"), FechaNacimiento = ParseFecha("02/02/2023"), Direccion = "Carrera 98 #23g - 78", Barrio = "San Jose de Fontibón", Celular = "3156004981", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1073238911", Nombre = "Cristhian Camilo Perez Estrada", Correo = "ccperez@aportesenlinea.com", Cargo = "ANALISTA III INGENIERO DE DESARROLLO SENIOR", Rol = "Ingeniero/LT", CelulaId = GetCelulaId("Nova"), Empresa = "AEL", FechaIngreso = ParseFecha("21/11/2022"), FechaNacimiento = ParseFecha("06/05/2023"), Direccion = "cra 21a no 159 a 04 apto 102 edificio rincon del parque", Barrio = "Bogota", Celular = "3118610754", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1022333350", Nombre = "Jenny Alejandra Mora Hernandez", Correo = "jmora@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Nova"), Empresa = "SQA", FechaIngreso = ParseFecha("15/01/2025"), FechaNacimiento = ParseFecha("25/05/1987"), Direccion = "Carrera 96C # 16-61", Barrio = "Villemar Hayuelos", Celular = "314 203 90 21", ContactoEmergencia = "Rosario Mora", CelularEmergencia = "310 7594714", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80852689", Nombre = "Christian Oswaldo Jiménez Robayo", Correo = "cjimenez@aportesenlinea.com", Cargo = "ANALISTA III SCRUM MASTER", Rol = "Scrum Master", CelulaId = GetCelulaId("Nova"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("08/01/2025"), FechaNacimiento = ParseFecha("28/04/1985"), Direccion = "Supermanzana SM Res 4, Hacienda Tocancipá, Conjunto Portofino 8-503", Barrio = "/ Vereda el Verganzo", Celular = "313 470 82 81", ContactoEmergencia = "Tatiana Anaya", CelularEmergencia = "321 371 59 66", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1022355992", Nombre = "Cesar Augusto Pachon Porras", Correo = "cpachon@aportesenlinea.com", Cargo = "COORDINADOR DE DESARROLLO DE NEGOCIO", Rol = "PO Funcional", CelulaId = GetCelulaId("Data Stargazers"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1128417080", Nombre = "Cesar Adelmo Muñoz Henao", Correo = "cahenao@aportesenlinea.com", Cargo = "ANALISTA III PRODUCT OWNER FUNCIONAL SERVICIO", Rol = "PO Funcional", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1083019159", Nombre = "Uber Antonio Marin Arboleda", Correo = "umarin@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "AEL", FechaIngreso = ParseFecha("05/04/2021"), FechaNacimiento = ParseFecha("09/01/2023"), Direccion = "Carrera 81 N 28 A 19 - Barrio Nueva Mansion", Barrio = "Barrio Nueva Mansion", Celular = "3016426995", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1016064908", Nombre = "Carlos Andrés Quesada Martínez", Correo = "cquesada@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("28/03/2022"), FechaNacimiento = ParseFecha("04/04/1994"), Direccion = "CARRERA 100 # 20-93", Barrio = "fontibon centro", Celular = "3165374502", ContactoEmergencia = "Yolanda Martinez", CelularEmergencia = "3152272485", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1110515353", Nombre = "Silvia Maria Hernandez Otalvaro", Correo = "smhernandez@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Bon voyage"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("26/09/2022"), FechaNacimiento = ParseFecha("28/09/1991"), Direccion = "Cra 7 # 51-57", Barrio = "Rincón de piedra pintada", Celular = "3166965229", ContactoEmergencia = "Robinson de la Asunción", CelularEmergencia = "3123146765", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "52533807", Nombre = "Angela María Juyó Rondón", Correo = "ajuyo@aportesenlinea.com", Cargo = "COORDINADOR OPERACIONES TI", Rol = "StakeHolder Operaciones TI", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1061819651", Nombre = "Angel Robledo Giron", Correo = "arobledo@aportesenlinea.com", Cargo = "ANALISTA I INGENIERO DE DESARROLLO", Rol = "Ingeniero", CelulaId = GetCelulaId("Direccion Desarrollo"), Empresa = "AEL", FechaIngreso = ParseFecha("10/02/2025"), FechaNacimiento = ParseFecha("01/08/1999"), Direccion = "Cra 39#1-18 Monte Arroyo Casa B5", Barrio = "Maria Occidente", Celular = "3144825261", ContactoEmergencia = "Maria Alexandra Giron", CelularEmergencia = "3235692420", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1001344075", Nombre = "Andrés Mauricio Acero Garavito", Correo = "aacero@aportesenlinea.com", Cargo = "ANALISTA I INGENIERO DE DESARROLLO", Rol = "Ingeniero", CelulaId = GetCelulaId("Wakanda"), Empresa = "AEL", FechaIngreso = ParseFecha("02/05/2022"), FechaNacimiento = ParseFecha("30/08/2023"), Direccion = "Carrera 51A #4-17", Barrio = "Colonia Oriental", Celular = "3209226635", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1144037797", Nombre = "Andres Felipe Cabezas", Correo = "acabezas@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Nova"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("21/07/2023"), FechaNacimiento = ParseFecha("10/08/1990"), Direccion = "Carrera 23 # 10 -73 und roseto apto 605G", Barrio = "Parque natura", Celular = "3158403822", ContactoEmergencia = "Ana Carolina", CelularEmergencia = "3163579046", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1020713148", Nombre = "Maria Kamila Redondo Gordillo", Correo = "mredondo@aportesenlinea.com", Cargo = "DIRECTOR SERVICIOS DE CONTACTO", Rol = "Sponsor", CelulaId = GetCelulaId("Wakanda"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2020"), FechaNacimiento = ParseFecha("01/01/1985"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "79694723", Nombre = "Alexander Castro Morales", Correo = "acastro@aportesenlinea.com", Cargo = "DIRECTOR DE DESARROLLO", Rol = "Director", CelulaId = GetCelulaId("Direccion Desarrollo"), Empresa = "AEL", FechaIngreso = ParseFecha("10/04/2023"), FechaNacimiento = ParseFecha("12/07/2023"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80127568-NV", Nombre = "Robert Ricardo Ramirez Rojas", Correo = "rramirez@aportesenlinea.com", Cargo = "GERENTE DE TRANSFORMACIÓN DIGITAL", Rol = "Sponsor", CelulaId = GetCelulaId("Nova"), Empresa = "AEL", FechaIngreso = ParseFecha("01/01/2020"), FechaNacimiento = ParseFecha("01/01/1975"), Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1019075102-MS", Nombre = "Alejandra Xiomara Jimenez", Correo = "ajimenez@aportesenlinea.com", Cargo = "ANALISTA III SCRUM MASTER", Rol = "Scrum Master", CelulaId = GetCelulaId("MindShift"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("21/10/2024"), FechaNacimiento = ParseFecha("03/10/1992"), Direccion = "Diagonal 54 # 17 – 100, apto 1230", Barrio = "Poblado niquia", Celular = "301 488 8581", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1019075102-DS", Nombre = "Alejandra Xiomara Jimenez", Correo = "ajimenez@aportesenlinea.com", Cargo = "ANALISTA III SCRUM MASTER", Rol = "Scrum Master", CelulaId = GetCelulaId("Data Stargazers"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("21/10/2024"), FechaNacimiento = ParseFecha("03/10/1992"), Direccion = "Diagonal 54 # 17 – 100, apto 1230", Barrio = "Poblado niquia", Celular = "301 488 8581", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "52856823", Nombre = "Alejandra Paola Rubio", Correo = "arubio@aportesenlinea.com", Cargo = "COORDINADOR ARQUITECTO DE SOFTWARE", Rol = "Arquitecto", CelulaId = GetCelulaId("Nova"), Empresa = "AEL", FechaIngreso = ParseFecha("09/01/2024"), FechaNacimiento = ParseFecha("06/02/1981"), Direccion = "Cra 78 H 49 B 42 sur", Barrio = "Catalina I", Celular = "3114976547", ContactoEmergencia = "César Augusto Correcha", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1001936659", Nombre = "Adrián Andrés Bolaños Herrera", Correo = "abolanos@aportesenlinea.com", Cargo = "ANALISTA I INGENIERO DE DESARROLLO", Rol = "Ingeniero", CelulaId = GetCelulaId("Enterprise Team"), Empresa = "AEL", FechaIngreso = ParseFecha("03/01/2022"), FechaNacimiento = ParseFecha("22/02/2023"), Direccion = "Los Caracoles, Mz-25 Lt-17 2da etapa", Barrio = "Los Caracoles", Celular = "3127566311", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1070004328", Nombre = "Kenneth Daniel Valderrama Parra", Correo = "kvalderrama@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "SQA", FechaIngreso = ParseFecha("01/01/2022"), FechaNacimiento = ParseFecha("05/04/1986"), Direccion = "Carrera 6 #5 - 87 sur", Barrio = "El Prado", Celular = "3102307476", CelularEmergencia = "3103740269", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1001998132", Nombre = "Jorge Isaac Alarcon Sierra", Correo = "jalarcon@aportesenlinea.com", Cargo = "ANALISTA I INGENIERO DE DESARROLLO TRAINEE", Rol = "Ingeniero", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "AEL", FechaIngreso = ParseFecha("09/12/2025"), FechaNacimiento = ParseFecha("07/02/2002"), Direccion = "Calle 36 #14c- 40", Barrio = "La Floresta", Celular = "3226578953", ContactoEmergencia = "Delvis Sierra", CelularEmergencia = "3017932148", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1022938000", Nombre = "Samuel Felipe Moreno Ledesma", Correo = "smoreno@aportesenlinea.com", Cargo = "APRENDIZ SENA PRODUCTIVO CALIDAD Y AUT", Rol = "Ingeniero", CelulaId = GetCelulaId("Direccion Desarrollo"), Empresa = "AEL", FechaIngreso = ParseFecha("16/01/2026"), FechaNacimiento = ParseFecha("31/10/2005"), Direccion = "Calle 67C sur 1 B 89 Este, conjunto recidencial Quintas del portal 5", Barrio = "La Fiscala", Celular = "3236910886", ContactoEmergencia = "Yenimar Bernal", CelularEmergencia = "3138636718", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1031809624", Nombre = "Santiago David Castro Medina", Correo = "scastro@aportesenlinea.com", Cargo = "APRENDIZ SENA PRODUCTIVO CALIDAD Y AUT", Rol = "Ingeniero", CelulaId = GetCelulaId("Direccion Desarrollo"), Empresa = "AEL", FechaIngreso = ParseFecha("16/01/2026"), FechaNacimiento = ParseFecha("22/01/2007"), Direccion = "Kr 87 No 1 Sur 78 LC 3 AP 403", Barrio = "Patio Bonito", Celular = "3102637609", ContactoEmergencia = "Clara Milena Medina Osorio", CelularEmergencia = "3106692682", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1234988522", Nombre = "Mateo Gómez Meneses", Correo = "mgomezm@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Maya"), Empresa = "STEFANINI", FechaIngreso = ParseFecha("28/01/2026"), FechaNacimiento = ParseFecha("29/08/1997"), Direccion = "Calle 83 #45A36", Barrio = "Manrique", Celular = "3216573346", ContactoEmergencia = "Patricia Meneses", CelularEmergencia = "3127470490", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1192910852", Nombre = "Luis Eduardo Diaz Acosta", Correo = "lediaz@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("MindShift"), Empresa = "AEL", FechaIngreso = ParseFecha("02/02/2026"), FechaNacimiento = ParseFecha("19/07/2000"), Direccion = "Calle 20 #15B - 05", Barrio = "Pueblito Español", Celular = "3023782320", ContactoEmergencia = "Rafael Díaz Mejia", CelularEmergencia = "3126032804", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1007157090", Nombre = "Juan Camilo Ortega Duarte", Correo = "jortega@aportesenlinea.com", Cargo = "ANALISTA I INGENIERO DE DESARROLLO TRAINEE", Rol = "Ingeniero", CelulaId = GetCelulaId("Wakanda"), Empresa = "AEL", FechaIngreso = ParseFecha("02/02/2026"), FechaNacimiento = ParseFecha("12/02/2003"), Direccion = "Calle 13 #36c - 61", Barrio = "Ciudad Verde", Celular = "3125271680", ContactoEmergencia = "Lucia Johanna Duarte Sanchez", CelularEmergencia = "3142424264", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1111791640", Nombre = "Wilson Stiven Salgado Blanco", Correo = "wsalgado@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Bon Voyage"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("18/09/2024"), FechaNacimiento = ParseFecha("24/04/1993"), Direccion = "Cll 14F #35-39 CR Solares APTO 501 TO 6", Barrio = "barrio Ciudad del Valle", Celular = "3005124264", ContactoEmergencia = "Carolina Buitrago Leon", CelularEmergencia = "3006901473", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1214724608", Nombre = "Juan Sebastian Vargas Rocha", Correo = "jvargas@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("MindShift"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("08/04/2026"), FechaNacimiento = ParseFecha("24/06/1994"), Direccion = "Calle 51 A # 43 - 70", Barrio = "Prado", Celular = "3175730872", ContactoEmergencia = "Sandra Zapata", CelularEmergencia = "3014495946", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1007449064", Nombre = "Jhonattan Danilo Sabogal Pinzon", Correo = "jsabogal@aportesenlinea.com", Cargo = "ANALISTA I INGENIERO DE DESARROLLO", Rol = "Ingeniero", CelulaId = sinAsignarId, Empresa = "AEL", FechaIngreso = ParseFecha("15/04/2026"), FechaNacimiento = ParseFecha("14/01/2001"), Direccion = "Calle 47 #50 - 17", Barrio = "Chapinerito", Celular = "3183803805", ContactoEmergencia = "Diana Pinzón", CelularEmergencia = "3182646327", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "52776078", Nombre = "Rocio Gordillo Diaz", Correo = "rgordillo@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Data Stargazers"), Empresa = "QVISION", FechaIngreso = ParseFecha("20/04/2026"), FechaNacimiento = ParseFecha("31/03/1981"), Direccion = "Carrera 118 # 83a 59 Casa 106 Quintas de Santa Barbara VI-II", Barrio = "El Cortijo", Celular = "3214547557", ContactoEmergencia = "Diego Moreno", CelularEmergencia = "3208392754", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1110487049", Nombre = "Yeny Paola Alfonso Marroquin", Correo = "yalfonso@aportesenlinea.com", Cargo = "ANALISTA II QA", Rol = "QA", CelulaId = GetCelulaId("Nova"), Empresa = "QVISION", FechaIngreso = ParseFecha("20/04/2026"), FechaNacimiento = ParseFecha("01/06/1989"), Direccion = "Carrera 9 avenida guavinal #79 00, bosque largo torre 16 apt 202", Barrio = "Bosque Largo", Celular = "3158240629", ContactoEmergencia = "Oscar Iván Ballesteros", CelularEmergencia = "3185206599", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1035428518", Nombre = "Sebastian Tobon Carvajal", Correo = "stobon@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Facturador"), Empresa = "MICHAEL PAGE", FechaIngreso = ParseFecha("27/04/2026"), FechaNacimiento = ParseFecha("10/10/1992"), Direccion = "Tr 38 AA No 57 110, apto 2221, unidad puerto paraíso", Barrio = "Santa Ana", Celular = "3207523478", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "14327144", Nombre = "Dalton Anderson Forero Solano", Correo = "dforero@aportesenlinea.com", Cargo = "ANALISTA II INGENIERO DE DESARROLLO SEMI-SENIOR", Rol = "Ingeniero", CelulaId = GetCelulaId("Facturador"), Empresa = "MICHAEL PAGE", FechaIngreso = ParseFecha("27/04/2026"), FechaNacimiento = ParseFecha("29/10/1984"), Direccion = "Calle 10 No. 20A-19", Barrio = "Versalles", Celular = "3128291610", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80826699-MY", Nombre = "Andres Patricio Rojas Sanjuan", Correo = "aprojas@aportesenlinea.com", Cargo = "COORDINADOR ARQUITECTO DE SOFTWARE", Rol = "Arquitecto", CelulaId = GetCelulaId("Maya"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("19/05/2026"), FechaNacimiento = ParseFecha("24/04/1984"), Direccion = "Calle 41 No. 1c-48 Apartamento 503B Edificio Gran Reserva", Barrio = "Balcones de Santa Ines", Celular = "3218028432", ContactoEmergencia = "Maria Alejandra Cipamocha", CelularEmergencia = "3212626531", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "80826699-PS", Nombre = "Andres Patricio Rojas Sanjuan", Correo = "aprojas@aportesenlinea.com", Cargo = "COORDINADOR ARQUITECTO DE SOFTWARE", Rol = "Arquitecto", CelulaId = GetCelulaId("Polaris Software Team"), Empresa = "SOPHOS", FechaIngreso = ParseFecha("19/05/2026"), FechaNacimiento = ParseFecha("24/04/1984"), Direccion = "Calle 41 No. 1c-48 Apartamento 503B Edificio Gran Reserva", Barrio = "Balcones de Santa Ines", Celular = "3218028432", ContactoEmergencia = "Maria Alejandra Cipamocha", CelularEmergencia = "3212626531", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now },
            new Consultor { Cedula = "1152200960", Nombre = "Yuliana Ospina Arango", Correo = "yospina@aportesenlinea.com", Cargo = "ANALISTA III PRODUCT OWNER TECNICO", Rol = "PO Técnico", CelulaId = GetCelulaId("Maya"), Empresa = "PERIFERIA IT", FechaIngreso = ParseFecha("12/05/2026"), FechaNacimiento = ParseFecha("25/05/1993"), Direccion = "Calle 43 No. 88 - 34", Barrio = "La America", Celular = "3223911271", ContactoEmergencia = "Maria Alejandra Guerra Arango", CelularEmergencia = "3218446292", Estado = EstadoConsultor.Activo, FechaCreacion = DateTime.Now }
        };

        _context.Consultores.AddRange(consultores);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Se crearon {consultores.Count} consultores reales del CSV");
    }
}

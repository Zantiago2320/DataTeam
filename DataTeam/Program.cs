using DataTeam.Data;
using DataTeam.Services;
using DataTeam.Services.BackgroundJobs;
using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Usar base de datos en memoria para desarrollo
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseInMemoryDatabase("DataTeamInMemoryDB"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => 
{
    options.SignIn.RequireConfirmedAccount = false; // Permitir login sin confirmar email en desarrollo
    options.Password.RequireDigit = false; // Permitir contraseñas simples en desarrollo
    options.Password.RequiredLength = 4;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddRoles<IdentityRole>() // Agregar soporte para roles
.AddEntityFrameworkStores<ApplicationDbContext>();

// Configurar rutas de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.LogoutPath = "/Identity/Account/Logout";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

// Registrar servicios personalizados
builder.Services.AddScoped<IAuditoriaService, AuditoriaService>();
builder.Services.AddScoped<IAuditoriaLogFormatterService, AuditoriaLogFormatterService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<ICsvService, CsvService>();
builder.Services.AddScoped<IExcelService, ExcelService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IEquipoService, EquipoService>();
builder.Services.AddScoped<CumpleanosJob>();
builder.Services.AddScoped<ReporteMensualJob>();
builder.Services.AddScoped<DbInitializerService>(); // Servicio de inicialización

// Configurar Hangfire con InMemoryStorage para desarrollo
// NOTA: Los trabajos se pierden al reiniciar. En producción se usará SQL Server.
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseInMemoryStorage());

builder.Services.AddHangfireServer(options =>
{
    options.SchedulePollingInterval = TimeSpan.FromSeconds(15);
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Inicializar base de datos con datos de ejemplo
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var initializer = services.GetRequiredService<DbInitializerService>();
        await initializer.InitializeAsync();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Error al inicializar la base de datos");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

// Agregar Hangfire Dashboard
app.UseHangfireDashboard("/hangfire");

app.UseStaticFiles();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

// Configurar trabajos recurrentes de Hangfire
using (var scope = app.Services.CreateScope())
{
    var recurringJobManager = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    // Ejecutar el primer día hábil del mes a las 8:00 AM
    recurringJobManager.AddOrUpdate<CumpleanosJob>(
        "enviar-cumpleanos",
        job => job.EnviarCorreosCumpleanosDelMesAsync(),
        "0 8 1-7 * *"); // Días 1-7 de cada mes a las 8 AM (el job verifica si es el primer día hábil)

    // Ejecutar el día 15 de cada mes a las 9:00 AM
    recurringJobManager.AddOrUpdate<ReporteMensualJob>(
        "reporte-mensual",
        job => job.EnviarReporteMensualAsync(),
        "0 9 15 * *");
}

await app.RunAsync();

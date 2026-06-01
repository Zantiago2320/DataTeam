using DataTeam.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Consultor> Consultores { get; set; }
    public DbSet<Celula> Celulas { get; set; }
    public DbSet<CelulaLider> CelulaLideres { get; set; }
    public DbSet<CelulaMiembro> CelulaMiembros { get; set; }
    public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }
    public DbSet<ProcesoContratacion> ProcesosContratacion { get; set; }
    public DbSet<ActividadAdmin> ActividadesAdmin { get; set; }
    public DbSet<ActividadOnOffboarding> ActividadesOnOffboarding { get; set; }
    public DbSet<LicenciaCopilot> LicenciasCopilot { get; set; }
    public DbSet<Novedad> Novedades { get; set; }
    public DbSet<ContactoFabrica> ContactosFabricas { get; set; }
    public DbSet<HistorialEnvioExcel> HistorialEnviosExcel { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurar índices únicos
        modelBuilder.Entity<Consultor>()
            .HasIndex(c => c.Cedula)
            .IsUnique();

        modelBuilder.Entity<Consultor>()
            .HasIndex(c => c.Correo)
            .IsUnique();

        modelBuilder.Entity<ContactoFabrica>()
            .HasIndex(cf => cf.Correo)
            .IsUnique();

        // Query filters globales para soft delete
        modelBuilder.Entity<Consultor>()
            .HasQueryFilter(c => !c.Eliminado);

        modelBuilder.Entity<ProcesoContratacion>()
            .HasQueryFilter(pc => !pc.Eliminado);

        modelBuilder.Entity<ActividadAdmin>()
            .HasQueryFilter(aa => !aa.Eliminado);

        modelBuilder.Entity<ActividadOnOffboarding>()
            .HasQueryFilter(ao => !ao.Eliminado);

        modelBuilder.Entity<LicenciaCopilot>()
            .HasQueryFilter(lc => !lc.Eliminado);

        modelBuilder.Entity<Novedad>()
            .HasQueryFilter(n => !n.Eliminado);

        modelBuilder.Entity<ContactoFabrica>()
            .HasQueryFilter(cf => !cf.Eliminado);

        // Configurar relación Consultor-Celula
        modelBuilder.Entity<Consultor>()
            .HasOne(c => c.Celula)
            .WithMany(ce => ce.Consultores)
            .HasForeignKey(c => c.CelulaId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configurar relación CelulaLider
        modelBuilder.Entity<CelulaLider>()
            .HasOne(cl => cl.Celula)
            .WithMany(c => c.CelulaLideres)
            .HasForeignKey(cl => cl.CelulaId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CelulaLider>()
            .HasOne(cl => cl.Consultor)
            .WithMany(c => c.CelulasQueLidera)
            .HasForeignKey(cl => cl.ConsultorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configurar relación AuditoriaLog-Consultor
        modelBuilder.Entity<AuditoriaLog>()
            .HasOne(a => a.Consultor)
            .WithMany(c => c.Auditorias)
            .HasForeignKey(a => a.ConsultorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data inicial
        modelBuilder.Entity<Celula>().HasData(
            new Celula { Id = 1, Nombre = "Sin Asignar", Descripcion = "Célula por defecto", Color = "#808080", FechaCreacion = DateTime.Now }
        );

        }
}


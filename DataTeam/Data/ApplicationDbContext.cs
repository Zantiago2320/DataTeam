using DataTeam.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Consultor> Consultores { get; set; }
    public DbSet<Celula> Celulas { get; set; }
    public DbSet<Equipo> Equipos { get; set; }
    public DbSet<EquipoLider> EquipoLideres { get; set; }
    public DbSet<CelulaLider> CelulaLideres { get; set; }
    public DbSet<CelulaMiembro> CelulaMiembros { get; set; } // Nueva tabla para asignaciones de miembros con roles
    public DbSet<EquipoMiembro> EquipoMiembros { get; set; } // Nueva tabla para asignaciones múltiples
    public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }
    public DbSet<ProcesoContratacion> ProcesosContratacion { get; set; }
    public DbSet<ActividadAdmin> ActividadesAdmin { get; set; }
    public DbSet<ActividadOnOffboarding> ActividadesOnOffboarding { get; set; }
    public DbSet<LicenciaCopilot> LicenciasCopilot { get; set; }
    public DbSet<Novedad> Novedades { get; set; }
    public DbSet<ContactoFabrica> ContactosFabricas { get; set; }

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

        // Configurar relación Consultor-Equipo (opcional)
        modelBuilder.Entity<Consultor>()
            .HasOne(c => c.Equipo)
            .WithMany(e => e.Consultores)
            .HasForeignKey(c => c.EquipoId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configurar relación EquipoLider (many-to-many entre Equipo y Consultor)
        modelBuilder.Entity<EquipoLider>()
            .HasOne(el => el.Equipo)
            .WithMany(e => e.EquipoLideres)
            .HasForeignKey(el => el.EquipoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EquipoLider>()
            .HasOne(el => el.Consultor)
            .WithMany(c => c.EquiposQueLidera)
            .HasForeignKey(el => el.ConsultorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configurar relación CelulaLider (many-to-many entre Celula y Consultor)
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

        // Configurar relación EquipoMiembro (many-to-many con % participación)
        modelBuilder.Entity<EquipoMiembro>()
            .HasOne(em => em.Equipo)
            .WithMany()
            .HasForeignKey(em => em.EquipoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<EquipoMiembro>()
            .HasOne(em => em.Consultor)
            .WithMany()
            .HasForeignKey(em => em.ConsultorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Índice único para evitar duplicados en EquipoMiembro
        modelBuilder.Entity<EquipoMiembro>()
            .HasIndex(em => new { em.EquipoId, em.ConsultorId })
            .IsUnique();

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

        modelBuilder.Entity<Equipo>().HasData(
            new Equipo { Id = 1, Nombre = "Sin Asignar", Descripcion = "Equipo por defecto", Color = "#808080", FechaCreacion = DateTime.Now }
        );
    }
}


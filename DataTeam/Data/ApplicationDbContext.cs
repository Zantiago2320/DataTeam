using DataTeam.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Consultor> Consultores { get; set; }
    public DbSet<Celula> Celulas { get; set; }
    public DbSet<AuditoriaLog> AuditoriaLogs { get; set; }

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

        // Configurar relación Consultor-Celula
        modelBuilder.Entity<Consultor>()
            .HasOne(c => c.Celula)
            .WithMany(ce => ce.Consultores)
            .HasForeignKey(c => c.CelulaId)
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


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataTeam.Models;

public class AuditoriaLog
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Entidad")]
    [StringLength(100)]
    public string Entidad { get; set; } = string.Empty;

    [Required]
    [Display(Name = "ID de Entidad")]
    public int EntidadId { get; set; }

    [Required]
    [Display(Name = "Acción")]
    [StringLength(50)]
    public string Accion { get; set; } = string.Empty;

    [Display(Name = "Usuario")]
    [StringLength(256)]
    public string? Usuario { get; set; }

    [Display(Name = "Rol")]
    [StringLength(50)]
    public string? RolUsuario { get; set; }

    [Display(Name = "Valores Anteriores")]
    public string? ValoresAnteriores { get; set; }

    [Display(Name = "Valores Nuevos")]
    public string? ValoresNuevos { get; set; }

    [Display(Name = "Fecha")]
    public DateTime Fecha { get; set; } = DateTime.Now;

    [Display(Name = "Dirección IP")]
    [StringLength(45)]
    public string? DireccionIP { get; set; }

    public int? ConsultorId { get; set; }

    [ForeignKey(nameof(ConsultorId))]
    public Consultor? Consultor { get; set; }
}

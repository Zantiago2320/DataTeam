using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataTeam.Models;

/// <summary>
/// Tabla intermedia para la relación many-to-many entre Equipos y Líderes (Consultores)
/// </summary>
public class EquipoLider
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Equipo")]
    public int EquipoId { get; set; }

    [ForeignKey(nameof(EquipoId))]
    public Equipo? Equipo { get; set; }

    [Required]
    [Display(Name = "Líder (Consultor)")]
    public int ConsultorId { get; set; }

    [ForeignKey(nameof(ConsultorId))]
    public Consultor? Consultor { get; set; }

    [Display(Name = "Fecha de Asignación")]
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;

    [Display(Name = "Es Líder Principal")]
    public bool EsLiderPrincipal { get; set; } = false;
}

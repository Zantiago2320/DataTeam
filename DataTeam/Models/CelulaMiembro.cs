using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataTeam.Models;

/// <summary>
/// Tabla intermedia para la relación many-to-many entre Células y Miembros (Consultores) con roles
/// </summary>
public class CelulaMiembro
{
    [Key]
    public int Id { get; set; }

    [Required]
    [Display(Name = "Célula")]
    public int CelulaId { get; set; }

    [ForeignKey(nameof(CelulaId))]
    public Celula? Celula { get; set; }

    [Required]
    [Display(Name = "Miembro (Consultor)")]
    public int ConsultorId { get; set; }

    [ForeignKey(nameof(ConsultorId))]
    public Consultor? Consultor { get; set; }

    [Required(ErrorMessage = "El rol es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Rol en la Célula")]
    public string Rol { get; set; } = string.Empty;

    [Display(Name = "Fecha de Asignación")]
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
}

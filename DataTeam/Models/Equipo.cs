using System.ComponentModel.DataAnnotations;

namespace DataTeam.Models;

/// <summary>
/// Representa un equipo de trabajo en la organización
/// </summary>
public class Equipo
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre del equipo es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Nombre del Equipo")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [StringLength(50)]
    [Display(Name = "Color Identificador")]
    public string? Color { get; set; }

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Display(Name = "Fecha de Última Modificación")]
    public DateTime? FechaModificacion { get; set; }

    // Relaciones
    [Display(Name = "Líderes del Equipo")]
    public ICollection<EquipoLider> EquipoLideres { get; set; } = new List<EquipoLider>();

    [Display(Name = "Miembros del Equipo")]
    public ICollection<Consultor> Consultores { get; set; } = new List<Consultor>();
}

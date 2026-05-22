using System.ComponentModel.DataAnnotations;

namespace DataTeam.Models;

/// <summary>
/// Modelo para gestión de licencias Copilot
/// Basado en la hoja "Licencias Copilot" del Excel
/// </summary>
public class LicenciaCopilot
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Asignado a")]
    public string AsignadoA { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Fecha de asignación")]
    [DataType(DataType.Date)]
    public DateTime FechaAsignacion { get; set; } = DateTime.Today;

    [Required]
    [StringLength(100)]
    [Display(Name = "Célula")]
    public string Celula { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "Proyecto")]
    public string? Proyecto { get; set; }

    [Display(Name = "Activa")]
    public bool Activa { get; set; } = true;

    [Display(Name = "Fecha de liberación")]
    [DataType(DataType.Date)]
    public DateTime? FechaLiberacion { get; set; }

    [StringLength(500)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    [Display(Name = "Fecha de creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Display(Name = "Fecha de actualización")]
    public DateTime? FechaActualizacion { get; set; }

    [Display(Name = "Eliminado")]
    public bool Eliminado { get; set; } = false;
}

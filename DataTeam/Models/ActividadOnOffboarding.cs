using System.ComponentModel.DataAnnotations;

namespace DataTeam.Models;

/// <summary>
/// Modelo para actividades de Onboarding y Offboarding
/// Basado en la hoja "On - Offboarding" del Excel
/// </summary>
public class ActividadOnOffboarding
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Actividad")]
    public string Actividad { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    [Display(Name = "Tipo")]
    public string Tipo { get; set; } = "Onboarding"; // Onboarding u Offboarding

    [Display(Name = "Interno")]
    public bool Interno { get; set; }

    [Display(Name = "Externo")]
    public bool Externo { get; set; }

    [StringLength(500)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    [Display(Name = "Tiempo (días)")]
    public int? TiempoDias { get; set; }

    [Display(Name = "Orden")]
    public int Orden { get; set; }

    [Display(Name = "Fecha de creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Display(Name = "Eliminado")]
    public bool Eliminado { get; set; } = false;
}

using System.ComponentModel.DataAnnotations;

namespace DataTeam.Models;

/// <summary>
/// Modelo para novedades y anuncios
/// Basado en la hoja "Novedades" del Excel
/// </summary>
public class Novedad
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(500)]
    [Display(Name = "Novedad")]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Fecha")]
    [DataType(DataType.Date)]
    public DateTime Fecha { get; set; } = DateTime.Today;

    [StringLength(50)]
    [Display(Name = "Mes")]
    public string? Mes { get; set; }

    [StringLength(200)]
    [Display(Name = "Responsable")]
    public string? Responsable { get; set; }

    [Display(Name = "Activa")]
    public bool Activa { get; set; } = true;

    [Display(Name = "Fecha de creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Display(Name = "Eliminado")]
    public bool Eliminado { get; set; } = false;
}

using System.ComponentModel.DataAnnotations;

namespace DataTeam.Models;

/// <summary>
/// Modelo para contactos de las fábricas
/// Basado en la hoja "Datos Fábricas" del Excel
/// </summary>
public class ContactoFabrica
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Cargo")]
    public string Cargo { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    [EmailAddress]
    [Display(Name = "Correo")]
    public string Correo { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Phone]
    [Display(Name = "Número de contacto")]
    public string NumeroContacto { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    [Display(Name = "Fábrica")]
    public string Fabrica { get; set; } = string.Empty;

    [Display(Name = "Activo")]
    public bool Activo { get; set; } = true;

    [Display(Name = "Fecha de creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Display(Name = "Fecha de actualización")]
    public DateTime? FechaActualizacion { get; set; }

    [Display(Name = "Eliminado")]
    public bool Eliminado { get; set; } = false;

    [Display(Name = "Fecha de eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [StringLength(256)]
    [Display(Name = "Eliminado por")]
    public string? EliminadoPor { get; set; }
}

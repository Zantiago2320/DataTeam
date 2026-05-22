using System.ComponentModel.DataAnnotations;

namespace DataTeam.Models;

/// <summary>
/// Modelo para actividades administrativas mensuales
/// Basado en la hoja "Actividades Admin" del Excel
/// </summary>
public class ActividadAdmin
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Actividad")]
    public string Actividad { get; set; } = string.Empty;

    [Display(Name = "Día 1")]
    public bool Dia01 { get; set; }

    [Display(Name = "Día 2")]
    public bool Dia02 { get; set; }

    [Display(Name = "Día 3")]
    public bool Dia03 { get; set; }

    [Display(Name = "Día 4")]
    public bool Dia04 { get; set; }

    [Display(Name = "Día 5")]
    public bool Dia05 { get; set; }

    [Display(Name = "Día 6")]
    public bool Dia06 { get; set; }

    [Display(Name = "Día 7")]
    public bool Dia07 { get; set; }

    [Display(Name = "Día 8")]
    public bool Dia08 { get; set; }

    [Display(Name = "Día 9")]
    public bool Dia09 { get; set; }

    [Display(Name = "Día 10")]
    public bool Dia10 { get; set; }

    [Display(Name = "Día 11")]
    public bool Dia11 { get; set; }

    [Display(Name = "Día 12")]
    public bool Dia12 { get; set; }

    [Display(Name = "Día 13")]
    public bool Dia13 { get; set; }

    [Display(Name = "Día 14")]
    public bool Dia14 { get; set; }

    [Display(Name = "Día 15")]
    public bool Dia15 { get; set; }

    [Display(Name = "Día 16")]
    public bool Dia16 { get; set; }

    [Display(Name = "Día 17")]
    public bool Dia17 { get; set; }

    [Display(Name = "Día 18")]
    public bool Dia18 { get; set; }

    [Display(Name = "Día 19")]
    public bool Dia19 { get; set; }

    [Display(Name = "Día 20")]
    public bool Dia20 { get; set; }

    [Display(Name = "Día 21")]
    public bool Dia21 { get; set; }

    [Display(Name = "Día 22")]
    public bool Dia22 { get; set; }

    [Display(Name = "Día 23")]
    public bool Dia23 { get; set; }

    [Display(Name = "Día 24")]
    public bool Dia24 { get; set; }

    [Display(Name = "Día 25")]
    public bool Dia25 { get; set; }

    [Display(Name = "Día 26")]
    public bool Dia26 { get; set; }

    [Display(Name = "Día 27")]
    public bool Dia27 { get; set; }

    [Display(Name = "Día 28")]
    public bool Dia28 { get; set; }

    [Display(Name = "Día 29")]
    public bool Dia29 { get; set; }

    [Display(Name = "Día 30")]
    public bool Dia30 { get; set; }

    [Display(Name = "Día 31")]
    public bool Dia31 { get; set; }

    [StringLength(500)]
    [Display(Name = "Observaciones")]
    public string? Observaciones { get; set; }

    [Display(Name = "Mes/Año")]
    [DataType(DataType.Date)]
    public DateTime PeriodoMesAnio { get; set; } = DateTime.Today;

    [Display(Name = "Fecha de creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Display(Name = "Eliminado")]
    public bool Eliminado { get; set; } = false;
}

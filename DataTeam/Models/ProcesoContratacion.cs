using System.ComponentModel.DataAnnotations;

namespace DataTeam.Models;

/// <summary>
/// Modelo para gestión de procesos de contratación
/// Basado en la hoja "Proceso contratacion" del Excel
/// </summary>
public class ProcesoContratacion
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Display(Name = "Requerimiento")]
    public string Requerimiento { get; set; } = string.Empty;

    [StringLength(200)]
    [Display(Name = "HV (Hoja de Vida)")]
    public string? HojaVida { get; set; }

    [StringLength(500)]
    [Display(Name = "Entrevistas")]
    public string? Entrevistas { get; set; }

    [StringLength(50)]
    [Display(Name = "Status")]
    public string? Status { get; set; }

    [StringLength(100)]
    [Display(Name = "Célula")]
    public string? Celula { get; set; }

    [StringLength(200)]
    [Display(Name = "Proyecto")]
    public string? Proyecto { get; set; }

    [Display(Name = "Tiempo de contratación (meses)")]
    public int? TiempoContratacionMeses { get; set; }

    [StringLength(100)]
    [Display(Name = "Rol")]
    public string? Rol { get; set; }

    [StringLength(100)]
    [Display(Name = "Fábrica")]
    public string? Fabrica { get; set; }

    [StringLength(200)]
    [Display(Name = "Nombre")]
    public string? Nombre { get; set; }

    [Display(Name = "Fecha de ingreso planeada")]
    [DataType(DataType.Date)]
    public DateTime? FechaIngreso { get; set; }

    [Display(Name = "Seleccionado")]
    public bool Seleccionado { get; set; }

    [StringLength(200)]
    [Display(Name = "Propuesta")]
    public string? Propuesta { get; set; }

    [Display(Name = "Matriz completada")]
    public bool MatrizCompleta { get; set; }

    [Display(Name = "Información usuarios")]
    public bool InformacionUsuarios { get; set; }

    [Display(Name = "Escalado a TH")]
    public bool EscaladoTH { get; set; }

    [Display(Name = "Entrega equipo en AEL")]
    [DataType(DataType.Date)]
    public DateTime? EntregaEquipoAEL { get; set; }

    [Display(Name = "Escalado a TI")]
    public bool EscaladoTI { get; set; }

    [Display(Name = "Entrega equipo a FAB")]
    [DataType(DataType.Date)]
    public DateTime? EntregaEquipoFAB { get; set; }

    [Display(Name = "Entrega equipo usuario final")]
    [DataType(DataType.Date)]
    public DateTime? EntregaEquipoUsuarioFinal { get; set; }

    [Display(Name = "Vincular a célula en Azure DevOps")]
    public bool VincularAzureDevOps { get; set; }

    [Display(Name = "Onboarding")]
    public bool OnboardingCompleto { get; set; }

    [Display(Name = "Fecha ingreso real")]
    [DataType(DataType.Date)]
    public DateTime? FechaIngresoReal { get; set; }

    [Display(Name = "Desviación en días")]
    public int? DesviacionDias { get; set; }

    [StringLength(200)]
    [Display(Name = "Encargado")]
    public string? Encargado { get; set; }

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

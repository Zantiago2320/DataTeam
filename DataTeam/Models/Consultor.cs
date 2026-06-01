using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataTeam.Models;

public class Consultor
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "La cédula es obligatoria")]
    [StringLength(20)]
    [Display(Name = "Cédula")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(200)]
    [Display(Name = "Nombre Completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no es válido")]
    [StringLength(100)]
    [Display(Name = "Correo Electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cargo es obligatorio")]
    [StringLength(100)]
    public string Cargo { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Ruta de Foto")]
    public string? RutaFoto { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
    [Display(Name = "Fecha de Ingreso")]
    [DataType(DataType.Date)]
    public DateTime FechaIngreso { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    [Display(Name = "Fecha de Nacimiento")]
    [DataType(DataType.Date)]
    [FechaNoPuedeSerFutura(ErrorMessage = "La fecha de nacimiento no puede ser en el futuro")]
    public DateTime FechaNacimiento { get; set; }

    [Display(Name = "Célula")]
    public int? CelulaId { get; set; }

    [ForeignKey(nameof(CelulaId))]
    public Celula? Celula { get; set; }

    [StringLength(100)]
    public string? Rol { get; set; }

    [Display(Name = "Capacidad (%)")]
    [Range(0, 100, ErrorMessage = "La capacidad debe estar entre 0 y 100")]
    public int? Capacidad { get; set; }

    [StringLength(100)]
    public string? Empresa { get; set; }

    [StringLength(200)]
    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [StringLength(100)]
    public string? Barrio { get; set; }

    [StringLength(20)]
    [Phone(ErrorMessage = "El número de celular no es válido")]
    public string? Celular { get; set; }

    [StringLength(200)]
    [Display(Name = "Contacto de Emergencia")]
    public string? ContactoEmergencia { get; set; }

    [StringLength(20)]
    [Phone(ErrorMessage = "El número de celular no es válido")]
    [Display(Name = "Celular de Emergencia")]
    public string? CelularEmergencia { get; set; }

    [Required]
    [Display(Name = "Estado")]
    public EstadoConsultor Estado { get; set; } = EstadoConsultor.Activo;

    [Display(Name = "Fecha de Retiro")]
    [DataType(DataType.Date)]
    public DateTime? FechaRetiro { get; set; }

    [Display(Name = "Tipo de Desvinculación")]
    public MotivoDeshabilitacion? TipoDesvinculacion { get; set; }

    [StringLength(500)]
    [Display(Name = "Motivo de Retiro")]
    public string? MotivoRetiro { get; set; }

    [Display(Name = "Eliminado")]
    public bool Eliminado { get; set; } = false;

    [Display(Name = "Fecha de Eliminación")]
    public DateTime? FechaEliminacion { get; set; }

    [StringLength(256)]
    [Display(Name = "Eliminado Por")]
    public string? EliminadoPor { get; set; }

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Display(Name = "Fecha de Actualización")]
    public DateTime? FechaActualizacion { get; set; }

    // Relaciones de navegación
    public ICollection<AuditoriaLog> Auditorias { get; set; } = new List<AuditoriaLog>();

    [Display(Name = "Células que Lidera")]
    public ICollection<CelulaLider> CelulasQueLidera { get; set; } = new List<CelulaLider>();

    [Display(Name = "Células donde es Miembro")]
    public ICollection<CelulaMiembro> CelulasMiembro { get; set; } = new List<CelulaMiembro>();
}

public enum EstadoConsultor
{
    [Display(Name = "Activo")]
    Activo,

    [Display(Name = "Retirado")]
    Retirado
}

public enum MotivoDeshabilitacion
{
    [Display(Name = "Despido")]
    Despido,

    [Display(Name = "Renuncia Voluntaria")]
    RenunciaVoluntaria,

    [Display(Name = "Fin de Contrato")]
    FinDeContrato,

    [Display(Name = "Mutuo Acuerdo")]
    MutuoAcuerdo,

    [Display(Name = "Abandono de Trabajo")]
    AbandonoDeTrabajo,

    [Display(Name = "Otros")]
    Otros
}

/// <summary>
/// Atributo de validación personalizado para evitar fechas futuras
/// </summary>
public class FechaNoPuedeSerFuturaAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is DateTime fecha)
        {
            if (fecha.Date > DateTime.Now.Date)
            {
                return new ValidationResult(ErrorMessage ?? "La fecha no puede ser en el futuro");
            }
        }
        return ValidationResult.Success;
    }
}

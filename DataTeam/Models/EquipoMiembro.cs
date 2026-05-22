using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DataTeam.Models
{
    /// <summary>
    /// Tabla de asociación para asignaciones múltiples de consultores a equipos.
    /// Permite que un consultor pertenezca a varios equipos con % de participación.
    /// </summary>
    [Table("EquipoMiembro")]
    public class EquipoMiembro
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID del equipo al que pertenece el consultor
        /// </summary>
        [Required]
        public int EquipoId { get; set; }

        /// <summary>
        /// ID del consultor miembro del equipo
        /// </summary>
        [Required]
        public int ConsultorId { get; set; }

        /// <summary>
        /// Porcentaje de participación en este equipo (1-100).
        /// La suma de todos los porcentajes de un consultor debe ser 100%.
        /// </summary>
        [Required]
        [Range(1, 100, ErrorMessage = "El porcentaje debe estar entre 1 y 100")]
        [Display(Name = "% Participación")]
        public int PorcentajeParticipacion { get; set; } = 100;

        /// <summary>
        /// Indica si este es el equipo principal del consultor.
        /// Solo uno de los equipos puede ser principal.
        /// </summary>
        [Display(Name = "¿Es equipo principal?")]
        public bool EsMiembroPrincipal { get; set; }

        /// <summary>
        /// Fecha en que se asignó el consultor a este equipo
        /// </summary>
        [Display(Name = "Fecha de Asignación")]
        [DataType(DataType.DateTime)]
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha en que se desasignó el consultor (si aplica).
        /// Null si aún está activo en el equipo.
        /// </summary>
        [Display(Name = "Fecha de Desasignación")]
        [DataType(DataType.DateTime)]
        public DateTime? FechaDesasignacion { get; set; }

        /// <summary>
        /// Indica si la asignación está activa
        /// </summary>
        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Navegación
        [ForeignKey(nameof(EquipoId))]
        public virtual Equipo? Equipo { get; set; }

        [ForeignKey(nameof(ConsultorId))]
        public virtual Consultor? Consultor { get; set; }

        /// <summary>
        /// Obtiene una descripción legible del miembro
        /// </summary>
        [NotMapped]
        public string DescripcionCompleta => $"{Consultor?.Nombre ?? "N/A"} - {Equipo?.Nombre ?? "N/A"} ({PorcentajeParticipacion}%)";
    }
}

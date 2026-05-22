using System.ComponentModel.DataAnnotations;
using DataTeam.Models;

namespace DataTeam.ViewModels;

public class DeshabilitarConsultorViewModel
{
    public int ConsultorId { get; set; }

    public string ConsultorNombre { get; set; } = string.Empty;

    public string ConsultorCedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un tipo de desvinculación")]
    [Display(Name = "Tipo de Desvinculación")]
    public MotivoDeshabilitacion TipoDesvinculacion { get; set; }

    [Required(ErrorMessage = "Debe especificar el motivo de la desvinculación")]
    [StringLength(500, ErrorMessage = "El motivo no puede exceder 500 caracteres")]
    [Display(Name = "Motivo Detallado")]
    public string MotivoDetallado { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha de retiro es obligatoria")]
    [Display(Name = "Fecha de Retiro")]
    [DataType(DataType.Date)]
    public DateTime FechaRetiro { get; set; } = DateTime.Now;
}

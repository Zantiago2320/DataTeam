using DataTeam.Models;
using System.ComponentModel.DataAnnotations;

namespace DataTeam.ViewModels;

public class ConsultorViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "La cédula es obligatoria")]
    [Display(Name = "Cédula")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [Display(Name = "Nombre Completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio")]
    [EmailAddress(ErrorMessage = "El correo no es válido")]
    [Display(Name = "Correo Electrónico")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El cargo es obligatorio")]
    public string Cargo { get; set; } = string.Empty;

    [Display(Name = "Foto de Perfil")]
    public IFormFile? FotoFile { get; set; }

    public string? RutaFoto { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
    [Display(Name = "Fecha de Ingreso")]
    [DataType(DataType.Date)]
    public DateTime FechaIngreso { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    [Display(Name = "Fecha de Nacimiento")]
    [DataType(DataType.Date)]
    public DateTime FechaNacimiento { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una célula")]
    [Display(Name = "Célula/Equipo")]
    public int CelulaId { get; set; }

    public string? CelulaNombre { get; set; }

    [Display(Name = "Rol")]
    public string? Rol { get; set; }

    [Display(Name = "Capacidad (%)")]
    [Range(0, 100, ErrorMessage = "La capacidad debe estar entre 0 y 100")]
    public int? Capacidad { get; set; }

    [Display(Name = "Empresa")]
    public string? Empresa { get; set; }

    [Display(Name = "Dirección")]
    public string? Direccion { get; set; }

    [Display(Name = "Barrio")]
    public string? Barrio { get; set; }

    [Phone(ErrorMessage = "El número de celular no es válido")]
    [Display(Name = "Celular")]
    public string? Celular { get; set; }

    [Display(Name = "Contacto de Emergencia")]
    public string? ContactoEmergencia { get; set; }

    [Phone(ErrorMessage = "El número de celular no es válido")]
    [Display(Name = "Celular de Emergencia")]
    public string? CelularEmergencia { get; set; }

    [Required]
    [Display(Name = "Estado")]
    public EstadoConsultor Estado { get; set; } = EstadoConsultor.Activo;

    [Display(Name = "Edad")]
    public int Edad => DateTime.Today.Year - FechaNacimiento.Year - 
        (DateTime.Today.DayOfYear < FechaNacimiento.DayOfYear ? 1 : 0);

    [Display(Name = "Antigüedad (años)")]
    public int Antiguedad => DateTime.Today.Year - FechaIngreso.Year - 
        (DateTime.Today.DayOfYear < FechaIngreso.DayOfYear ? 1 : 0);
}

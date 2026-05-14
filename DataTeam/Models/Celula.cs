using System.ComponentModel.DataAnnotations;

namespace DataTeam.Models;

public class Celula
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "El nombre de la célula es obligatorio")]
    [StringLength(100)]
    [Display(Name = "Nombre de Célula")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Descripción")]
    public string? Descripcion { get; set; }

    [StringLength(50)]
    [Display(Name = "Color")]
    public string? Color { get; set; }

    [Display(Name = "Líder de Célula")]
    public int? LiderConsultorId { get; set; }

    [Display(Name = "Activa")]
    public bool Activa { get; set; } = true;

    [Display(Name = "Fecha de Creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    public ICollection<Consultor> Consultores { get; set; } = new List<Consultor>();
}

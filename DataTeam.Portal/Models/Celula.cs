namespace DataTeam.Portal.Models;

public class Celula
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Color { get; set; }
    public string? ImagenUrl { get; set; }
    public bool Activa { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;

    // Relaciones
    public ICollection<CelulaMiembro> Miembros { get; set; } = new List<CelulaMiembro>();
    public ICollection<CelulaLider> Lideres { get; set; } = new List<CelulaLider>();
}

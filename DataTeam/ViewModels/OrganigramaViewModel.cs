using DataTeam.Models;

namespace DataTeam.ViewModels;

public class OrganigramaViewModel
{
    public List<CelulaConConsultores> Celulas { get; set; } = new();
}

public class CelulaConConsultores
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public string? Color { get; set; }
    public string? NombreLider { get; set; }
    public List<ConsultorResumen> Consultores { get; set; } = new();
}

public class ConsultorResumen
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string? RutaFoto { get; set; }
    public string Correo { get; set; } = string.Empty;
    public string? Rol { get; set; }
    public EstadoConsultor Estado { get; set; }
}

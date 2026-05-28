using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace DataTeam.Models;

/// <summary>
/// Entidad para registrar el historial de envíos de archivos Excel por correo
/// </summary>
public class HistorialEnvioExcel
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(255)]
    public string DestinatarioEmail { get; set; } = string.Empty;

    /// <summary>
    /// JSON array con múltiples destinatarios: ["email1@example.com", "email2@example.com"]
    /// </summary>
    [StringLength(4000)]
    public string? DestinatariosJson { get; set; }

    [StringLength(500)]
    public string? DestinatarioNombre { get; set; }

    [Required]
    [StringLength(500)]
    public string Asunto { get; set; } = string.Empty;

    [StringLength(2000)]
    public string? Mensaje { get; set; }

    [Required]
    [StringLength(255)]
    public string NombreArchivo { get; set; } = string.Empty;

    public long TamanoArchivo { get; set; } // En bytes

    public int CantidadRegistros { get; set; } // Cantidad de consultores en el Excel

    [Required]
    public DateTime FechaEnvio { get; set; } = DateTime.Now;

    [Required]
    [StringLength(100)]
    public string UsuarioEnvio { get; set; } = string.Empty; // Email del usuario que envió

    public bool EnvioExitoso { get; set; }

    [StringLength(1000)]
    public string? MensajeError { get; set; } // Si falló el envío

    // Opcional: Guardar el archivo en base de datos (puede crecer mucho)
    public byte[]? ArchivoBytes { get; set; }

    /// <summary>
    /// Helper para obtener lista de destinatarios desde JSON
    /// </summary>
    public List<string> GetDestinatarios()
    {
        if (string.IsNullOrWhiteSpace(DestinatariosJson))
        {
            return new List<string> { DestinatarioEmail };
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(DestinatariosJson) ?? new List<string> { DestinatarioEmail };
        }
        catch
        {
            return new List<string> { DestinatarioEmail };
        }
    }

    /// <summary>
    /// Helper para establecer lista de destinatarios como JSON
    /// </summary>
    public void SetDestinatarios(List<string> destinatarios)
    {
        if (destinatarios != null && destinatarios.Any())
        {
            DestinatariosJson = JsonSerializer.Serialize(destinatarios);
            DestinatarioEmail = destinatarios.First(); // Mantener primer email por compatibilidad
        }
    }

    [StringLength(50)]
    public string? TipoFiltro { get; set; } // "Todos", "Celula", etc.

    public int? CelulaIdFiltro { get; set; } // Si se filtró por célula específica
}

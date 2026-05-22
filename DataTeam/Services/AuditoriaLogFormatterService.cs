using System.Text;
using System.Text.Json;
using DataTeam.Models;

namespace DataTeam.Services;

public interface IAuditoriaLogFormatterService
{
    string FormatearCambios(string? valoresAnteriores, string? valoresNuevos);
    List<CambioDetallado> ObtenerCambiosDetallados(string? valoresAnteriores, string? valoresNuevos);
}

public class AuditoriaLogFormatterService : IAuditoriaLogFormatterService
{
    private readonly Dictionary<string, string> _nombresAmigables = new()
    {
        // Campos de Consultor
        { "Cedula", "Cédula" },
        { "Nombre", "Nombre Completo" },
        { "Correo", "Correo Electrónico" },
        { "Cargo", "Cargo" },
        { "FechaIngreso", "Fecha de Ingreso" },
        { "FechaNacimiento", "Fecha de Nacimiento" },
        { "CelulaId", "Célula" },
        { "EquipoId", "Equipo" },
        { "Rol", "Rol" },
        { "Capacidad", "Capacidad (%)" },
        { "Empresa", "Empresa" },
        { "Direccion", "Dirección" },
        { "Barrio", "Barrio" },
        { "Celular", "Celular" },
        { "ContactoEmergencia", "Contacto de Emergencia" },
        { "CelularEmergencia", "Celular de Emergencia" },
        { "Estado", "Estado" },
        { "Eliminado", "Deshabilitado" },
        { "FechaEliminacion", "Fecha de Deshabilitación" },
        { "EliminadoPor", "Deshabilitado Por" },
        { "FechaRetiro", "Fecha de Retiro" },
        { "TipoDesvinculacion", "Tipo de Desvinculación" },
        { "MotivoRetiro", "Motivo de Retiro" },
        { "RutaFoto", "Foto de Perfil" },

        // Campos de Célula
        { "Descripcion", "Descripción" },
        { "Color", "Color" },
        { "Activa", "Activa" },
        { "FechaCreacion", "Fecha de Creación" },
        { "FechaModificacion", "Fecha de Modificación" },

        // Campos de Equipo
        { "EquipoNombre", "Nombre del Equipo" }
    };

    public string FormatearCambios(string? valoresAnteriores, string? valoresNuevos)
    {
        var cambios = ObtenerCambiosDetallados(valoresAnteriores, valoresNuevos);

        if (!cambios.Any())
        {
            return "No se detectaron cambios específicos";
        }

        var sb = new StringBuilder();
        foreach (var cambio in cambios)
        {
            sb.AppendLine($"• {cambio.NombreAmigable}: {cambio.ValorAnterior} → {cambio.ValorNuevo}");
        }

        return sb.ToString().TrimEnd();
    }

    public List<CambioDetallado> ObtenerCambiosDetallados(string? valoresAnteriores, string? valoresNuevos)
    {
        var cambios = new List<CambioDetallado>();

        try
        {
            if (string.IsNullOrWhiteSpace(valoresAnteriores) && string.IsNullOrWhiteSpace(valoresNuevos))
            {
                return cambios;
            }

            var objetoAnterior = string.IsNullOrWhiteSpace(valoresAnteriores) 
                ? new Dictionary<string, JsonElement>() 
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(valoresAnteriores) ?? new Dictionary<string, JsonElement>();

            var objetoNuevo = string.IsNullOrWhiteSpace(valoresNuevos) 
                ? new Dictionary<string, JsonElement>() 
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(valoresNuevos) ?? new Dictionary<string, JsonElement>();

            // Obtener todas las claves únicas
            var todasLasClaves = objetoAnterior.Keys.Union(objetoNuevo.Keys).Distinct();

            foreach (var clave in todasLasClaves)
            {
                var valorAnterior = objetoAnterior.ContainsKey(clave) 
                    ? FormatearValor(clave, objetoAnterior[clave]) 
                    : "(sin valor)";

                var valorNuevo = objetoNuevo.ContainsKey(clave) 
                    ? FormatearValor(clave, objetoNuevo[clave]) 
                    : "(sin valor)";

                // Solo agregar si hay cambio
                if (valorAnterior != valorNuevo)
                {
                    var tipoCambio = DeterminarTipoCambio(valorAnterior, valorNuevo);

                    cambios.Add(new CambioDetallado
                    {
                        NombreCampo = clave,
                        NombreAmigable = _nombresAmigables.ContainsKey(clave) ? _nombresAmigables[clave] : clave,
                        ValorAnterior = valorAnterior,
                        ValorNuevo = valorNuevo,
                        TipoCambio = tipoCambio,
                        Descripcion = GenerarDescripcionCambio(
                            _nombresAmigables.ContainsKey(clave) ? _nombresAmigables[clave] : clave,
                            valorAnterior, 
                            valorNuevo, 
                            tipoCambio)
                    });
                }
            }
        }
        catch (JsonException)
        {
            // Si no se puede parsear JSON, retornar cambio genérico
            cambios.Add(new CambioDetallado
            {
                NombreCampo = "Cambio",
                NombreAmigable = "Modificación",
                ValorAnterior = valoresAnteriores ?? "(vacío)",
                ValorNuevo = valoresNuevos ?? "(vacío)"
            });
        }

        return cambios;
    }

    private string FormatearValor(string nombreCampo, JsonElement valor)
    {
        try
        {
            // Manejar nulos
            if (valor.ValueKind == JsonValueKind.Null)
            {
                return "(sin valor)";
            }

            // Fechas
            if (nombreCampo.Contains("Fecha") && valor.ValueKind == JsonValueKind.String)
            {
                if (DateTime.TryParse(valor.GetString(), out var fecha))
                {
                    return fecha.ToString("dd/MM/yyyy HH:mm");
                }
            }

            // Booleanos
            if (valor.ValueKind == JsonValueKind.True)
            {
                return "Sí";
            }
            if (valor.ValueKind == JsonValueKind.False)
            {
                return "No";
            }

            // Estados
            if (nombreCampo == "Estado" && valor.ValueKind == JsonValueKind.Number)
            {
                var estadoValue = valor.GetInt32();
                return estadoValue == 0 ? "Activo" : "Retirado";
            }

            // Tipo de Desvinculación
            if (nombreCampo == "TipoDesvinculacion" && valor.ValueKind == JsonValueKind.Number)
            {
                var tipoValue = valor.GetInt32();
                return tipoValue switch
                {
                    0 => "Despido",
                    1 => "Renuncia Voluntaria",
                    2 => "Fin de Contrato",
                    3 => "Mutuo Acuerdo",
                    4 => "Abandono de Trabajo",
                    5 => "Otros",
                    _ => $"Tipo {tipoValue}"
                };
            }

            // Valor por defecto
            return valor.ValueKind == JsonValueKind.String 
                ? valor.GetString() ?? "(vacío)" 
                : valor.ToString();
        }
        catch
        {
            return valor.ToString();
        }
    }

    private string DeterminarTipoCambio(string valorAnterior, string valorNuevo)
    {
        if (valorAnterior == "(sin valor)" || string.IsNullOrWhiteSpace(valorAnterior))
            return "Agregado";

        if (valorNuevo == "(sin valor)" || string.IsNullOrWhiteSpace(valorNuevo))
            return "Eliminado";

        return "Modificado";
    }

    private string GenerarDescripcionCambio(string campo, string valorAnterior, string valorNuevo, string tipoCambio)
    {
        return tipoCambio switch
        {
            "Agregado" => $"Se agregó {campo}: \"{valorNuevo}\"",
            "Eliminado" => $"Se eliminó {campo}: \"{valorAnterior}\"",
            "Modificado" => $"{campo} cambió de \"{valorAnterior}\" a \"{valorNuevo}\"",
            _ => $"{campo}: {valorAnterior} → {valorNuevo}"
        };
    }
}

public class CambioDetallado
{
    public string NombreCampo { get; set; } = string.Empty;
    public string NombreAmigable { get; set; } = string.Empty;
    public string ValorAnterior { get; set; } = string.Empty;
    public string ValorNuevo { get; set; } = string.Empty;
    public string TipoCambio { get; set; } = string.Empty; // "Agregado", "Eliminado", "Modificado"
    public string Descripcion { get; set; } = string.Empty; // Descripción legible del cambio
}

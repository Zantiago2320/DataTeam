using System.Text.RegularExpressions;

namespace DataTeam.Services;

public interface IFileService
{
    Task<string> GuardarFotoAsync(IFormFile foto, string cedulaConsultor);
    Task EliminarFotoAsync(string rutaFoto);
    string ObtenerRutaFotoPorDefecto();
}

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;
    private readonly string _carpetaFotos = "uploads/fotos";
    private static readonly Regex CedulaValidaRegex = new(@"^[0-9]{6,15}$", RegexOptions.Compiled);

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    public async Task<string> GuardarFotoAsync(IFormFile foto, string cedulaConsultor)
    {
        if (foto == null || foto.Length == 0)
            throw new ArgumentException("El archivo de foto no es válido");

        // SEGURIDAD: Validar cédula contra path traversal
        if (string.IsNullOrWhiteSpace(cedulaConsultor) || !CedulaValidaRegex.IsMatch(cedulaConsultor))
        {
            _logger.LogWarning("Intento de carga de foto con cédula inválida: {Cedula}", cedulaConsultor ?? "null");
            throw new ArgumentException("La cédula del consultor contiene caracteres no válidos");
        }

        // Validar extensión
        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !extensionesPermitidas.Contains(extension))
            throw new ArgumentException("Solo se permiten archivos de imagen (jpg, jpeg, png, gif)");

        // Validar tamaño (máximo 5 MB)
        if (foto.Length > 5 * 1024 * 1024)
            throw new ArgumentException("El archivo no debe superar los 5 MB");

        // Crear carpeta si no existe
        var carpetaCompleta = Path.Combine(_environment.WebRootPath, _carpetaFotos);

        // SEGURIDAD: Verificar que la ruta esté dentro de WebRootPath
        var carpetaCompletaNormalizada = Path.GetFullPath(carpetaCompleta);
        var webRootNormalizado = Path.GetFullPath(_environment.WebRootPath);

        if (!carpetaCompletaNormalizada.StartsWith(webRootNormalizado, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Intento de path traversal detectado: {Path}", carpetaCompleta);
            throw new InvalidOperationException("Ruta de archivo no válida");
        }

        if (!Directory.Exists(carpetaCompleta))
            Directory.CreateDirectory(carpetaCompleta);

        // Generar nombre único con cédula sanitizada
        var nombreArchivo = $"{cedulaConsultor}_{Guid.NewGuid():N}{extension}";
        var rutaCompleta = Path.Combine(carpetaCompleta, nombreArchivo);

        // SEGURIDAD: Verificar nuevamente que la ruta final esté dentro de la carpeta permitida
        var rutaCompletaNormalizada = Path.GetFullPath(rutaCompleta);
        if (!rutaCompletaNormalizada.StartsWith(carpetaCompletaNormalizada, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogError("Intento de path traversal detectado en ruta final: {Path}", rutaCompleta);
            throw new InvalidOperationException("Ruta de archivo no válida");
        }

        // Guardar archivo
        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await foto.CopyToAsync(stream);
        }

        _logger.LogInformation("Foto guardada correctamente para consultor {Cedula}", cedulaConsultor);

        // Retornar ruta relativa
        return $"/{_carpetaFotos}/{nombreArchivo}";
    }

    public async Task EliminarFotoAsync(string rutaFoto)
    {
        if (string.IsNullOrWhiteSpace(rutaFoto) || rutaFoto == ObtenerRutaFotoPorDefecto())
            return;

        // SEGURIDAD: Sanitizar ruta de entrada
        var rutaRelativa = rutaFoto.TrimStart('/').Replace("..", "").Replace("~", "");
        var rutaCompleta = Path.Combine(_environment.WebRootPath, rutaRelativa);

        // SEGURIDAD: Verificar que la ruta esté dentro de WebRootPath
        var rutaCompletaNormalizada = Path.GetFullPath(rutaCompleta);
        var webRootNormalizado = Path.GetFullPath(_environment.WebRootPath);

        if (!rutaCompletaNormalizada.StartsWith(webRootNormalizado, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogWarning("Intento de eliminar archivo fuera de WebRoot: {Path}", rutaFoto);
            return;
        }

        if (File.Exists(rutaCompleta))
        {
            await Task.Run(() => File.Delete(rutaCompleta));
            _logger.LogInformation("Foto eliminada: {Path}", rutaRelativa);
        }
    }

    public string ObtenerRutaFotoPorDefecto()
    {
        return "/images/default-avatar.svg";
    }
}

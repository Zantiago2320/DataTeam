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
    private readonly string _carpetaFotos = "uploads/fotos";

    public FileService(IWebHostEnvironment environment)
    {
        _environment = environment;
    }

    public async Task<string> GuardarFotoAsync(IFormFile foto, string cedulaConsultor)
    {
        if (foto == null || foto.Length == 0)
            throw new ArgumentException("El archivo de foto no es válido");

        // Validar extensión
        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        var extension = Path.GetExtension(foto.FileName).ToLowerInvariant();

        if (!extensionesPermitidas.Contains(extension))
            throw new ArgumentException("Solo se permiten archivos de imagen (jpg, jpeg, png, gif)");

        // Validar tamaño (máximo 5 MB)
        if (foto.Length > 5 * 1024 * 1024)
            throw new ArgumentException("El archivo no debe superar los 5 MB");

        // Crear carpeta si no existe
        var carpetaCompleta = Path.Combine(_environment.WebRootPath, _carpetaFotos);
        if (!Directory.Exists(carpetaCompleta))
            Directory.CreateDirectory(carpetaCompleta);

        // Generar nombre único
        var nombreArchivo = $"{cedulaConsultor}_{Guid.NewGuid()}{extension}";
        var rutaCompleta = Path.Combine(carpetaCompleta, nombreArchivo);

        // Guardar archivo
        using (var stream = new FileStream(rutaCompleta, FileMode.Create))
        {
            await foto.CopyToAsync(stream);
        }

        // Retornar ruta relativa
        return $"/{_carpetaFotos}/{nombreArchivo}";
    }

    public async Task EliminarFotoAsync(string rutaFoto)
    {
        if (string.IsNullOrWhiteSpace(rutaFoto) || rutaFoto == ObtenerRutaFotoPorDefecto())
            return;

        var rutaCompleta = Path.Combine(_environment.WebRootPath, rutaFoto.TrimStart('/'));

        if (File.Exists(rutaCompleta))
        {
            await Task.Run(() => File.Delete(rutaCompleta));
        }
    }

    public string ObtenerRutaFotoPorDefecto()
    {
        return "/images/default-avatar.svg";
    }
}

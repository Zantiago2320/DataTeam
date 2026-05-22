using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using DataTeam.Models;

namespace DataTeam.Services;

public class CsvService : ICsvService
{
    private readonly string _csvPath;
    private readonly string _csvPath2;
    private readonly ILogger<CsvService> _logger;
    private readonly SemaphoreSlim _semaphore = new(1, 1);

    public CsvService(IWebHostEnvironment env, ILogger<CsvService> logger)
    {
        _logger = logger;

        // Prioridad: DateTeam_2026.csv > DATE-TEAM-1.csv > DATE TEAM 1.1.csv
        var csvPath2026 = Path.Combine(env.ContentRootPath, "DateTeam_2026.csv");
        var csvPathAlt1 = Path.Combine(env.ContentRootPath, "DATE-TEAM-1.csv");
        var csvPathAlt2 = Path.Combine(env.ContentRootPath, "DATE TEAM 1.1.csv");

        if (File.Exists(csvPath2026))
        {
            _csvPath = csvPath2026;
            _logger.LogInformation($"✅ Usando archivo: DateTeam_2026.csv");
        }
        else if (File.Exists(csvPathAlt1))
        {
            _csvPath = csvPathAlt1;
            _logger.LogInformation($"✅ Usando archivo: DATE-TEAM-1.csv");
        }
        else
        {
            _csvPath = csvPathAlt2;
            _logger.LogInformation($"✅ Usando archivo: DATE TEAM 1.1.csv");
        }

        _csvPath2 = _csvPath;
    }

    private CsvConfiguration GetConfiguration()
    {
        // Detectar delimitador automáticamente basado en el archivo
        var delimiter = DetectDelimiter();

        return new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            Delimiter = delimiter,
            HasHeaderRecord = true,
            MissingFieldFound = null,
            BadDataFound = null,
            HeaderValidated = null,
            Encoding = Encoding.UTF8,
            TrimOptions = TrimOptions.Trim,
            IgnoreBlankLines = true
        };
    }

    private string DetectDelimiter()
    {
        try
        {
            if (!File.Exists(_csvPath))
                return ",";

            using var reader = new StreamReader(_csvPath, Encoding.UTF8);

            // Leer primera línea que no esté vacía
            string? line = null;
            for (int i = 0; i < 10; i++) // Intentar hasta 10 líneas
            {
                line = reader.ReadLine();
                if (!string.IsNullOrWhiteSpace(line) && (line.Contains(',') || line.Contains(';')))
                    break;
            }

            if (line == null)
                return ",";

            // Contar delimitadores
            int commas = line.Count(c => c == ',');
            int semicolons = line.Count(c => c == ';');

            _logger.LogInformation($"Delimitador detectado: {(semicolons > commas ? "punto y coma (;)" : "coma (,)")} - Comas: {commas}, Punto y coma: {semicolons}");

            return semicolons > commas ? ";" : ",";
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error al detectar delimitador, usando coma por defecto");
            return ",";
        }
    }

    public async Task<List<EmpleadoDataTeam>> LeerEmpleadosAsync()
    {
        await _semaphore.WaitAsync();
        try
        {
            if (!File.Exists(_csvPath))
            {
                _logger.LogWarning($"Archivo CSV no encontrado: {_csvPath}");
                return new List<EmpleadoDataTeam>();
            }

            using var reader = new StreamReader(_csvPath, Encoding.UTF8);

            // Detectar si tiene líneas de metadatos (líneas que no empiezan con comillas o no tienen el delimitador)
            var lineasSaltar = 0;
            var delimiter = DetectDelimiter();

            // Leer primeras líneas para detectar metadata
            var primerasLineas = new List<string>();
            for (int i = 0; i < 10; i++)
            {
                var linea = await reader.ReadLineAsync();
                if (linea != null)
                    primerasLineas.Add(linea);
            }

            // Contar líneas que parecen metadatos (sin delimitadores suficientes o vacías)
            foreach (var linea in primerasLineas)
            {
                var delimitadores = linea.Count(c => c.ToString() == delimiter);

                // Verificar si la línea parece ser un encabezado válido
                // Un encabezado debe tener palabras como "Cédula", "Nombre", "Correo", etc.
                var esEncabezadoValido = linea.Contains("Cédula") || 
                                        linea.Contains("Nombre") || 
                                        linea.Contains("Correo") ||
                                        linea.Contains("Cargo");

                // Si tiene menos de 5 delimitadores O parece ser una fecha/metadata, saltarla
                if (delimitadores < 5 || (!esEncabezadoValido && lineasSaltar == 0))
                    lineasSaltar++;
                else
                    break;
            }

            _logger.LogInformation($"Se detectaron {lineasSaltar} líneas de metadatos a saltar");

            if (lineasSaltar > 0 && primerasLineas.Count > lineasSaltar)
            {
                _logger.LogInformation($"Primera línea saltada: {primerasLineas[0]}");
                _logger.LogInformation($"Línea de encabezado detectada: {primerasLineas[lineasSaltar]}");
            }

            // Reiniciar lectura
            using var reader2 = new StreamReader(_csvPath, Encoding.UTF8);

            // Saltar líneas de metadatos
            for (int i = 0; i < lineasSaltar; i++)
            {
                await reader2.ReadLineAsync();
            }

            using var csv = new CsvReader(reader2, GetConfiguration());

            var records = new List<EmpleadoDataTeam>();

            try
            {
                await foreach (var record in csv.GetRecordsAsync<EmpleadoDataTeam>())
                {
                    // CRÍTICO: Solo agregar registros que tengan AMBOS cédula Y nombre
                    // La cédula es el identificador único y es obligatoria
                    if (!string.IsNullOrWhiteSpace(record.Cedula) && 
                        !string.IsNullOrWhiteSpace(record.Nombre))
                    {
                        // Trim extra de seguridad por si TrimOptions no funcionó
                        record.Cedula = record.Cedula?.Trim();
                        record.Nombre = record.Nombre?.Trim();
                        record.Correo = record.Correo?.Trim();

                        records.Add(record);
                    }
                }
            }
            catch (CsvHelper.HeaderValidationException ex)
            {
                _logger.LogError(ex, "Error de validación de encabezados. Encabezados encontrados: {Headers}", string.Join(", ", ex.InvalidHeaders.Select(h => h.Names.FirstOrDefault() ?? "null")));
                throw new InvalidOperationException($"El archivo CSV no tiene los encabezados esperados. Se encontraron {ex.InvalidHeaders.Length} encabezados incorrectos.", ex);
            }
            catch (CsvHelper.MissingFieldException ex)
            {
                _logger.LogError(ex, "Falta un campo en el CSV. Fila: {Row}, Índice: {Index}", ex.Context?.Parser?.Row ?? -1, ex.Context?.Reader?.CurrentIndex ?? -1);
                throw new InvalidOperationException($"El archivo CSV tiene columnas faltantes en la fila {ex.Context?.Parser?.Row ?? -1}.", ex);
            }
            catch (CsvHelper.ReaderException ex)
            {
                _logger.LogError(ex, "Error al leer el CSV. Fila: {Row}", ex.Context?.Parser?.Row ?? -1);
                throw new InvalidOperationException($"Error al leer el archivo CSV en la fila {ex.Context?.Parser?.Row ?? -1}.", ex);
            }

            _logger.LogInformation($"Se leyeron {records.Count} empleados del CSV");
            return records;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al leer el archivo CSV");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task<EmpleadoDataTeam?> ObtenerEmpleadoPorCedulaAsync(string cedula)
    {
        var empleados = await LeerEmpleadosAsync();
        return empleados.FirstOrDefault(e => e.Cedula == cedula);
    }

    public async Task GuardarEmpleadosAsync(List<EmpleadoDataTeam> empleados)
    {
        await _semaphore.WaitAsync();
        try
        {
            // Leer las primeras 5 líneas originales
            var lineasIniciales = new List<string>();
            if (File.Exists(_csvPath))
            {
                using var reader = new StreamReader(_csvPath, Encoding.UTF8);
                for (int i = 0; i < 5; i++)
                {
                    var linea = await reader.ReadLineAsync();
                    if (linea != null)
                        lineasIniciales.Add(linea);
                }
            }
            else
            {
                // Si no existe, crear líneas vacías
                for (int i = 0; i < 5; i++)
                {
                    lineasIniciales.Add(";;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;");
                }
            }

            // Crear archivo temporal
            var tempPath = Path.GetTempFileName();

            using (var writer = new StreamWriter(tempPath, false, Encoding.UTF8))
            {
                // Escribir las 5 líneas iniciales
                foreach (var linea in lineasIniciales)
                {
                    await writer.WriteLineAsync(linea);
                }

                // Escribir los datos con CsvHelper
                using var csv = new CsvWriter(writer, GetConfiguration());
                await csv.WriteRecordsAsync(empleados);
            }

            // Reemplazar el archivo original
            File.Copy(tempPath, _csvPath, true);
            File.Delete(tempPath);

            _logger.LogInformation($"Se guardaron {empleados.Count} empleados en el CSV");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al guardar el archivo CSV");
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }

    public async Task ActualizarEmpleadoAsync(EmpleadoDataTeam empleadoActualizado)
    {
        var empleados = await LeerEmpleadosAsync();
        var indice = empleados.FindIndex(e => e.Cedula == empleadoActualizado.Cedula);

        if (indice >= 0)
        {
            empleados[indice] = empleadoActualizado;
            await GuardarEmpleadosAsync(empleados);
            _logger.LogInformation($"Empleado {empleadoActualizado.Nombre} (Cédula: {empleadoActualizado.Cedula}) actualizado");
        }
        else
        {
            throw new InvalidOperationException($"No se encontró el empleado con cédula {empleadoActualizado.Cedula}");
        }
    }

    public async Task AgregarEmpleadoAsync(EmpleadoDataTeam nuevoEmpleado)
    {
        var empleados = await LeerEmpleadosAsync();

        // Verificar que no exista ya
        if (empleados.Any(e => e.Cedula == nuevoEmpleado.Cedula))
        {
            throw new InvalidOperationException($"Ya existe un empleado con cédula {nuevoEmpleado.Cedula}");
        }

        empleados.Add(nuevoEmpleado);
        await GuardarEmpleadosAsync(empleados);
        _logger.LogInformation($"Nuevo empleado {nuevoEmpleado.Nombre} (Cédula: {nuevoEmpleado.Cedula}) agregado");
    }

    public async Task<(List<EmpleadoDataTeam> empleados, int total)> ObtenerEmpleadosPaginadosAsync(
        int pagina = 1, 
        int porPagina = 50, 
        string? filtro = null,
        string? celula = null)
    {
        var todosLosEmpleados = await LeerEmpleadosAsync();

        // Aplicar filtro por célula si existe
        // Importante: Un empleado puede estar en varias células separadas por comas o punto y coma
        if (!string.IsNullOrWhiteSpace(celula))
        {
            todosLosEmpleados = todosLosEmpleados.Where(e =>
                !string.IsNullOrWhiteSpace(e.Celula) &&
                (e.Celula.Contains(celula, StringComparison.OrdinalIgnoreCase) ||
                 e.Celula.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Any(c => c.Trim().Equals(celula, StringComparison.OrdinalIgnoreCase)))
            ).ToList();
        }

        // Aplicar filtro general si existe
        if (!string.IsNullOrWhiteSpace(filtro))
        {
            var filtroLower = filtro.ToLower();
            todosLosEmpleados = todosLosEmpleados.Where(e =>
                (e.Nombre?.ToLower().Contains(filtroLower) ?? false) ||
                (e.Cedula?.ToLower().Contains(filtroLower) ?? false) ||
                (e.Correo?.ToLower().Contains(filtroLower) ?? false) ||
                (e.Celula?.ToLower().Contains(filtroLower) ?? false) ||
                (e.CargoOficial?.ToLower().Contains(filtroLower) ?? false)
            ).ToList();
        }

        var total = todosLosEmpleados.Count;
        var empleadosPagina = todosLosEmpleados
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .ToList();

        return (empleadosPagina, total);
    }

    public async Task<List<string>> ObtenerCelulasUnicasAsync()
    {
        var empleados = await LeerEmpleadosAsync();
        var celulasUnicas = new HashSet<string>();

        foreach (var empleado in empleados)
        {
            if (!string.IsNullOrWhiteSpace(empleado.Celula))
            {
                // Separar por coma o punto y coma para manejar empleados en múltiples células
                var celulas = empleado.Celula
                    .Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrWhiteSpace(c));

                foreach (var celula in celulas)
                {
                    celulasUnicas.Add(celula);
                }
            }
        }

        return celulasUnicas.OrderBy(c => c).ToList();
    }
}

using ClosedXML.Excel;
using DataTeam.Data;
using DataTeam.Models;
using Microsoft.EntityFrameworkCore;

namespace DataTeam.Services;

public interface IExcelService
{
    Task<byte[]> ExportarConsultoresAsync();
    Task<byte[]> ExportarConsultoresPorCelulaAsync(int celulaId);
}

public class ExcelService : IExcelService
{
    private readonly ApplicationDbContext _context;

    public ExcelService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<byte[]> ExportarConsultoresAsync()
    {
        var consultores = await _context.Consultores
            .Include(c => c.CelulasMiembro)
                .ThenInclude(cm => cm.Celula)
            .Where(c => c.Estado == EstadoConsultor.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return GenerarExcel(consultores, "DataTeam - Consultores");
    }

    public async Task<byte[]> ExportarConsultoresPorCelulaAsync(int celulaId)
    {
        var celula = await _context.Celulas.FindAsync(celulaId);
        var consultores = await _context.Consultores
            .Include(c => c.CelulasMiembro)
                .ThenInclude(cm => cm.Celula)
            .Where(c => c.CelulasMiembro.Any(cm => cm.CelulaId == celulaId) && c.Estado == EstadoConsultor.Activo)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return GenerarExcel(consultores, $"DataTeam - {celula?.Nombre ?? "Célula"}");
    }

    private byte[] GenerarExcel(List<Consultor> consultores, string nombreHoja)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(nombreHoja);

        // Configurar encabezados con los 10 campos requeridos por TH
        var encabezados = new[]
        {
            "Cédula",
            "Nombre",
            "Cargo",
            "Celular",
            "Correo",
            "Célula",
            "Ciudad",
            "Capacidad dentro del equipo",
            "Rol dentro del equipo",
            "Empresa"
        };

        // Aplicar encabezados
        for (int i = 0; i < encabezados.Length; i++)
        {
            var cell = worksheet.Cell(1, i + 1);
            cell.Value = encabezados[i];
            cell.Style.Font.Bold = true;
            cell.Style.Font.FontSize = 12;
            cell.Style.Font.FontColor = XLColor.White;
            cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#28a745"); // Verde del tema
            cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            cell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }

        // Datos
        int row = 2;
        foreach (var consultor in consultores)
        {
            // Obtener la primera célula asignada (o todas si tiene múltiples)
            var celulas = consultor.CelulasMiembro
                .Select(cm => cm.Celula?.Nombre ?? "")
                .Where(n => !string.IsNullOrEmpty(n))
                .ToList();
            var celulaNombre = celulas.Any() ? string.Join(", ", celulas) : "Sin asignar";

            // Obtener el rol de la primera célula (o múltiples)
            var roles = consultor.CelulasMiembro
                .Select(cm => cm.Rol ?? "")
                .Where(r => !string.IsNullOrEmpty(r))
                .ToList();
            var rolNombre = roles.Any() ? string.Join(", ", roles) : "";

            worksheet.Cell(row, 1).Value = consultor.Cedula;
            worksheet.Cell(row, 2).Value = consultor.Nombre;
            worksheet.Cell(row, 3).Value = consultor.Cargo;
            worksheet.Cell(row, 4).Value = consultor.Celular ?? "";
            worksheet.Cell(row, 5).Value = consultor.Correo;
            worksheet.Cell(row, 6).Value = celulaNombre;
            worksheet.Cell(row, 7).Value = "Bogotá"; // Por defecto, ajustar si tienes campo ciudad
            worksheet.Cell(row, 8).Value = consultor.Capacidad.HasValue ? $"{consultor.Capacidad}%" : "100%";
            worksheet.Cell(row, 9).Value = rolNombre;
            worksheet.Cell(row, 10).Value = consultor.Empresa ?? "";

            // Aplicar bordes y estilo alternado
            var rowRange = worksheet.Range(row, 1, row, 10);
            rowRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            rowRange.Style.Border.InsideBorder = XLBorderStyleValues.Hair;

            // Filas alternadas
            if (row % 2 == 0)
            {
                rowRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa"); // Gris muy claro
            }

            row++;
        }

        // Ajustar ancho de columnas automáticamente
        worksheet.Columns().AdjustToContents();

        // Configurar ancho mínimo y máximo para mejor legibilidad
        foreach (var column in worksheet.ColumnsUsed())
        {
            if (column.Width < 12) column.Width = 12;
            if (column.Width > 50) column.Width = 50;
        }

        // Habilitar filtros automáticos
        worksheet.RangeUsed().SetAutoFilter();

        // Congelar la primera fila (encabezados)
        worksheet.SheetView.FreezeRows(1);

        // Guardar en memoria
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

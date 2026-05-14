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
            .Include(c => c.Celula)
            .OrderBy(c => c.Celula!.Nombre)
            .ThenBy(c => c.Nombre)
            .ToListAsync();

        return GenerarExcel(consultores, "Todos los Consultores");
    }

    public async Task<byte[]> ExportarConsultoresPorCelulaAsync(int celulaId)
    {
        var celula = await _context.Celulas.FindAsync(celulaId);
        var consultores = await _context.Consultores
            .Include(c => c.Celula)
            .Where(c => c.CelulaId == celulaId)
            .OrderBy(c => c.Nombre)
            .ToListAsync();

        return GenerarExcel(consultores, $"Consultores - {celula?.Nombre ?? "Célula"}");
    }

    private byte[] GenerarExcel(List<Consultor> consultores, string nombreHoja)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(nombreHoja);

        // Encabezados
        worksheet.Cell(1, 1).Value = "Cédula";
        worksheet.Cell(1, 2).Value = "Nombre";
        worksheet.Cell(1, 3).Value = "Correo";
        worksheet.Cell(1, 4).Value = "Cargo";
        worksheet.Cell(1, 5).Value = "Célula";
        worksheet.Cell(1, 6).Value = "Rol";
        worksheet.Cell(1, 7).Value = "Capacidad (%)";
        worksheet.Cell(1, 8).Value = "Empresa";
        worksheet.Cell(1, 9).Value = "Fecha Ingreso";
        worksheet.Cell(1, 10).Value = "Fecha Nacimiento";
        worksheet.Cell(1, 11).Value = "Edad";
        worksheet.Cell(1, 12).Value = "Dirección";
        worksheet.Cell(1, 13).Value = "Barrio";
        worksheet.Cell(1, 14).Value = "Celular";
        worksheet.Cell(1, 15).Value = "Contacto Emergencia";
        worksheet.Cell(1, 16).Value = "Estado";

        // Aplicar estilo a encabezados
        var headerRange = worksheet.Range(1, 1, 1, 16);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

        // Datos
        int row = 2;
        foreach (var consultor in consultores)
        {
            worksheet.Cell(row, 1).Value = consultor.Cedula;
            worksheet.Cell(row, 2).Value = consultor.Nombre;
            worksheet.Cell(row, 3).Value = consultor.Correo;
            worksheet.Cell(row, 4).Value = consultor.Cargo;
            worksheet.Cell(row, 5).Value = consultor.Celula?.Nombre ?? "";
            worksheet.Cell(row, 6).Value = consultor.Rol ?? "";
            worksheet.Cell(row, 7).Value = consultor.Capacidad?.ToString() ?? "";
            worksheet.Cell(row, 8).Value = consultor.Empresa ?? "";
            worksheet.Cell(row, 9).Value = consultor.FechaIngreso.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 10).Value = consultor.FechaNacimiento.ToString("dd/MM/yyyy");
            worksheet.Cell(row, 11).Value = DateTime.Today.Year - consultor.FechaNacimiento.Year;
            worksheet.Cell(row, 12).Value = consultor.Direccion ?? "";
            worksheet.Cell(row, 13).Value = consultor.Barrio ?? "";
            worksheet.Cell(row, 14).Value = consultor.Celular ?? "";
            worksheet.Cell(row, 15).Value = consultor.ContactoEmergencia ?? "";
            worksheet.Cell(row, 16).Value = consultor.Estado.ToString();

            // Colorear según estado
            if (consultor.Estado == EstadoConsultor.Retirado)
            {
                worksheet.Range(row, 1, row, 16).Style.Fill.BackgroundColor = XLColor.LightGray;
            }

            row++;
        }

        // Ajustar ancho de columnas
        worksheet.Columns().AdjustToContents();

        // Guardar en memoria
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}

using CsvHelper.Configuration.Attributes;

namespace DataTeam.Models;

public class EmpleadoDataTeam
{
    [Index(0)]
    [Name("Cédula")]
    public string? Cedula { get; set; }

    [Index(1)]
    [Name("Nombre")]
    public string? Nombre { get; set; }

    [Index(2)]
    [Name("Correo")]
    public string? Correo { get; set; }

    [Index(3)]
    [Name("Nombre del Cargo Oficial")]
    public string? CargoOficial { get; set; }

    [Index(4)]
    [Name("Desarrollo")]
    public string? Desarrollo { get; set; }

    [Index(5)]
    [Name("Rol")]
    public string? Rol { get; set; }

    [Index(6)]
    [Name("% participación")]
    public string? PorcentajeParticipacion { get; set; }

    [Index(7)]
    [Name("Célula")]
    public string? Celula { get; set; }

    [Index(8)]
    [Name("Lider")]
    public string? Lider { get; set; }

    [Index(9)]
    [Name("Empresa")]
    public string? Empresa { get; set; }

    [Index(10)]
    [Name("Udemy")]
    public string? Udemy { get; set; }

    [Index(11)]
    [Name("ARL")]
    public string? ARL { get; set; }

    [Index(12)]
    [Name("Ciudad")]
    public string? Ciudad { get; set; }

    [Index(13)]
    [Name("FECHA DE CUMPLEAÑOS")]
    public string? FechaCumpleanos { get; set; }

    [Index(14)]
    [Name("Mes cumple")]
    public string? MesCumple { get; set; }

    [Index(15)]
    [Name("Dirección")]
    public string? Direccion { get; set; }

    [Index(16)]
    [Name("Barrio")]
    public string? Barrio { get; set; }

    [Index(17)]
    [Name("Telefono Fijo")]
    public string? TelefonoFijo { get; set; }

    [Index(18)]
    [Name("Tel Celular")]
    public string? TelCelular { get; set; }

    [Index(19)]
    [Name("Contacto adicional")]
    public string? ContactoAdicional { get; set; }

    [Index(20)]
    [Name("Nº contacto adicional")]
    public string? NumeroContactoAdicional { get; set; }

    [Index(21)]
    [Name("Fecha Ingreso")]
    public string? FechaIngreso { get; set; }

    [Index(22)]
    [Name("Nº Renovaciones")]
    public string? NumeroRenovaciones { get; set; }

    [Index(23)]
    [Name("Fecha de renovación Actual")]
    public string? FechaRenovacionActual { get; set; }

    [Index(24)]
    [Name("Fecha Vto Contrato")]
    public string? FechaVtoContrato { get; set; }

    [Index(25)]
    [Name("Inducción")]
    public string? Induccion { get; set; }

    [Index(26)]
    [Name("Plan de entrenamiento")]
    public string? PlanEntrenamiento { get; set; }

    [Index(27)]
    [Name("Fecha Vto PP")]
    public string? FechaVtoPP { get; set; }

    [Index(28)]
    [Name("PP")]
    public string? PP { get; set; }

    [Index(29)]
    [Name("Visual")]
    public string? Visual { get; set; }

    [Index(30)]
    [Name("Estado")]
    public string? Estado { get; set; }

    [Index(31)]
    [Name("VAC - Inicio")]
    public string? VacInicio { get; set; }

    [Index(32)]
    [Name("VAC - Final")]
    public string? VacFinal { get; set; }

    [Index(33)]
    [Name("VAC - Reintegro")]
    public string? VacReintegro { get; set; }

    [Index(34)]
    [Name("Mes vacaciones")]
    public string? MesVacaciones { get; set; }

    [Index(35)]
    [Name("Saldo de vacaciones para 2025")]
    public string? SaldoVacaciones2025 { get; set; }

    [Index(36)]
    [Name("Días tomados 2025")]
    public string? DiasTomados2025 { get; set; }

    [Index(37)]
    [Name("Días pte por disfrutar 2025")]
    public string? DiasPendientes2025 { get; set; }

    [Index(38)]
    [Name("Horario de Trabajo")]
    public string? HorarioTrabajo { get; set; }

    [Index(39)]
    [Name("Observaciones")]
    public string? Observaciones { get; set; }

    // Propiedad de ayuda para identificación única
    public string GetIdentifier() => $"{Cedula}_{Nombre}";
}

# 🚀 REFERENCIA RÁPIDA: TRABAJAR CON EQUIPOS Y CÉLULAS

## 📋 PARA DESARROLLADORES

Esta guía rápida muestra cómo trabajar con el sistema de equipos y células en el código.

---

## 🎯 MODELO DE DATOS

### Modelo Simple (Actual)

```
Consultor
├─ EquipoId (FK) → Un equipo principal
└─ CelulaId (FK) → Una célula

Equipo
├─ EquipoLideres (N:N) → Múltiples líderes
└─ Consultores (1:N) → Miembros

Celula
├─ CelulaLideres (N:N) → Múltiples líderes
└─ Consultores (1:N) → Miembros
```

### Modelo Flexible (Opcional con EquipoMiembro)

```
Consultor
└─ CelulaId (FK) → Una célula

EquipoMiembro (N:N)
├─ EquipoId (FK)
├─ ConsultorId (FK)
├─ PorcentajeParticipacion (int)
└─ EsMiembroPrincipal (bool)
```

---

## 💻 QUERIES COMUNES

### 1. Obtener Consultores de un Equipo

#### Modelo Simple

```csharp
// En Controller o Service
var equipo = await _context.Equipos
	.Include(e => e.Consultores)
	.FirstOrDefaultAsync(e => e.Id == equipoId);

var miembros = equipo.Consultores
	.Where(c => c.Estado == "Activo")
	.ToList();
```

#### Modelo Flexible

```csharp
var miembros = await _context.EquipoMiembros
	.Include(em => em.Consultor)
	.Where(em => em.EquipoId == equipoId && em.Activo)
	.Select(em => new {
		em.Consultor,
		em.PorcentajeParticipacion,
		em.EsMiembroPrincipal
	})
	.ToListAsync();
```

---

### 2. Obtener Equipos de un Consultor

#### Modelo Simple

```csharp
var consultor = await _context.Consultores
	.Include(c => c.Equipo)
	.FirstOrDefaultAsync(c => c.Id == consultorId);

var equipoPrincipal = consultor.Equipo;
```

#### Modelo Flexible

```csharp
var equipos = await _context.EquipoMiembros
	.Include(em => em.Equipo)
	.Where(em => em.ConsultorId == consultorId && em.Activo)
	.OrderByDescending(em => em.EsMiembroPrincipal)
	.ThenByDescending(em => em.PorcentajeParticipacion)
	.Select(em => new {
		em.Equipo,
		em.PorcentajeParticipacion,
		EsPrincipal = em.EsMiembroPrincipal
	})
	.ToListAsync();
```

---

### 3. Obtener Líderes de un Equipo

```csharp
var lideres = await _context.EquipoLideres
	.Include(el => el.Consultor)
	.Where(el => el.EquipoId == equipoId)
	.OrderByDescending(el => el.EsLiderPrincipal)
	.Select(el => el.Consultor)
	.ToListAsync();

// Solo el líder principal
var liderPrincipal = await _context.EquipoLideres
	.Include(el => el.Consultor)
	.Where(el => el.EquipoId == equipoId && el.EsLiderPrincipal)
	.Select(el => el.Consultor)
	.FirstOrDefaultAsync();
```

---

### 4. Asignar Consultor a Equipo

#### Modelo Simple

```csharp
// En Service o Controller
var consultor = await _context.Consultores.FindAsync(consultorId);
if (consultor != null)
{
	consultor.EquipoId = equipoId;
	await _context.SaveChangesAsync();
}
```

#### Modelo Flexible

```csharp
// Validar que la suma de porcentajes no exceda 100%
var porcentajeActual = await _context.EquipoMiembros
	.Where(em => em.ConsultorId == consultorId && em.Activo)
	.SumAsync(em => em.PorcentajeParticipacion);

if (porcentajeActual + porcentajeNuevo > 100)
{
	throw new InvalidOperationException(
		$"El consultor ya tiene {porcentajeActual}% asignado. No se puede asignar {porcentajeNuevo}% adicional.");
}

// Crear asignación
var miembro = new EquipoMiembro
{
	EquipoId = equipoId,
	ConsultorId = consultorId,
	PorcentajeParticipacion = porcentajeNuevo,
	EsMiembroPrincipal = porcentajeActual == 0, // Si es el primero, es principal
	FechaAsignacion = DateTime.UtcNow,
	Activo = true
};

_context.EquipoMiembros.Add(miembro);
await _context.SaveChangesAsync();
```

---

### 5. Desasignar Consultor de Equipo

#### Modelo Simple

```csharp
var consultor = await _context.Consultores.FindAsync(consultorId);
if (consultor != null)
{
	consultor.EquipoId = null; // O asignar a "Sin Asignar"
	await _context.SaveChangesAsync();
}
```

#### Modelo Flexible

```csharp
// Soft delete
var miembro = await _context.EquipoMiembros
	.FirstOrDefaultAsync(em => 
		em.EquipoId == equipoId && 
		em.ConsultorId == consultorId && 
		em.Activo);

if (miembro != null)
{
	miembro.Activo = false;
	miembro.FechaDesasignacion = DateTime.UtcNow;
	await _context.SaveChangesAsync();
}

// Hard delete (no recomendado)
_context.EquipoMiembros.Remove(miembro);
await _context.SaveChangesAsync();
```

---

### 6. Cambiar Equipo Principal de un Consultor (Modelo Flexible)

```csharp
// Quitar marca de principal del equipo actual
var miembroActual = await _context.EquipoMiembros
	.FirstOrDefaultAsync(em => 
		em.ConsultorId == consultorId && 
		em.EsMiembroPrincipal && 
		em.Activo);

if (miembroActual != null)
{
	miembroActual.EsMiembroPrincipal = false;
}

// Marcar nuevo equipo como principal
var nuevoMiembro = await _context.EquipoMiembros
	.FirstOrDefaultAsync(em => 
		em.ConsultorId == consultorId && 
		em.EquipoId == nuevoEquipoId && 
		em.Activo);

if (nuevoMiembro != null)
{
	nuevoMiembro.EsMiembroPrincipal = true;
	await _context.SaveChangesAsync();
}
```

---

## 📊 REPORTES Y ESTADÍSTICAS

### 1. Top 5 Equipos Más Grandes

```csharp
var topEquipos = await _context.Equipos
	.Include(e => e.Consultores)
	.Where(e => e.Activo)
	.Select(e => new
	{
		e.Nombre,
		CantidadMiembros = e.Consultores.Count(c => c.Estado == "Activo"),
		Miembros = e.Consultores
			.Where(c => c.Estado == "Activo")
			.Select(c => c.Nombre)
			.ToList()
	})
	.OrderByDescending(e => e.CantidadMiembros)
	.Take(5)
	.ToListAsync();
```

---

### 2. Consultores Sin Equipo

```csharp
var sinEquipo = await _context.Consultores
	.Where(c => c.EquipoId == null && c.Estado == "Activo")
	.Select(c => new { c.Cedula, c.Nombre, c.Correo })
	.ToListAsync();
```

---

### 3. Equipos Sin Líder

```csharp
var sinLider = await _context.Equipos
	.Where(e => e.Activo && !e.EquipoLideres.Any())
	.Select(e => new { e.Nombre, e.Descripcion })
	.ToListAsync();
```

---

### 4. Distribución de Consultores por Equipo (Modelo Flexible)

```csharp
var distribucion = await _context.EquipoMiembros
	.Include(em => em.Equipo)
	.Include(em => em.Consultor)
	.Where(em => em.Activo)
	.GroupBy(em => em.Equipo.Nombre)
	.Select(g => new
	{
		Equipo = g.Key,
		CantidadMiembros = g.Count(),
		PromedioParticipacion = g.Average(em => em.PorcentajeParticipacion),
		Miembros = g.Select(em => new
		{
			em.Consultor.Nombre,
			em.PorcentajeParticipacion
		}).ToList()
	})
	.OrderByDescending(e => e.CantidadMiembros)
	.ToListAsync();
```

---

## 🎨 VISTAS RAZOR

### 1. Mostrar Equipo del Consultor

```cshtml
@model Consultor

<div class="card">
	<div class="card-body">
		<h5>@Model.Nombre</h5>
		@if (Model.Equipo != null)
		{
			<span class="badge" style="background-color: @Model.Equipo.Color">
				@Model.Equipo.Nombre
			</span>
		}
		else
		{
			<span class="badge bg-secondary">Sin Equipo</span>
		}
	</div>
</div>
```

---

### 2. Listar Miembros de un Equipo

```cshtml
@model Equipo

<h3>@Model.Nombre</h3>
<div class="list-group">
	@foreach (var consultor in Model.Consultores.Where(c => c.Estado == "Activo"))
	{
		<div class="list-group-item">
			<div class="d-flex justify-content-between">
				<span>@consultor.Nombre</span>
				<span class="text-muted">@consultor.Cargo</span>
			</div>
		</div>
	}
</div>
```

---

### 3. Listar Líderes de un Equipo

```cshtml
@model Equipo

<h4>Líderes</h4>
<ul class="list-unstyled">
	@foreach (var lider in Model.EquipoLideres.OrderByDescending(el => el.EsLiderPrincipal))
	{
		<li>
			@lider.Consultor.Nombre
			@if (lider.EsLiderPrincipal)
			{
				<span class="badge bg-primary">Principal</span>
			}
		</li>
	}
</ul>
```

---

## 🛡️ VALIDACIONES RECOMENDADAS

### 1. Validar % Participación Total

```csharp
public async Task<bool> ValidarPorcentajeTotal(int consultorId)
{
	var totalPorcentaje = await _context.EquipoMiembros
		.Where(em => em.ConsultorId == consultorId && em.Activo)
		.SumAsync(em => em.PorcentajeParticipacion);

	return totalPorcentaje == 100;
}
```

---

### 2. Evitar Duplicados al Asignar

```csharp
public async Task<bool> YaEsMiembro(int equipoId, int consultorId)
{
	return await _context.EquipoMiembros
		.AnyAsync(em => 
			em.EquipoId == equipoId && 
			em.ConsultorId == consultorId && 
			em.Activo);
}
```

---

### 3. Validar Solo Un Líder Principal

```csharp
public async Task<bool> TieneLiderPrincipal(int equipoId)
{
	return await _context.EquipoLideres
		.CountAsync(el => el.EquipoId == equipoId && el.EsLiderPrincipal) == 1;
}
```

---

## 🔧 SERVICIOS RECOMENDADOS

### IEquipoService (Ya existe)

```csharp
public interface IEquipoService
{
	Task<IEnumerable<Equipo>> ObtenerTodosAsync();
	Task<Equipo?> ObtenerPorIdAsync(int id);
	Task CrearAsync(Equipo equipo);
	Task ActualizarAsync(Equipo equipo);
	Task CambiarEstadoAsync(int id, bool activo);
	Task AsignarLiderAsync(int equipoId, int consultorId, bool esPrincipal);
	Task RemoverLiderAsync(int equipoId, int consultorId);
	Task AsignarMiembroAsync(int equipoId, int consultorId);
	Task RemoverMiembroAsync(int equipoId, int consultorId);
}
```

---

### IEquipoMiembroService (Nuevo para modelo flexible)

```csharp
public interface IEquipoMiembroService
{
	Task<IEnumerable<EquipoMiembro>> ObtenerMiembrosEquipoAsync(int equipoId);
	Task<IEnumerable<EquipoMiembro>> ObtenerEquiposConsultorAsync(int consultorId);
	Task AsignarAsync(int equipoId, int consultorId, int porcentaje, bool esPrincipal = false);
	Task DesasignarAsync(int equipoId, int consultorId);
	Task CambiarPorcentajeAsync(int equipoId, int consultorId, int nuevoPorcentaje);
	Task CambiarEquipoPrincipalAsync(int consultorId, int nuevoEquipoId);
	Task<bool> ValidarPorcentajeTotalAsync(int consultorId);
	Task<int> ObtenerPorcentajeDisponibleAsync(int consultorId);
}
```

---

## 📝 EJEMPLO COMPLETO: ASIGNAR CONSULTOR CON VALIDACIONES

```csharp
public async Task<IActionResult> AsignarMiembro(int equipoId, int consultorId, int porcentaje = 100)
{
	try
	{
		// Validar que el equipo existe
		var equipo = await _context.Equipos.FindAsync(equipoId);
		if (equipo == null)
			return NotFound("Equipo no encontrado");

		// Validar que el consultor existe
		var consultor = await _context.Consultores.FindAsync(consultorId);
		if (consultor == null)
			return NotFound("Consultor no encontrado");

		// Validar que no esté ya asignado
		var yaAsignado = await _context.EquipoMiembros
			.AnyAsync(em => em.EquipoId == equipoId && em.ConsultorId == consultorId && em.Activo);

		if (yaAsignado)
			return BadRequest("El consultor ya pertenece a este equipo");

		// Validar porcentaje disponible
		var porcentajeActual = await _context.EquipoMiembros
			.Where(em => em.ConsultorId == consultorId && em.Activo)
			.SumAsync(em => em.PorcentajeParticipacion);

		if (porcentajeActual + porcentaje > 100)
			return BadRequest($"Porcentaje no disponible. Actual: {porcentajeActual}%, Disponible: {100 - porcentajeActual}%");

		// Crear asignación
		var miembro = new EquipoMiembro
		{
			EquipoId = equipoId,
			ConsultorId = consultorId,
			PorcentajeParticipacion = porcentaje,
			EsMiembroPrincipal = porcentajeActual == 0, // Si es el primero, es principal
			FechaAsignacion = DateTime.UtcNow,
			Activo = true
		};

		_context.EquipoMiembros.Add(miembro);
		await _context.SaveChangesAsync();

		// Log de auditoría
		_logger.LogInformation(
			"Consultor {ConsultorId} asignado al equipo {EquipoId} con {Porcentaje}% de participación",
			consultorId, equipoId, porcentaje);

		return Ok(new { 
			mensaje = "Consultor asignado exitosamente",
			porcentajeRestante = 100 - porcentajeActual - porcentaje
		});
	}
	catch (Exception ex)
	{
		_logger.LogError(ex, "Error al asignar consultor {ConsultorId} al equipo {EquipoId}", consultorId, equipoId);
		return StatusCode(500, "Error interno del servidor");
	}
}
```

---

## 🎯 MEJORES PRÁCTICAS

### ✅ DO

- Usar Include() para cargar navegaciones necesarias
- Validar porcentajes antes de asignar (modelo flexible)
- Usar transacciones para operaciones múltiples
- Loggear cambios importantes en equipos/asignaciones
- Usar soft delete para mantener historial
- Cachear equipos/células si son consultados frecuentemente

### ❌ DON'T

- No cargar todas las navegaciones si no las necesitas (N+1 problem)
- No permitir que un consultor tenga > 100% de participación
- No hacer hard delete de asignaciones (perderás historial)
- No olvidar actualizar `FechaModificacion` al editar
- No permitir equipos sin líder en producción
- No permitir consultores activos sin equipo

---

## 📚 RECURSOS ADICIONALES

- [Documentación EF Core](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Best Practices](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/best-practices)
- [LINQ Query Examples](https://learn.microsoft.com/en-us/dotnet/csharp/linq/)

---

**📅 Última actualización:** 2025-01  
**👤 Autor:** GitHub Copilot  
**🔗 Ver también:** `ANALISIS-ASIGNACION-EQUIPOS.md`, `GUIA-EJECUCION-MIGRACION.md`

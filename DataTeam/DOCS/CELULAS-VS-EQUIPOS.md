# 📘 ENTENDIENDO CÉLULAS VS EQUIPOS

## 🎯 DIFERENCIA CLAVE

En este proyecto, **Célula** y **Equipo** representan conceptos diferentes:

### 🔵 CÉLULA (Cell)
- **Definición**: Unidad organizacional principal del consultor
- **Origen**: Campo "Célula" en el CSV proporcionado por Talento Humano
- **Uso**: Organización funcional, reporting, asignación de proyectos
- **Campo**: `Consultor.CelulaId` (obligatorio)
- **Relación**: Un consultor pertenece a **UNA** célula principal

### 🟢 EQUIPO (Team)
- **Definición**: Agrupación opcional/adicional para proyectos específicos
- **Origen**: Campo opcional para organización interna de TI
- **Uso**: Proyectos temporales, asignaciones flexibles, matrices
- **Campo**: `Consultor.EquipoId` (opcional)
- **Relación**: Un consultor puede tener un equipo adicional

---

## 📊 ESTRUCTURA ACTUAL DEL MODELO

```
Consultor
├─ CelulaId (int, obligatorio) → Célula principal del consultor
├─ EquipoId (int?, opcional) → Equipo opcional/proyecto
└─ Rol (string) → Rol funcional (Ingeniero, QA, PO Técnico, etc.)

Celula
├─ CelulaLideres (N:N) → Múltiples líderes posibles
└─ Consultores (1:N) → Miembros de la célula

Equipo
├─ EquipoLideres (N:N) → Múltiples líderes posibles
└─ Consultores (1:N) → Miembros del equipo
```

---

## 🗂️ CÉLULAS IDENTIFICADAS EN EL CSV

Basado en el CSV proporcionado, estas son las células activas:

| Célula | Descripción | Líderes Principales | Total Consultores |
|--------|-------------|---------------------|-------------------|
| **Enterprise Team** | Desarrollo empresarial | Alexander Castro, Jennifer Toro, Ingrid Porras | ~11 |
| **Nova** | Innovación y nuevas tecnologías | Alexander Castro, Jennifer Toro, Victor Martinez | ~12 |
| **Bon Voyage** | Soluciones de viaje | Alexander Castro, Cristhian Amezquita, Diego Montenegro | ~12 |
| **MindShift** | Transformación digital | Alexander Castro, Cristhian Amezquita, Victor Martinez | ~12 |
| **Wakanda** | Desarrollo avanzado | Alexander Castro, Jennifer Toro, Maria Camila Redondo | ~10 |
| **DEVSECOPS** | Seguridad y operaciones | Cristhian Amezquita, Robert Ramirez, Danna Ordoñez | ~9 |
| **Data Stargazers** | Datos y analytics | Alexander Castro, Diego Montenegro, Cristhian Amezquita | ~7 |
| **Maya** | Plataformas de desarrollo | Alexander Castro, Jennifer Toro, Victor Martinez | ~10 |
| **Aurora** | Aplicaciones | Alexander Castro, Robert Ramirez | ~3 |
| **Polaris Software Team** | Software | Alexander Castro, Jennifer Toro, Mauricio Mejia | ~12 |
| **Seguridad** | Seguridad especializada | Alexander Castro | ~1 |
| **Administrativo** | Administrativo | Alexander Castro | ~1 |
| **Transversal Calidad** | Calidad transversal | Cristhian Amezquita | ~3 |
| **Direccion Desarrollo** | Dirección de desarrollo | Alexander Castro, Robert Ramirez | ~6 |
| **Facturador** | Facturación electrónica | Victor Martinez | ~2 |

---

## 👥 CASOS ESPECIALES: CONSULTORES EN MÚLTIPLES CÉLULAS

Según el CSV, estos consultores aparecen en **dos células diferentes**:

| Cédula | Nombre | Células | Solución Aplicada |
|--------|--------|---------|-------------------|
| 779729 | Cecilio Trinidad | Enterprise Team + Wakanda | CelulaId = Enterprise Team (primer registro) |
| 79568718 | Hugo Bermudez | Enterprise Team + Wakanda | CelulaId = Enterprise Team (primer registro) |
| 1072666410 | Esneider Gualtero | Polaris + Maya | CelulaId = Polaris (primer registro) |
| 53006451 | Diana Saavedra | Enterprise Team + Wakanda | CelulaId = Enterprise Team (primer registro) |
| 80127568 | Robert Ramirez | DEVSECOPS + Aurora + Nova | CelulaId = DEVSECOPS (primer registro) |
| 80088963 | Diego Montenegro | Bon Voyage + Data Stargazers | CelulaId = Bon Voyage (primer registro) |
| 1019075102 | Alejandra Jimenez | MindShift + Data Stargazers | CelulaId = MindShift (primer registro) |
| 80826699 | Andres Rojas | Maya + Polaris | CelulaId = Maya (primer registro) |

### ⚙️ ¿Cómo se manejaron estos casos?

**Opción actual (Modelo Simple):**
- El script SQL asigna **CelulaId** al **primer registro** encontrado en el CSV
- La célula secundaria queda registrada pero no asignada

**Opción futura (Modelo Flexible):**
- Usar tabla `EquipoMiembro` o equivalente para soportar múltiples células con % participación
- Ver: `MigracionEquipoMiembroFlexible.sql` (adaptable a células)

---

## 🚀 EJECUCIÓN DE LA MIGRACIÓN

### Pre-requisitos

```sql
-- Crear backup completo
BACKUP DATABASE DataTeamDB
TO DISK = 'C:\Backups\DataTeamDB_PreMigracionCelulas.bak'
WITH INIT, NAME = 'Full Database Backup Before Cells Migration';
```

### Ejecutar Script

#### Desde SQL Server Management Studio (SSMS)

1. Abrir `DataTeam/Data/Scripts/MigracionCelulas.sql`
2. Conectar a la base de datos `DataTeamDB`
3. Ejecutar (`F5`)
4. Revisar mensajes de salida

#### Desde PowerShell

```powershell
cd C:\Users\USER\OneDrive\Desktop\proyectos\DataTeam

sqlcmd -S localhost -d DataTeamDB `
  -i "DataTeam\Data\Scripts\MigracionCelulas.sql" `
  -o "DataTeam\Data\Scripts\Logs\MigracionCelulas_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
```

---

## ✅ VALIDACIONES POST-MIGRACIÓN

### 1. Consultores Sin Célula

```sql
SELECT Cedula, Nombre, Estado
FROM Consultores
WHERE CelulaId IS NULL AND Estado = 'Activo';
```

**✅ Resultado esperado:** 0 rows (o solo consultores inactivos)

---

### 2. Células Sin Líder

```sql
SELECT c.Nombre, c.Descripcion
FROM Celulas c
LEFT JOIN CelulaLider cl ON c.Id = cl.CelulaId
WHERE cl.CelulaId IS NULL AND c.Activa = 1 AND c.Nombre != 'Sin Asignar';
```

**✅ Resultado esperado:** 0 rows (todas las células tienen líder)

---

### 3. Distribución por Célula

```sql
SELECT 
	c.Nombre AS Celula,
	COUNT(co.Id) AS TotalMiembros,
	COUNT(CASE WHEN co.Estado = 'Activo' THEN 1 END) AS Activos,
	COUNT(CASE WHEN co.Estado = 'Inactivo' THEN 1 END) AS Inactivos
FROM Celulas c
LEFT JOIN Consultores co ON co.CelulaId = c.Id
WHERE c.Activa = 1
GROUP BY c.Id, c.Nombre
ORDER BY TotalMiembros DESC;
```

**✅ Resultado esperado:**
- Enterprise Team: ~11 consultores
- MindShift: ~12 consultores
- Nova: ~12 consultores
- Bon Voyage: ~12 consultores
- Polaris: ~12 consultores
- etc.

---

### 4. Líderes por Célula

```sql
SELECT 
	c.Nombre AS Celula,
	co.Nombre AS Lider,
	co.Cedula,
	CASE WHEN cl.EsLiderPrincipal = 1 THEN 'Principal' ELSE 'Secundario' END AS TipoLider
FROM Celulas c
INNER JOIN CelulaLider cl ON c.Id = cl.CelulaId
INNER JOIN Consultores co ON cl.ConsultorId = co.Id
WHERE c.Activa = 1
ORDER BY c.Nombre, cl.EsLiderPrincipal DESC;
```

**✅ Resultado esperado:**
- Cada célula debe tener al menos 1 líder
- Alexander Castro debe aparecer en ~14 células
- Cristhian Amezquita en ~11 células
- Jennifer Toro en ~8 células

---

## 🔄 CONSULTAS ÚTILES

### Ver Consultor con Su Célula

```csharp
var consultor = await _context.Consultores
	.Include(c => c.Celula)
	.ThenInclude(cel => cel.CelulaLideres)
		.ThenInclude(cl => cl.Consultor)
	.FirstOrDefaultAsync(c => c.Cedula == "1023928928");

// Acceso
var nombreCelula = consultor.Celula?.Nombre;
var lideres = consultor.Celula?.CelulaLideres
	.Select(cl => cl.Consultor.Nombre)
	.ToList();
```

---

### Ver Miembros de Una Célula

```csharp
var celula = await _context.Celulas
	.Include(c => c.Consultores)
	.Include(c => c.CelulaLideres)
		.ThenInclude(cl => cl.Consultor)
	.FirstOrDefaultAsync(c => c.Nombre == "Enterprise Team");

var miembros = celula.Consultores
	.Where(c => c.Estado == "Activo")
	.ToList();

var lideres = celula.CelulaLideres
	.Select(cl => cl.Consultor.Nombre)
	.ToList();
```

---

### Top 5 Células Más Grandes

```csharp
var topCelulas = await _context.Celulas
	.Include(c => c.Consultores)
	.Where(c => c.Activa)
	.Select(c => new
	{
		c.Nombre,
		TotalMiembros = c.Consultores.Count(co => co.Estado == "Activo"),
		Miembros = c.Consultores
			.Where(co => co.Estado == "Activo")
			.Select(co => co.Nombre)
			.ToList()
	})
	.OrderByDescending(c => c.TotalMiembros)
	.Take(5)
	.ToListAsync();
```

---

## 📝 SIGUIENTES PASOS DESPUÉS DE LA MIGRACIÓN

### ✅ Checklist Post-Migración

- [ ] Ejecutar todas las validaciones SQL
- [ ] Verificar organigrama en `/Organigrama`
- [ ] Probar UI de Células en `/Celulas/Index`
- [ ] Verificar asignación de líderes en `/Celulas/AsignarLideres/{id}`
- [ ] Confirmar que consultores nuevos se asignan correctamente a células
- [ ] Actualizar `EmpleadoSeederService.cs` para incluir células en el seed
- [ ] Documentar cualquier ajuste manual realizado
- [ ] Programar backup regular post-migración

---

## 🎨 INTERFAZ DE USUARIO

### Vista de Consultor con Célula

```cshtml
@model Consultor

<div class="card">
	<div class="card-body">
		<h5>@Model.Nombre</h5>
		<p class="text-muted">@Model.Cargo</p>
		@if (Model.Celula != null)
		{
			<span class="badge" style="background-color: @Model.Celula.Color">
				📍 @Model.Celula.Nombre
			</span>
		}
		else
		{
			<span class="badge bg-secondary">Sin Célula</span>
		}
	</div>
</div>
```

---

### Lista de Miembros de Célula

```cshtml
@model Celula

<h3>@Model.Nombre</h3>
<p class="text-muted">@Model.Descripcion</p>

<h5>Líderes</h5>
<ul class="list-unstyled">
	@foreach (var lider in Model.CelulaLideres.OrderByDescending(cl => cl.EsLiderPrincipal))
	{
		<li>
			<i class="bi bi-person-badge"></i>
			@lider.Consultor.Nombre
			@if (lider.EsLiderPrincipal)
			{
				<span class="badge bg-primary">Principal</span>
			}
		</li>
	}
</ul>

<h5>Miembros</h5>
<div class="list-group">
	@foreach (var consultor in Model.Consultores.Where(c => c.Estado == "Activo").OrderBy(c => c.Nombre))
	{
		<div class="list-group-item">
			<div class="d-flex justify-content-between">
				<span>@consultor.Nombre</span>
				<span class="text-muted">@consultor.Rol</span>
			</div>
		</div>
	}
</div>
```

---

## 🔧 TROUBLESHOOTING

### Problema: Líderes no encontrados

**Síntoma:**
```
⚠️ Jennifer Toro no encontrada
⚠️ Danna Ordoñez no encontrada
```

**Solución:**
1. Verificar que los consultores existen en la BD:

```sql
SELECT * FROM Consultores WHERE Nombre LIKE '%Toro%';
SELECT * FROM Consultores WHERE Nombre LIKE '%Ordoñez%' OR Nombre LIKE '%Ordonez%';
```

2. Si no existen, crearlos primero o ajustar el script con las cédulas correctas

---

### Problema: Consultores sin célula después de migración

**Síntoma:**
```sql
SELECT COUNT(*) FROM Consultores WHERE CelulaId IS NULL AND Estado = 'Activo';
-- Retorna > 0
```

**Solución:**
1. Ver cuáles consultores no se asignaron:

```sql
SELECT Cedula, Nombre, Correo, Cargo
FROM Consultores
WHERE CelulaId IS NULL AND Estado = 'Activo';
```

2. Asignar manualmente a "Sin Asignar" o a su célula correspondiente

---

## 📚 RECURSOS ADICIONALES

- Ver `REFERENCIA-RAPIDA-EQUIPOS.md` para ejemplos de código
- Ver `GUIA-EJECUCION-MIGRACION.md` para rollback y troubleshooting avanzado
- Ver `ANALISIS-ASIGNACION-EQUIPOS.md` para análisis detallado del CSV

---

**📅 Última actualización:** 2025-01  
**👤 Autor:** GitHub Copilot  
**🔗 Ver también:** `MigracionCelulas.sql`, `REFERENCIA-RAPIDA-EQUIPOS.md`

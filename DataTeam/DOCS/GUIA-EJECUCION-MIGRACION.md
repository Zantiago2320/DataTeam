# 🚀 GUÍA DE EJECUCIÓN: MIGRACIÓN DE EQUIPOS Y CÉLULAS

## 📋 ÍNDICE

1. [Pre-requisitos](#pre-requisitos)
2. [Decisión: Modelo Simple vs Flexible](#decisión-modelo-simple-vs-flexible)
3. [Ejecución Paso a Paso](#ejecución-paso-a-paso)
4. [Validaciones Post-Migración](#validaciones-post-migración)
5. [Troubleshooting](#troubleshooting)
6. [Rollback](#rollback)

---

## 🔍 PRE-REQUISITOS

### ✅ Checklist Antes de Ejecutar

- [ ] **Backup de base de datos** completo creado
- [ ] Confirmar conexión a la base de datos correcta
- [ ] Revisar el documento `ANALISIS-ASIGNACION-EQUIPOS.md`
- [ ] Decidir entre modelo Simple o Flexible
- [ ] Ventana de mantenimiento programada (si aplica)
- [ ] Notificar a usuarios sobre la migración

### 🗂️ Scripts Disponibles

| Script | Descripción | Modelo |
|--------|-------------|--------|
| `MigracionEquiposCelulas.sql` | Migración completa con modelo simple | Simple |
| `MigracionEquipoMiembroFlexible.sql` | Tabla adicional para asignaciones múltiples | Flexible |

---

## 🎯 DECISIÓN: MODELO SIMPLE VS FLEXIBLE

### Modelo Simple (Recomendado para empezar)

**📌 Características:**
- Usa el campo `Consultor.EquipoId` existente
- Cada consultor tiene UN equipo principal
- Tabla `EquipoLider` para múltiples líderes por equipo
- Más simple de consultar y mantener

**✅ Usar si:**
- La mayoría de consultores están en un solo equipo
- Los casos de múltiple asignación son excepciones
- Quieres simplicidad en las consultas
- Puedes resolver casos especiales con lógica de negocio

**📄 Script:** `MigracionEquiposCelulas.sql`

---

### Modelo Flexible (Recomendado para crecimiento)

**📌 Características:**
- Agrega tabla `EquipoMiembro` (many-to-many)
- Soporta múltiples equipos con % participación
- Campo `EsMiembroPrincipal` para equipo principal
- Campo `PorcentajeParticipacion` (suma debe ser 100%)

**✅ Usar si:**
- Hay muchos consultores en múltiples equipos
- Necesitas trackear % de participación
- El negocio requiere reportes por tiempo dedicado
- Planeas facturación/costing por equipo

**📄 Scripts:** 
1. `MigracionEquiposCelulas.sql` (ejecutar primero)
2. `MigracionEquipoMiembroFlexible.sql` (ejecutar segundo)

---

## 🛠️ EJECUCIÓN PASO A PASO

### Opción A: Modelo Simple

#### Paso 1: Backup

```sql
-- Crear backup completo
BACKUP DATABASE DataTeamDB
TO DISK = 'C:\Backups\DataTeamDB_PreMigracion.bak'
WITH INIT, NAME = 'Full Database Backup Before Migration';
```

#### Paso 2: Ejecutar Migración Principal

```powershell
# Desde PowerShell en la raíz del proyecto
sqlcmd -S localhost -d DataTeamDB -i "Data\Scripts\MigracionEquiposCelulas.sql" -o "Data\Scripts\Logs\Migracion_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
```

O desde **SQL Server Management Studio (SSMS)**:
1. Abrir `MigracionEquiposCelulas.sql`
2. Conectar a la base de datos `DataTeamDB`
3. Ejecutar (`F5`)
4. Revisar mensajes de salida

#### Paso 3: Validar

Ver sección [Validaciones Post-Migración](#validaciones-post-migración).

---

### Opción B: Modelo Flexible

#### Paso 1: Backup

```sql
BACKUP DATABASE DataTeamDB
TO DISK = 'C:\Backups\DataTeamDB_PreMigracionFlexible.bak'
WITH INIT, NAME = 'Full Database Backup Before Flexible Migration';
```

#### Paso 2: Ejecutar Migración Principal

```powershell
sqlcmd -S localhost -d DataTeamDB -i "Data\Scripts\MigracionEquiposCelulas.sql" -o "Data\Scripts\Logs\Migracion1_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
```

#### Paso 3: Ejecutar Migración Flexible

```powershell
sqlcmd -S localhost -d DataTeamDB -i "Data\Scripts\MigracionEquipoMiembroFlexible.sql" -o "Data\Scripts\Logs\Migracion2_$(Get-Date -Format 'yyyyMMdd_HHmmss').log"
```

#### Paso 4: Validar

Ver sección [Validaciones Post-Migración](#validaciones-post-migración).

---

## ✅ VALIDACIONES POST-MIGRACIÓN

### Validación 1: Consultores Sin Equipo

```sql
-- Debe retornar 0 filas (o solo consultores inactivos)
SELECT Cedula, Nombre, Estado
FROM Consultores
WHERE EquipoId IS NULL AND Estado = 'Activo';
```

**✅ Resultado esperado:** `0 rows`

---

### Validación 2: Equipos Sin Líder

```sql
-- Equipos activos sin líder asignado
SELECT e.Nombre
FROM Equipos e
LEFT JOIN EquipoLider el ON e.Id = el.EquipoId
WHERE el.EquipoId IS NULL AND e.Activo = 1;
```

**✅ Resultado esperado:** Solo debe aparecer **"Sin Asignar"**

---

### Validación 3: Distribución por Equipo

```sql
-- Ver cuántos consultores tiene cada equipo
SELECT 
	e.Nombre AS Equipo,
	COUNT(c.Id) AS TotalMiembros,
	COUNT(CASE WHEN c.Estado = 'Activo' THEN 1 END) AS Activos,
	COUNT(CASE WHEN c.Estado = 'Inactivo' THEN 1 END) AS Inactivos
FROM Equipos e
LEFT JOIN Consultores c ON c.EquipoId = e.Id
WHERE e.Activo = 1
GROUP BY e.Id, e.Nombre
ORDER BY TotalMiembros DESC;
```

**✅ Resultado esperado:** 
- Enterprise Team: ~9-12 consultores
- MindShift: ~10-12 consultores
- Nova: ~10-13 consultores
- Bon Voyage: ~8-10 consultores
- etc.

---

### Validación 4: Líderes Asignados

```sql
-- Ver líderes por equipo
SELECT 
	e.Nombre AS Equipo,
	c.Nombre AS Lider,
	c.Cedula,
	CASE WHEN el.EsLiderPrincipal = 1 THEN 'Principal' ELSE 'Secundario' END AS TipoLider
FROM Equipos e
INNER JOIN EquipoLider el ON e.Id = el.EquipoId
INNER JOIN Consultores c ON el.ConsultorId = c.Id
ORDER BY e.Nombre, el.EsLiderPrincipal DESC;
```

**✅ Resultado esperado:**
- Cada equipo (excepto "Sin Asignar") debe tener al menos 1 líder
- Alexander Castro debe aparecer en ~12 equipos
- Cristhian Amezquita en ~4 equipos

---

### Validación 5: Modelo Flexible (Solo si aplicaste `MigracionEquipoMiembroFlexible.sql`)

```sql
-- Consultores con % participación != 100%
SELECT 
	c.Cedula,
	c.Nombre,
	SUM(em.PorcentajeParticipacion) AS TotalPorcentaje
FROM Consultores c
INNER JOIN EquipoMiembro em ON c.Id = em.ConsultorId
WHERE em.Activo = 1
GROUP BY c.Id, c.Cedula, c.Nombre
HAVING SUM(em.PorcentajeParticipacion) != 100;
```

**✅ Resultado esperado:** `0 rows` (todos deben sumar 100%)

---

```sql
-- Consultores con múltiples equipos
SELECT 
	c.Nombre,
	c.Cedula,
	COUNT(em.EquipoId) AS CantidadEquipos,
	STRING_AGG(e.Nombre + ' (' + CAST(em.PorcentajeParticipacion AS NVARCHAR) + '%)', ', ') AS Equipos
FROM Consultores c
INNER JOIN EquipoMiembro em ON c.Id = em.ConsultorId
INNER JOIN Equipos e ON em.EquipoId = e.Id
WHERE em.Activo = 1
GROUP BY c.Id, c.Nombre, c.Cedula
HAVING COUNT(em.EquipoId) > 1
ORDER BY CantidadEquipos DESC;
```

**✅ Resultado esperado:** 
- Cecilio Trinidad: 2 equipos (Enterprise Team 50%, Wakanda 50%)
- Hugo Bermudez: 2 equipos (Enterprise Team 50%, Wakanda 50%)
- Esneider Gualtero: 2 equipos (Polaris 50%, Maya 50%)
- Diana Saavedra: 2 equipos (Enterprise Team 50%, Wakanda 50%)
- Alejandra Jimenez: 2 equipos (MindShift 50%, Data Stargazers 50%)
- Andres Rojas: 2 equipos (Maya 50%, Polaris 50%)

---

## 🔥 TROUBLESHOOTING

### Problema 1: "Cannot insert duplicate key"

**🐛 Error:**
```
Violation of UNIQUE KEY constraint 'UQ_EquipoLider_Consultor'
```

**✅ Solución:**
- El script ya maneja duplicados con `NOT EXISTS`
- Si ocurre, ejecutar:

```sql
-- Limpiar duplicados antes de reintentar
DELETE FROM EquipoLider
WHERE Id NOT IN (
	SELECT MIN(Id)
	FROM EquipoLider
	GROUP BY EquipoId, ConsultorId
);
```

---

### Problema 2: Líderes no encontrados

**🐛 Mensaje:**
```
⚠️ Jennifer Toro no encontrada
```

**✅ Solución:**
1. Verificar que el consultor existe en la BD:

```sql
SELECT * FROM Consultores WHERE Nombre LIKE '%Toro%';
```

2. Si no existe, crear manualmente o ajustar el script con la cédula correcta.

---

### Problema 3: Consultores siguen sin equipo después de la migración

**🐛 Síntoma:**
```sql
SELECT COUNT(*) FROM Consultores WHERE EquipoId IS NULL AND Estado = 'Activo';
-- Retorna > 0
```

**✅ Solución:**
1. Verificar que las cédulas en el script coinciden con la BD:

```sql
-- Buscar consultores activos sin equipo
SELECT Cedula, Nombre, Correo
FROM Consultores
WHERE EquipoId IS NULL AND Estado = 'Activo';
```

2. Asignar manualmente al equipo "Sin Asignar":

```sql
UPDATE Consultores
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Sin Asignar')
WHERE EquipoId IS NULL AND Estado = 'Activo';
```

---

### Problema 4: Suma de porcentajes != 100% (Modelo Flexible)

**🐛 Síntoma:**
```sql
SELECT c.Nombre, SUM(em.PorcentajeParticipacion) AS Total
FROM Consultores c
INNER JOIN EquipoMiembro em ON c.Id = em.ConsultorId
GROUP BY c.Id, c.Nombre
HAVING SUM(em.PorcentajeParticipacion) != 100;
```

**✅ Solución:**
1. Identificar casos problemáticos
2. Ajustar manualmente:

```sql
-- Ejemplo: Ajustar porcentaje de un consultor
UPDATE EquipoMiembro
SET PorcentajeParticipacion = 60
WHERE ConsultorId = (SELECT Id FROM Consultores WHERE Cedula = '1234567890')
AND EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Enterprise Team');
```

---

## 🔙 ROLLBACK

### Rollback Completo (Restaurar Backup)

```sql
USE master;
GO

-- Desconectar usuarios activos
ALTER DATABASE DataTeamDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
GO

-- Restaurar backup
RESTORE DATABASE DataTeamDB
FROM DISK = 'C:\Backups\DataTeamDB_PreMigracion.bak'
WITH REPLACE;
GO

-- Reconectar usuarios
ALTER DATABASE DataTeamDB SET MULTI_USER;
GO
```

---

### Rollback Parcial (Solo Equipos)

```sql
-- Limpiar asignaciones
UPDATE Consultores SET EquipoId = NULL;

-- Eliminar líderes
DELETE FROM EquipoLider;

-- Eliminar equipos (excepto "Sin Asignar")
DELETE FROM Equipos WHERE Nombre != 'Sin Asignar';
```

---

### Rollback Modelo Flexible

```sql
-- Eliminar tabla EquipoMiembro
DROP TABLE IF EXISTS EquipoMiembro;

-- Luego ejecutar Rollback Parcial de arriba
```

---

## 📊 QUERIES ÚTILES POST-MIGRACIÓN

### Query 1: Top 5 Equipos Más Grandes

```sql
SELECT TOP 5
	e.Nombre AS Equipo,
	COUNT(c.Id) AS TotalMiembros,
	STRING_AGG(c.Nombre, ', ') AS Consultores
FROM Equipos e
LEFT JOIN Consultores c ON c.EquipoId = e.Id
WHERE e.Activo = 1 AND c.Estado = 'Activo'
GROUP BY e.Id, e.Nombre
ORDER BY TotalMiembros DESC;
```

---

### Query 2: Consultores Sin Líder en su Equipo

```sql
SELECT 
	c.Nombre AS Consultor,
	e.Nombre AS Equipo,
	'Sin líder asignado' AS Alerta
FROM Consultores c
INNER JOIN Equipos e ON c.EquipoId = e.Id
WHERE e.Id NOT IN (SELECT DISTINCT EquipoId FROM EquipoLider)
AND c.Estado = 'Activo'
AND e.Nombre != 'Sin Asignar';
```

---

### Query 3: Líderes con Más Equipos

```sql
SELECT 
	c.Nombre AS Lider,
	c.Cedula,
	COUNT(DISTINCT el.EquipoId) AS CantidadEquipos,
	STRING_AGG(e.Nombre, ', ') AS Equipos
FROM Consultores c
INNER JOIN EquipoLider el ON c.Id = el.ConsultorId
INNER JOIN Equipos e ON el.EquipoId = e.Id
GROUP BY c.Id, c.Nombre, c.Cedula
ORDER BY CantidadEquipos DESC;
```

---

## 📝 PRÓXIMOS PASOS DESPUÉS DE LA MIGRACIÓN

### ✅ Checklist Post-Migración

- [ ] Ejecutar todas las validaciones
- [ ] Notificar a usuarios que la migración finalizó
- [ ] Actualizar `EmpleadoSeederService.cs` para incluir equipos en el seed
- [ ] Probar UI de Equipos en `/Equipos/Index`
- [ ] Probar asignación de miembros en `/Equipos/AsignarMiembros/{id}`
- [ ] Verificar organigrama en `/Organigrama`
- [ ] Documentar cualquier ajuste manual realizado
- [ ] Programar backup regular post-migración

---

### 🔄 Actualizar EmpleadoSeederService.cs

Después de la migración exitosa, actualizar el seeder para incluir equipos:

```csharp
// En EmpleadoSeederService.cs, agregar después de crear consultores:

// Asignar equipos automáticamente en desarrollo
var enterpriseTeam = context.Equipos.FirstOrDefault(e => e.Nombre == "Enterprise Team");
if (enterpriseTeam != null)
{
	var consultoresSinEquipo = context.Consultores
		.Where(c => c.EquipoId == null && c.Estado == "Activo")
		.Take(5)
		.ToList();

	foreach (var c in consultoresSinEquipo)
	{
		c.EquipoId = enterpriseTeam.Id;
	}
}

await context.SaveChangesAsync();
```

---

## 🎯 RESUMEN EJECUTIVO

| Aspecto | Modelo Simple | Modelo Flexible |
|---------|---------------|-----------------|
| **Scripts** | 1 script | 2 scripts |
| **Tiempo estimado** | 5-10 min | 10-15 min |
| **Complejidad** | Baja | Media |
| **Escalabilidad** | Media | Alta |
| **Casos de uso** | Mayoría en 1 equipo | Múltiples equipos frecuentes |
| **Rollback** | Fácil | Moderado |

---

**📅 Fecha del documento:** 2025-01  
**👤 Autor:** GitHub Copilot  
**📦 Versión:** 1.0  
**🔗 Referencias:** `ANALISIS-ASIGNACION-EQUIPOS.md`

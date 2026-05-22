# 🔍 Verificación Rápida de Seed

## Consultas SQL para Verificar los Datos Cargados

### 1. Verificar Total de Consultores
```sql
SELECT COUNT(*) AS TotalConsultores FROM Consultores WHERE Eliminado = 0;
-- Resultado esperado: 114
```

### 2. Verificar Total de Células
```sql
SELECT COUNT(*) AS TotalCelulas FROM Celulas WHERE Activa = 1;
-- Resultado esperado: 18
```

### 3. Distribución por Célula
```sql
SELECT 
	c.Nombre AS Celula,
	COUNT(co.Id) AS CantidadConsultores
FROM Celulas c
LEFT JOIN Consultores co ON c.Id = co.CelulaId AND co.Eliminado = 0
GROUP BY c.Nombre
ORDER BY COUNT(co.Id) DESC;
```

### 4. Consultores por Empresa
```sql
SELECT 
	Empresa,
	COUNT(*) AS Cantidad
FROM Consultores
WHERE Eliminado = 0
GROUP BY Empresa
ORDER BY COUNT(*) DESC;
```

### 5. Consultores por Rol
```sql
SELECT 
	Rol,
	COUNT(*) AS Cantidad
FROM Consultores
WHERE Eliminado = 0
GROUP BY Rol
ORDER BY COUNT(*) DESC;
```

### 6. Verificar Personas Multi-Célula
```sql
SELECT 
	Nombre,
	Cedula,
	Correo,
	(SELECT Nombre FROM Celulas WHERE Id = CelulaId) AS Celula
FROM Consultores
WHERE Nombre IN (
	'Cecilio Rafael de la Trinidad Maraima Nava',
	'Hugo Bermudez Diaz',
	'Robert Ricardo Ramirez Rojas',
	'Diego Guillermo Montenegro Revelo',
	'Diana Milena Saavedra Ferrer',
	'Esneider Gualtero Hernández',
	'Alejandra Xiomara Jimenez',
	'Andres Patricio Rojas Sanjuan'
)
AND Eliminado = 0
ORDER BY Nombre, Cedula;
```

### 7. Consultores sin Célula Asignada
```sql
SELECT 
	Nombre,
	Cedula,
	Cargo
FROM Consultores co
INNER JOIN Celulas c ON co.CelulaId = c.Id
WHERE c.Nombre = 'Sin Asignar'
AND co.Eliminado = 0;
```

### 8. Verificar Fechas de Ingreso
```sql
SELECT 
	YEAR(FechaIngreso) AS Año,
	COUNT(*) AS Ingresos
FROM Consultores
WHERE Eliminado = 0
GROUP BY YEAR(FechaIngreso)
ORDER BY YEAR(FechaIngreso) DESC;
```

### 9. Top 10 Células Más Grandes
```sql
SELECT TOP 10
	c.Nombre AS Celula,
	c.Color,
	COUNT(co.Id) AS Miembros
FROM Celulas c
LEFT JOIN Consultores co ON c.Id = co.CelulaId AND co.Eliminado = 0
GROUP BY c.Nombre, c.Color
ORDER BY COUNT(co.Id) DESC;
```

### 10. Consultores con Información Completa
```sql
SELECT 
	COUNT(*) AS ConInformacionCompleta
FROM Consultores
WHERE 
	Eliminado = 0
	AND Nombre IS NOT NULL
	AND Correo IS NOT NULL
	AND Cargo IS NOT NULL
	AND CelulaId IS NOT NULL
	AND FechaIngreso IS NOT NULL
	AND FechaNacimiento IS NOT NULL;
-- Resultado esperado: 114
```

---

## 🎯 Resultados Esperados

| Métrica | Valor Esperado |
|---------|----------------|
| Total Consultores | 114 |
| Total Células | 18 |
| Mayor Célula | Enterprise Team / Nova / Polaris Software Team |
| Empresas Representadas | 7+ |
| Roles Diferentes | 10+ |

---

## ✅ Checklist de Verificación

- [ ] Total de consultores = 114
- [ ] Total de células = 18
- [ ] No hay consultores con datos NULL críticos
- [ ] Todas las células tienen al menos 1 consultor (excepto "Sin Asignar" si aplica)
- [ ] Las personas multi-célula tienen registros separados
- [ ] Las fechas se parsearon correctamente
- [ ] Los correos son válidos (@aportesenlinea.com, @sqasa.co, etc.)

---

## 🛠️ En la UI

### Verificación en la Página de Empleados
1. Navega a `/Consultores`
2. Deberías ver: **"Mostrando 114 empleados"**
3. Filtro por Célula debe mostrar 18 opciones
4. Filtro por Empresa debe mostrar: AEL, SOPHOS, STEFANINI, PERIFERIA IT, etc.

### Verificación en la Página de Células
1. Navega a `/Celulas`
2. Deberías ver 18 células activas
3. Cada célula debe mostrar su cantidad de miembros

---

## 🔄 Reiniciar Seed (Si es Necesario)

Si los datos no se cargan correctamente:

1. **Detén la aplicación**
2. **Limpia la base de datos InMemory** (automático al reiniciar)
3. **Inicia de nuevo**
4. Verifica los logs en la consola:
   ```
   [DbInitializerService] Se crearon 18 células
   [DbInitializerService] ✅ Se crearon 114 consultores reales del CSV
   ```

---

**Última actualización**: ${new Date().toLocaleDateString('es-ES')}

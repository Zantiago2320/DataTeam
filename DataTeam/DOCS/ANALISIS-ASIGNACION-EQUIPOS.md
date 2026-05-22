# 📊 ANÁLISIS DE ASIGNACIÓN DE EQUIPOS Y CÉLULAS

## 🎯 RESUMEN DEL ANÁLISIS

Análisis completo del CSV proporcionado con **113 registros** de consultores (algunos duplicados por asignaciones múltiples).

---

## 📋 EQUIPOS/CÉLULAS IDENTIFICADOS

### **Equipos Únicos en el CSV:**

| # | Nombre del Equipo | Cantidad Asignaciones | Líder(es) Identificado(s) | Color Sugerido |
|---|-------------------|----------------------|---------------------------|----------------|
| 1 | **Enterprise Team** | 9 | Alexander Castro, Jennifer Toro, Hugo Bermudez | #1E3A8A (Azul Oscuro) |
| 2 | **Nova** | 11 | Alexander Castro, Jennifer Toro, Robert Ramirez | #10B981 (Verde) |
| 3 | **Bon Voyage** | 10 | Alexander Castro, Cristhian Amezquita, Diego Montenegro | #F59E0B (Naranja) |
| 4 | **MindShift** | 12 | Alexander Castro, Cristhian Amezquita, Ingrid Porras | #8B5CF6 (Morado) |
| 5 | **Wakanda** | 9 | Alexander Castro, Jennifer Toro, Maria Kamila Redondo | #EF4444 (Rojo) |
| 6 | **DevSecOps** (DEVSECOPS) | 7 | Cristhian Amezquita, Robert Ramirez, Danna Ordoñez | #6366F1 (Índigo) |
| 7 | **Data Stargazers** | 6 | Alexander Castro, Diego Montenegro, Cesar Pachon | #EC4899 (Rosa) |
| 8 | **Maya** | 10 | Alexander Castro, Jennifer Toro, Robert Ramirez | #14B8A6 (Turquesa) |
| 9 | **Aurora** | 2 | Alexander Castro, Robert Ramirez, Karol Rubiano | #F97316 (Naranja Claro) |
| 10 | **Polaris Software Team** | 11 | Alexander Castro, Jennifer Toro, Victor Martinez | #3B82F6 (Azul) |
| 11 | **Seguridad** | 1 | Alexander Castro | #DC2626 (Rojo Oscuro) |
| 12 | **Administrativo** | 1 | Alexander Castro | #64748B (Gris) |
| 13 | **Transversal Calidad** | 3 | Cristhian Amezquita | #A855F7 (Morado Claro) |
| 14 | **Dirección Desarrollo** | 5 | Alexander Castro, Robert Ramirez | #0EA5E9 (Cyan) |
| 15 | **Facturador** | 2 | Victor Martinez | #22C55E (Verde Lima) |
| 16 | **Sin Asignación** (Sin asigancion) | 3 | N/A | #9CA3AF (Gris Claro) |

**TOTAL: 16 equipos identificados**

---

## 👥 LÍDERES IDENTIFICADOS

### **Líderes por Frecuencia:**

| Nombre Completo | Correo | Equipos que Lidera | Rol Identificado |
|-----------------|--------|-------------------|------------------|
| **Alexander Castro** | acastro@aportesenlinea.com | 15 equipos | Director de Desarrollo |
| **Jennifer Toro** | (no proporcionado) | 8 equipos | PO/Scrum Master Lead |
| **Cristhian Amezquita** | camezquita@aportesenlinea.com | 7 equipos | Coordinador QA y DevSecOps |
| **Robert Ramirez** | rramirez@aportesenlinea.com | 5 equipos | Gerente de Transformación Digital |
| **Victor Martinez** | (no proporcionado) | 4 equipos | Arquitecto Lead |
| **Diego Montenegro** | dmontenegro@aportesenlinea.com | 2 equipos | Gerente de Soluciones |
| **Maria Kamila Redondo** | mredondo@aportesenlinea.com | 2 equipos | Director Servicios de Contacto |
| **Ingrid Porras** | imanrique@aportesenlinea.com | 1 equipo | Gerente de Servicios |
| **Juan Manuel Clavijo** | (no proporcionado) | 0 equipos (sponsor) | Ejecutivo |
| **Mauricio Mejia** | (no proporcionado) | 1 equipo | Coordinador Operaciones |
| **Danna Ordoñez** | dordonez@aportesenlinea.com | 1 equipo | PO Funcional |
| **Karol Rubiano** | ebarros@aportesenlinea.com | 1 equipo | Director Agilismo |
| **Cesar Pachon** | cpachon@aportesenlinea.com | 1 equipo | Coordinador Desarrollo de Negocio |

---

## 🔍 CASOS ESPECIALES DETECTADOS

### **1. Consultores en Múltiples Equipos:**

| Cédula | Nombre | Equipos Asignados | % Participación |
|--------|--------|------------------|-----------------|
| 779729 | Cecilio Trinidad | Enterprise Team (50%), Wakanda (50%) | 50% + 50% |
| 79568718 | Hugo Bermudez | Enterprise Team (50%), Wakanda (50%) | 50% + 50% |
| 1072666410 | Esneider Gualtero | Polaris (50%), Maya (50%) | 50% + 50% |
| 80127568 | Robert Ramirez | DEVSECOPS, Aurora, Nova (sponsor) | Múltiples |
| 80088963 | Diego Montenegro | Bon Voyage, Data Stargazers (sponsor) | Múltiples |
| 53006451 | Diana Saavedra | Enterprise Team (50%), Wakanda (50%) | 50% + 50% |
| 52856823 | Alejandra Rubio | Nova (50%) | Solo 50% |
| 1019075102 | Alejandra Jimenez | MindShift (50%), Data Stargazers (50%) | 50% + 50% |
| 80826699 | Andres Rojas | Maya (50%), Polaris (50%) | 50% + 50% |
| 1082843183 | Karol Rubiano | Aurora (50%) | Solo 50% |
| 79694723 | Alexander Castro | Dirección Desarrollo (50%) | Solo 50% |

**⚠️ DECISIÓN ARQUITECTÓNICA:**
- En la BD, `Consultor.EquipoId` solo puede tener UN equipo principal
- Para asignaciones múltiples, usar la tabla `EquipoLider` o crear tabla `EquipoMiembro` con `PorcentajeParticipacion`

---

### **2. Consultores sin Equipo Asignado:**

| Cédula | Nombre | Estado |
|--------|--------|--------|
| 1023026324 | Paula Andrea Rojas | Sin asigancion |
| 1012462196 | Nelson Pabon | Sin asigancion |
| 1007449064 | Jhonattan Sabogal | Campo vacío |

**→ Asignar al equipo por defecto "Sin Asignar"**

---

### **3. Roles Especiales (Sponsors/Directores):**

| Nombre | Rol | Nota |
|--------|-----|------|
| Sandra Tovar | Director Servicios Especializados | Sponsor |
| Juan Manuel Clavijo | (Mencionado como sponsor) | No aparece como consultor |
| Robert Ramirez | Gerente Transformación Digital | Sponsor múltiples equipos |
| Diego Montenegro | Gerente Soluciones | Sponsor múltiples equipos |
| Ingrid Porras | Gerente Servicios | Sponsor |
| Maria Kamila Redondo | Director Servicios Contacto | Sponsor |
| Laura Avila | Director Innovación | Sponsor |

---

## 📐 MODELO DE DATOS PROPUESTO

### **Opción 1: Modelo Simple (Actual)**
```
Consultor
  ├─ EquipoId (FK, nullable) → Equipo principal
  ├─ CelulaId (FK) → Célula (obligatorio)

Equipo
  ├─ EquipoLideres (N:N) → Líderes asignados
  └─ Consultores (1:N) → Miembros
```

**Limitación:** No permite múltiples equipos con % participación.

---

### **Opción 2: Modelo Flexible (Recomendado)**
```
Consultor
  ├─ CelulaId (FK) → Célula principal

Equipo
  ├─ EquipoLideres (N:N)
  └─ EquipoMiembros (N:N) → Nueva tabla

EquipoMiembro (Nueva)
  ├─ EquipoId (FK)
  ├─ ConsultorId (FK)
  ├─ PorcentajeParticipacion (int)
  ├─ EsMiembroPrincipal (bool)
  └─ FechaAsignacion
```

**Ventaja:** Soporta múltiples equipos con % participación.

---

## 🎨 PALETA DE COLORES POR EQUIPO

```json
{
  "Enterprise Team": "#1E3A8A",
  "Nova": "#10B981",
  "Bon Voyage": "#F59E0B",
  "MindShift": "#8B5CF6",
  "Wakanda": "#EF4444",
  "DevSecOps": "#6366F1",
  "Data Stargazers": "#EC4899",
  "Maya": "#14B8A6",
  "Aurora": "#F97316",
  "Polaris Software Team": "#3B82F6",
  "Seguridad": "#DC2626",
  "Administrativo": "#64748B",
  "Transversal Calidad": "#A855F7",
  "Dirección Desarrollo": "#0EA5E9",
  "Facturador": "#22C55E",
  "Sin Asignar": "#9CA3AF"
}
```

---

## 📊 ESTADÍSTICAS

```
Total consultores únicos: 102
Total asignaciones: 113 (incluye duplicados por múltiples equipos)
Consultores con múltiples equipos: 11
Equipos/Células únicos: 16
Líderes únicos: 13
Consultores sin asignación: 3
```

---

## ⚠️ DECISIONES PENDIENTES

1. **¿Crear tabla `EquipoMiembro` para soportar % participación?**
   - ✅ Recomendado: SÍ
   - Permite múltiples equipos con porcentajes

2. **¿Qué hacer con consultores al 50% en dos equipos?**
   - Opción A: Asignar equipo principal + tabla EquipoMiembro con ambos
   - Opción B: Solo guardar en EquipoMiembro (no usar EquipoId)

3. **¿Los "sponsors" deben ser líderes de equipos?**
   - Recomendado: NO (son roles ejecutivos, no líderes operativos)
   - Crear campo `Consultor.EsSponsor` o rol Identity "Sponsor"

4. **¿Cómo manejar líderes sin correo en el CSV?**
   - Jennifer Toro, Victor Martinez, Juan Manuel Clavijo → Buscar en BD o crear usuarios temporales

5. **¿Células = Equipos en este contexto?**
   - El CSV usa "Célula" pero los nombres parecen ser "Equipos"
   - Propuesta: Crear UN equipo por cada nombre en columna "Célula"
   - Crear UNA célula genérica por área (ej. "Célula Desarrollo", "Célula QA")

---

## 🚀 PLAN DE MIGRACIÓN PROPUESTO

### **Fase 1: Preparación**
1. Crear tabla `EquipoMiembro` (nueva)
2. Poblar tabla `Equipo` con 16 equipos del CSV
3. Crear/actualizar líderes en tabla `Consultor`

### **Fase 2: Asignación Simple**
1. Asignar cada consultor a su equipo principal (primera aparición en CSV)
2. Asignar líderes usando `EquipoLider`

### **Fase 3: Asignación Múltiple**
1. Poblar `EquipoMiembro` para consultores con múltiples equipos
2. Guardar % participación

### **Fase 4: Validación**
1. Query de validación: consultores sin equipo
2. Query de validación: equipos sin líder
3. Query de validación: suma de % participación = 100%

---

## 📝 QUERIES DE VALIDACIÓN

```sql
-- Consultores sin equipo asignado
SELECT Cedula, Nombre, Correo
FROM Consultores
WHERE EquipoId IS NULL AND Estado = 'Activo';

-- Equipos sin líder
SELECT e.Nombre
FROM Equipos e
LEFT JOIN EquipoLider el ON e.Id = el.EquipoId
WHERE el.EquipoId IS NULL AND e.Activo = 1;

-- Consultores con % participación != 100%
SELECT c.Cedula, c.Nombre, SUM(em.PorcentajeParticipacion) AS TotalPorcentaje
FROM Consultores c
INNER JOIN EquipoMiembro em ON c.Id = em.ConsultorId
GROUP BY c.Id, c.Cedula, c.Nombre
HAVING SUM(em.PorcentajeParticipacion) != 100;

-- Distribución por equipo
SELECT e.Nombre AS Equipo, COUNT(c.Id) AS TotalMiembros
FROM Equipos e
LEFT JOIN Consultores c ON c.EquipoId = e.Id
WHERE e.Activo = 1
GROUP BY e.Id, e.Nombre
ORDER BY TotalMiembros DESC;
```

---

## 🎯 PRÓXIMOS PASOS

1. ✅ **Revisar este análisis** con el usuario
2. ⏳ **Decidir**: ¿Crear tabla `EquipoMiembro`?
3. ⏳ **Generar scripts SQL** de migración
4. ⏳ **Actualizar `EmpleadoSeederService.cs`**
5. ⏳ **Ejecutar migración** en base de datos
6. ⏳ **Validar** con queries de verificación

---

**Fecha de análisis:** 2025-01-XX  
**Analizado por:** GitHub Copilot  
**Fuente:** CSV proporcionado por usuario con 113 registros

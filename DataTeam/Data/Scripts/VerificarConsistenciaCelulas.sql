-- Script para verificar y reparar inconsistencias en asignación de células
-- DataTeam - Verificación de Consistencia de Células
-- Fecha: 2025

-- =====================================================
-- 1. CONSULTAR CONSULTORES SIN CÉLULA PRINCIPAL
-- =====================================================
SELECT 
    c.Id,
    c.Cedula,
    c.Nombre,
    c.Cargo,
    c.CelulaId AS 'CelulaPrincipal_ID',
    cel.Nombre AS 'CelulaPrincipal_Nombre',
    (SELECT COUNT(*) FROM CelulaMiembro cm WHERE cm.ConsultorId = c.Id) AS 'TotalCelulasMiembro'
FROM Consultores c
LEFT JOIN Celulas cel ON c.CelulaId = cel.Id
WHERE c.Eliminado = 0
ORDER BY 
    CASE WHEN c.CelulaId IS NULL THEN 0 ELSE 1 END,
    c.Nombre;

-- =====================================================
-- 2. CONSULTORES CON CÉLULA PRINCIPAL PERO SIN MIEMBRO
-- =====================================================
SELECT 
    c.Id,
    c.Cedula,
    c.Nombre,
    cel.Nombre AS 'CelulaPrincipal',
    'Tiene CelulaId pero no está en CelulaMiembro' AS 'Problema'
FROM Consultores c
INNER JOIN Celulas cel ON c.CelulaId = cel.Id
LEFT JOIN CelulaMiembro cm ON c.Id = cm.ConsultorId AND c.CelulaId = cm.CelulaId
WHERE c.Eliminado = 0
    AND cm.ConsultorId IS NULL;

-- =====================================================
-- 3. CONSULTORES EN CELULAS MIEMBRO SIN CÉLULA PRINCIPAL
-- =====================================================
SELECT 
    c.Id,
    c.Cedula,
    c.Nombre,
    cel.Nombre AS 'CelulaEnMiembro',
    'Está en CelulaMiembro pero no tiene CelulaId' AS 'Problema'
FROM Consultores c
INNER JOIN CelulaMiembro cm ON c.Id = cm.ConsultorId
INNER JOIN Celulas cel ON cm.CelulaId = cel.Id
WHERE c.Eliminado = 0
    AND c.CelulaId IS NULL;

-- =====================================================
-- 4. RESUMEN POR CÉLULA
-- =====================================================
SELECT 
    cel.Nombre AS 'Célula',
    COUNT(DISTINCT c.Id) AS 'Consultores_CelulaPrincipal',
    COUNT(DISTINCT cm.ConsultorId) AS 'Consultores_EnMiembro',
    cel.Activa
FROM Celulas cel
LEFT JOIN Consultores c ON cel.Id = c.CelulaId AND c.Eliminado = 0
LEFT JOIN CelulaMiembro cm ON cel.Id = cm.CelulaId
LEFT JOIN Consultores c2 ON cm.ConsultorId = c2.Id AND c2.Eliminado = 0
WHERE cel.Activa = 1
GROUP BY cel.Nombre, cel.Activa
ORDER BY cel.Nombre;

-- =====================================================
-- 5. REPARAR: SINCRONIZAR CÉLULA PRINCIPAL CON MIEMBRO
-- (Ejecutar solo si encuentras inconsistencias)
-- =====================================================
/*
-- OPCIÓN A: Agregar a CelulaMiembro los que tienen CelulaId pero no están en la tabla
INSERT INTO CelulaMiembro (CelulaId, ConsultorId, Rol, FechaAsignacion)
SELECT 
    c.CelulaId,
    c.Id,
    COALESCE(c.Rol, 'Miembro'),
    GETDATE()
FROM Consultores c
LEFT JOIN CelulaMiembro cm ON c.Id = cm.ConsultorId AND c.CelulaId = cm.CelulaId
WHERE c.Eliminado = 0
    AND c.CelulaId IS NOT NULL
    AND cm.ConsultorId IS NULL;

-- OPCIÓN B: Actualizar CelulaId basándose en la primera célula de CelulaMiembro
UPDATE c
SET c.CelulaId = (
    SELECT TOP 1 cm.CelulaId
    FROM CelulaMiembro cm
    WHERE cm.ConsultorId = c.Id
    ORDER BY cm.FechaAsignacion
)
FROM Consultores c
WHERE c.Eliminado = 0
    AND c.CelulaId IS NULL
    AND EXISTS (SELECT 1 FROM CelulaMiembro cm WHERE cm.ConsultorId = c.Id);
*/

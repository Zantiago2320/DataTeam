-- ============================================================================
-- SCRIPT DE MIGRACIÓN: ASIGNACIÓN DE CONSULTORES A CÉLULAS Y LÍDERES
-- Fecha: 2025-01
-- Descripción: Asigna consultores a células según CSV proporcionado
-- ============================================================================

USE DataTeamDB;
GO

PRINT '========================================';
PRINT '🚀 INICIANDO MIGRACIÓN DE CÉLULAS';
PRINT '========================================';
PRINT '';

-- ============================================================================
-- PARTE 1: CREAR CÉLULAS FALTANTES
-- ============================================================================

PRINT '📦 Creando células...';

MERGE INTO Celulas AS Target
USING (VALUES
	('Enterprise Team', 'Equipo empresarial de desarrollo y arquitectura', '#1E3A8A', 1),
	('Nova', 'Equipo de innovación y nuevas tecnologías', '#10B981', 1),
	('Bon Voyage', 'Equipo de desarrollo de soluciones de viaje', '#F59E0B', 1),
	('MindShift', 'Equipo de transformación digital', '#8B5CF6', 1),
	('Wakanda', 'Equipo de desarrollo avanzado', '#EF4444', 1),
	('DevSecOps', 'Equipo de seguridad y operaciones', '#6366F1', 1),
	('Data Stargazers', 'Equipo de datos y analytics', '#EC4899', 1),
	('Maya', 'Equipo de desarrollo de plataformas', '#14B8A6', 1),
	('Aurora', 'Equipo de desarrollo de aplicaciones', '#F97316', 1),
	('Polaris Software Team', 'Equipo de desarrollo de software', '#3B82F6', 1),
	('Seguridad', 'Equipo especializado en seguridad', '#DC2626', 1),
	('Administrativo', 'Equipo administrativo', '#64748B', 1),
	('Transversal Calidad', 'Equipo transversal de calidad', '#A855F7', 1),
	('Direccion Desarrollo', 'Dirección de desarrollo', '#0EA5E9', 1),
	('Facturador', 'Equipo de facturación electrónica', '#22C55E', 1),
	('DEVSECOPS', 'Equipo de DevSecOps y seguridad', '#6366F1', 1),
	('Bon voyage', 'Equipo de desarrollo de soluciones de viaje (variante)', '#F59E0B', 1)
) AS Source (Nombre, Descripcion, Color, Activa)
ON Target.Nombre = Source.Nombre
WHEN NOT MATCHED THEN
	INSERT (Nombre, Descripcion, Color, Activa, FechaCreacion, FechaModificacion)
	VALUES (Source.Nombre, Source.Descripcion, Source.Color, Source.Activa, GETUTCDATE(), GETUTCDATE())
WHEN MATCHED THEN
	UPDATE SET 
		Descripcion = Source.Descripcion,
		Color = Source.Color,
		Activa = Source.Activa,
		FechaModificacion = GETUTCDATE();

PRINT '✅ Células creadas/actualizadas: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ============================================================================
-- PARTE 2: ASIGNAR LÍDERES A CÉLULAS
-- ============================================================================

PRINT '';
PRINT '👥 Asignando líderes a células...';

-- Alexander Castro → Múltiples células
DECLARE @AlexanderCastro INT = (SELECT Id FROM Consultores WHERE Cedula = '79694723');

IF @AlexanderCastro IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @AlexanderCastro, GETUTCDATE(), 1
	FROM Celulas c
	WHERE c.Nombre IN (
		'Enterprise Team', 'Nova', 'Bon Voyage', 'MindShift', 'Wakanda',
		'Data Stargazers', 'Maya', 'Aurora', 'Polaris Software Team',
		'Seguridad', 'Administrativo', 'Direccion Desarrollo', 'DevSecOps',
		'Bon voyage'
	)
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @AlexanderCastro
	);
	PRINT '  ✅ Alexander Castro asignado como líder a ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' células';
END
ELSE
	PRINT '  ⚠️ Alexander Castro no encontrado (Cédula: 79694723)';

-- Jennifer Toro → Múltiples células (PO Técnico/Scrum Master/Agil Coach)
DECLARE @JenniferToro INT = (SELECT Id FROM Consultores WHERE Correo = 'jtoro@aportesenlinea.com');

IF @JenniferToro IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @JenniferToro, GETUTCDATE(), 0
	FROM Celulas c
	WHERE c.Nombre IN (
		'Nova', 'Enterprise Team', 'Wakanda', 'Polaris Software Team',
		'Maya', 'Bon Voyage', 'MindShift', 'Data Stargazers'
	)
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @JenniferToro
	);
	PRINT '  ✅ Jennifer Toro asignada como líder a ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' células';
END
ELSE
	PRINT '  ⚠️ Jennifer Toro no encontrada';

-- Cristhian Amezquita → Múltiples células (QA/DevSecOps)
DECLARE @CristhianAmezquita INT = (SELECT Id FROM Consultores WHERE Cedula = '1032458577');

IF @CristhianAmezquita IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @CristhianAmezquita, GETUTCDATE(), 1
	FROM Celulas c
	WHERE c.Nombre IN (
		'Bon Voyage', 'MindShift', 'DEVSECOPS', 'DevSecOps', 'Transversal Calidad',
		'Maya', 'Nova', 'Enterprise Team', 'Wakanda', 'Data Stargazers',
		'Polaris Software Team'
	)
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @CristhianAmezquita
	);
	PRINT '  ✅ Cristhian Amezquita asignado como líder a ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' células';
END
ELSE
	PRINT '  ⚠️ Cristhian Amezquita no encontrado (Cédula: 1032458577)';

-- Victor Martinez → Múltiples células (Arquitecto)
DECLARE @VictorMartinez INT = (SELECT Id FROM Consultores WHERE Nombre LIKE '%Victor%Martinez%');

IF @VictorMartinez IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @VictorMartinez, GETUTCDATE(), 0
	FROM Celulas c
	WHERE c.Nombre IN (
		'MindShift', 'Bon Voyage', 'Nova', 'Enterprise Team', 'Wakanda',
		'Maya', 'Polaris Software Team', 'Facturador'
	)
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @VictorMartinez
	);
	PRINT '  ✅ Victor Martinez asignado como líder a ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' células';
END
ELSE
	PRINT '  ⚠️ Victor Martinez no encontrado';

-- Robert Ramirez → Múltiples células (Sponsor/Gerente)
DECLARE @RobertRamirez INT = (SELECT Id FROM Consultores WHERE Cedula = '80127568');

IF @RobertRamirez IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @RobertRamirez, GETUTCDATE(), 0
	FROM Celulas c
	WHERE c.Nombre IN (
		'DEVSECOPS', 'DevSecOps', 'Aurora', 'Nova', 'Direccion Desarrollo'
	)
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @RobertRamirez
	);
	PRINT '  ✅ Robert Ramirez asignado como líder a ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' células';
END
ELSE
	PRINT '  ⚠️ Robert Ramirez no encontrado (Cédula: 80127568)';

-- Juan Manuel Clavijo → Múltiples células (Sponsor)
DECLARE @JuanClavijo INT = (SELECT Id FROM Consultores WHERE Nombre LIKE '%Juan Manuel%Clavijo%');

IF @JuanClavijo IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @JuanClavijo, GETUTCDATE(), 0
	FROM Celulas c
	WHERE c.Nombre IN (
		'DEVSECOPS', 'MindShift', 'Bon Voyage', 'Wakanda', 'Nova'
	)
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @JuanClavijo
	);
	PRINT '  ✅ Juan Manuel Clavijo asignado como líder a ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' células';
END
ELSE
	PRINT '  ⚠️ Juan Manuel Clavijo no encontrado';

-- Diego Montenegro → Bon Voyage, Data Stargazers
DECLARE @DiegoMontenegro INT = (SELECT Id FROM Consultores WHERE Cedula = '80088963');

IF @DiegoMontenegro IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @DiegoMontenegro, GETUTCDATE(), 0
	FROM Celulas c
	WHERE c.Nombre IN ('Bon Voyage', 'Data Stargazers')
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @DiegoMontenegro
	);
	PRINT '  ✅ Diego Montenegro asignado como líder a ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' células';
END
ELSE
	PRINT '  ⚠️ Diego Montenegro no encontrado (Cédula: 80088963)';

-- Maria Camila Redondo → Wakanda, MindShift
DECLARE @MariaRedondo INT = (SELECT Id FROM Consultores WHERE Cedula = '1020713148');

IF @MariaRedondo IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @MariaRedondo, GETUTCDATE(), 0
	FROM Celulas c
	WHERE c.Nombre IN ('Wakanda', 'MindShift')
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @MariaRedondo
	);
	PRINT '  ✅ Maria Camila Redondo asignada como líder a ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' células';
END
ELSE
	PRINT '  ⚠️ Maria Camila Redondo no encontrada (Cédula: 1020713148)';

-- Danna Ordoñez → DEVSECOPS
DECLARE @DannaOrdonez INT = (SELECT Id FROM Consultores WHERE Nombre LIKE '%Danna%Ordoñez%' OR Nombre LIKE '%Danna%Ordonez%');

IF @DannaOrdonez IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @DannaOrdonez, GETUTCDATE(), 0
	FROM Celulas c
	WHERE c.Nombre IN ('DEVSECOPS', 'DevSecOps')
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @DannaOrdonez
	);
	PRINT '  ✅ Danna Ordoñez asignada como líder';
END
ELSE
	PRINT '  ⚠️ Danna Ordoñez no encontrada';

-- Mauricio Mejia → Polaris Software Team
DECLARE @MauricioMejia INT = (SELECT Id FROM Consultores WHERE Nombre LIKE '%Mauricio%Mejia%');

IF @MauricioMejia IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @MauricioMejia, GETUTCDATE(), 0
	FROM Celulas c
	WHERE c.Nombre = 'Polaris Software Team'
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @MauricioMejia
	);
	PRINT '  ✅ Mauricio Mejia asignado como líder';
END
ELSE
	PRINT '  ⚠️ Mauricio Mejia no encontrado';

-- Ingrid Porras → Enterprise Team
DECLARE @IngridPorras INT = (SELECT Id FROM Consultores WHERE Cedula = '52414874');

IF @IngridPorras IS NOT NULL
BEGIN
	INSERT INTO CelulaLider (CelulaId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT c.Id, @IngridPorras, GETUTCDATE(), 0
	FROM Celulas c
	WHERE c.Nombre = 'Enterprise Team'
	AND NOT EXISTS (
		SELECT 1 FROM CelulaLider cl 
		WHERE cl.CelulaId = c.Id AND cl.ConsultorId = @IngridPorras
	);
	PRINT '  ✅ Ingrid Porras asignada como líder';
END
ELSE
	PRINT '  ⚠️ Ingrid Porras no encontrada (Cédula: 52414874)';

PRINT '';
PRINT '✅ Asignación de líderes completada';

-- ============================================================================
-- PARTE 3: ASIGNAR CONSULTORES A CÉLULAS
-- ============================================================================

PRINT '';
PRINT '🔧 Asignando consultores a células...';

-- Enterprise Team
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Enterprise Team')
WHERE Cedula IN (
	'1023928928', '52964246', '779729', '1030697393', '1000538611',
	'1105786797', '79568718', '53006451', '1128417080', '1016064908',
	'1001936659'
) AND CelulaId IS NULL;
PRINT '  → Enterprise Team: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Nova
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Nova')
WHERE Cedula IN (
	'1018471934', '1033720903', '80920988', '1030608602', '1067946006',
	'1073238911', '1022333350', '80852689', '1144037797', '52856823',
	'1110487049', '80127568'
) AND CelulaId IS NULL;
PRINT '  → Nova: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Bon Voyage / Bon voyage (normalizar a "Bon Voyage")
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Bon Voyage')
WHERE Cedula IN (
	'1030635496', '1065817149', '43186711', '1014199932', '1235245612',
	'1214715394', '1032416617', '1083019159', '1110515353', '1111791640',
	'80088963', '1036934864'
) AND CelulaId IS NULL;
PRINT '  → Bon Voyage: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- MindShift
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'MindShift')
WHERE Cedula IN (
	'80174594', '1085297237', '80075269', '1023918569', '1073710856',
	'1016082141', '1233907650', '52414874', '1026294015', '1019075102',
	'1192910852', '1214724608'
) AND CelulaId IS NULL;
PRINT '  → MindShift: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Wakanda
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Wakanda')
WHERE Cedula IN (
	'1140870751', '779729', '1007165946', '1026288243', '79568718',
	'53006451', '1012403476', '1001344075', '1007157090', '1020713148'
) AND CelulaId IS NULL;
PRINT '  → Wakanda: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- DEVSECOPS / DevSecOps (normalizar a "DEVSECOPS")
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'DEVSECOPS')
WHERE Cedula IN (
	'1000874819', '80127568', '1040327881', '1003557777', '1022353132',
	'1053804044', '1053773898', '1041611184', '1032458577'
) AND CelulaId IS NULL;
PRINT '  → DEVSECOPS: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Data Stargazers
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Data Stargazers')
WHERE Cedula IN (
	'1083023296', '1192794693', '1016042511', '80088963', '1022355992',
	'1019075102', '52776078'
) AND CelulaId IS NULL;
PRINT '  → Data Stargazers: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Maya
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Maya')
WHERE Cedula IN (
	'1234097180', '1010215539', '1016098923', '72002153', '1016051882',
	'1016091477', '1072666410', '80826699', '1152200960', '1234988522'
) AND CelulaId IS NULL;
PRINT '  → Maya: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Aurora
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Aurora')
WHERE Cedula IN (
	'1012317652', '1082843183', '80127568'
) AND CelulaId IS NULL;
PRINT '  → Aurora: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Polaris Software Team
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Polaris Software Team')
WHERE Cedula IN (
	'52765886', '1031176153', '1018491399', '1023009270', '1015428579',
	'1030569638', '1072666410', '1140876545', '52533807', '1070004328',
	'1001998132', '80826699'
) AND CelulaId IS NULL;
PRINT '  → Polaris Software Team: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Seguridad
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Seguridad')
WHERE Cedula IN ('1082864596') AND CelulaId IS NULL;
PRINT '  → Seguridad: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Administrativo
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Administrativo')
WHERE Cedula IN ('1010093635') AND CelulaId IS NULL;
PRINT '  → Administrativo: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Transversal Calidad
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Transversal Calidad')
WHERE Cedula IN ('1033776027', '1110484616', '1023900544') AND CelulaId IS NULL;
PRINT '  → Transversal Calidad: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Direccion Desarrollo
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Direccion Desarrollo')
WHERE Cedula IN (
	'1073324641', '1006661952', '1061819651', '1022938000', '1031809624',
	'79694723'
) AND CelulaId IS NULL;
PRINT '  → Direccion Desarrollo: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Facturador
UPDATE Consultores 
SET CelulaId = (SELECT Id FROM Celulas WHERE Nombre = 'Facturador')
WHERE Cedula IN ('1035428518', '14327144') AND CelulaId IS NULL;
PRINT '  → Facturador: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Asignar "Sin Asignar" a los que no tienen célula
DECLARE @SinAsignarId INT = (SELECT Id FROM Celulas WHERE Nombre = 'Sin Asignar');
IF @SinAsignarId IS NOT NULL
BEGIN
	UPDATE Consultores 
	SET CelulaId = @SinAsignarId
	WHERE CelulaId IS NULL AND Estado = 'Activo';
	IF @@ROWCOUNT > 0
		PRINT '  → Sin Asignar: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores (SIN CÉLULA EN CSV)';
END

PRINT '';
PRINT '✅ Asignación de consultores completada';

-- ============================================================================
-- PARTE 4: VALIDACIONES
-- ============================================================================

PRINT '';
PRINT '🔍 Ejecutando validaciones...';
PRINT '';

-- Validación 1: Consultores activos sin célula
DECLARE @SinCelula INT = (SELECT COUNT(*) FROM Consultores WHERE CelulaId IS NULL AND Estado = 'Activo');
PRINT '📊 Consultores activos sin célula: ' + CAST(@SinCelula AS NVARCHAR(10));

-- Validación 2: Células sin líder
PRINT '';
PRINT '📊 Células sin líder:';
SELECT c.Nombre, c.Descripcion
FROM Celulas c
LEFT JOIN CelulaLider cl ON c.Id = cl.CelulaId
WHERE cl.CelulaId IS NULL AND c.Activa = 1 AND c.Nombre != 'Sin Asignar';

-- Validación 3: Distribución por célula
PRINT '';
PRINT '📊 Distribución de consultores por célula:';
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

-- Validación 4: Líderes por célula
PRINT '';
PRINT '📊 Líderes por célula:';
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

-- Validación 5: Consultores con múltiples registros en CSV (duplicados)
PRINT '';
PRINT '📊 Consultores que aparecen en múltiples células (según CSV):';
PRINT '  → Cecilio Trinidad (779729): Enterprise Team + Wakanda';
PRINT '  → Hugo Bermudez (79568718): Enterprise Team + Wakanda';
PRINT '  → Esneider Gualtero (1072666410): Polaris + Maya';
PRINT '  → Diana Saavedra (53006451): Enterprise Team + Wakanda';
PRINT '  → Robert Ramirez (80127568): DEVSECOPS + Aurora + Nova';
PRINT '  → Diego Montenegro (80088963): Bon Voyage + Data Stargazers';
PRINT '  → Alejandra Jimenez (1019075102): MindShift + Data Stargazers';
PRINT '  → Andres Rojas (80826699): Maya + Polaris';
PRINT '';
PRINT '⚠️ Nota: Estos consultores tienen CelulaId asignado al PRIMER registro encontrado.';
PRINT '⚠️ Si deseas modelo flexible con múltiples células, ejecuta MigracionEquipoMiembroFlexible.sql';

PRINT '';
PRINT '========================================';
PRINT '✅ MIGRACIÓN DE CÉLULAS COMPLETADA';
PRINT '========================================';
GO

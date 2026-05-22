-- ============================================================================
-- SCRIPT DE MIGRACIÓN: CREACIÓN DE EQUIPOS Y ASIGNACIÓN DE CONSULTORES
-- Fecha: 2025-01
-- Descripción: Crea equipos identificados en el CSV y asigna consultores
-- ============================================================================

USE DataTeamDB;
GO

-- ============================================================================
-- PARTE 1: CREAR EQUIPOS
-- ============================================================================

PRINT '📦 Creando equipos...';

-- Insertar equipos solo si no existen
MERGE INTO Equipos AS Target
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
	('Dirección Desarrollo', 'Dirección de desarrollo', '#0EA5E9', 1),
	('Facturador', 'Equipo de facturación electrónica', '#22C55E', 1)
) AS Source (Nombre, Descripcion, Color, Activo)
ON Target.Nombre = Source.Nombre
WHEN NOT MATCHED THEN
	INSERT (Nombre, Descripcion, Color, Activo, FechaCreacion, FechaModificacion)
	VALUES (Source.Nombre, Source.Descripcion, Source.Color, Source.Activo, GETUTCDATE(), GETUTCDATE())
WHEN MATCHED THEN
	UPDATE SET 
		Descripcion = Source.Descripcion,
		Color = Source.Color,
		Activo = Source.Activo,
		FechaModificacion = GETUTCDATE();

PRINT '✅ Equipos creados/actualizados: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ============================================================================
-- PARTE 2: ASIGNAR LÍDERES A EQUIPOS
-- ============================================================================

PRINT '';
PRINT '👥 Asignando líderes a equipos...';

-- Alexander Castro → Multiple equipos
DECLARE @AlexanderCastro INT = (SELECT Id FROM Consultores WHERE Cedula = '79694723');

IF @AlexanderCastro IS NOT NULL
BEGIN
	INSERT INTO EquipoLider (EquipoId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT e.Id, @AlexanderCastro, GETUTCDATE(), 1
	FROM Equipos e
	WHERE e.Nombre IN (
		'Enterprise Team', 'Nova', 'Bon Voyage', 'MindShift', 'Wakanda',
		'Data Stargazers', 'Maya', 'Aurora', 'Polaris Software Team',
		'Seguridad', 'Administrativo', 'Dirección Desarrollo'
	)
	AND NOT EXISTS (
		SELECT 1 FROM EquipoLider el 
		WHERE el.EquipoId = e.Id AND el.ConsultorId = @AlexanderCastro
	);
	PRINT '✅ Alexander Castro asignado como líder';
END
ELSE
	PRINT '⚠️ Alexander Castro no encontrado (Cédula: 79694723)';

-- Cristhian Amezquita → DevSecOps, MindShift, Bon Voyage, Transversal Calidad
DECLARE @CristhianAmezquita INT = (SELECT Id FROM Consultores WHERE Cedula = '1032458577');

IF @CristhianAmezquita IS NOT NULL
BEGIN
	INSERT INTO EquipoLider (EquipoId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT e.Id, @CristhianAmezquita, GETUTCDATE(), 1
	FROM Equipos e
	WHERE e.Nombre IN ('DevSecOps', 'MindShift', 'Bon Voyage', 'Transversal Calidad')
	AND NOT EXISTS (
		SELECT 1 FROM EquipoLider el 
		WHERE el.EquipoId = e.Id AND el.ConsultorId = @CristhianAmezquita
	);
	PRINT '✅ Cristhian Amezquita asignado como líder';
END
ELSE
	PRINT '⚠️ Cristhian Amezquita no encontrado (Cédula: 1032458577)';

-- Robert Ramirez → DevSecOps, Aurora, Nova
DECLARE @RobertRamirez INT = (SELECT Id FROM Consultores WHERE Cedula = '80127568');

IF @RobertRamirez IS NOT NULL
BEGIN
	INSERT INTO EquipoLider (EquipoId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT e.Id, @RobertRamirez, GETUTCDATE(), 0 -- No principal porque ya hay otro líder
	FROM Equipos e
	WHERE e.Nombre IN ('DevSecOps', 'Aurora', 'Nova', 'Dirección Desarrollo')
	AND NOT EXISTS (
		SELECT 1 FROM EquipoLider el 
		WHERE el.EquipoId = e.Id AND el.ConsultorId = @RobertRamirez
	);
	PRINT '✅ Robert Ramirez asignado como líder';
END
ELSE
	PRINT '⚠️ Robert Ramirez no encontrado (Cédula: 80127568)';

-- Diego Montenegro → Bon Voyage, Data Stargazers
DECLARE @DiegoMontenegro INT = (SELECT Id FROM Consultores WHERE Cedula = '80088963');

IF @DiegoMontenegro IS NOT NULL
BEGIN
	INSERT INTO EquipoLider (EquipoId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT e.Id, @DiegoMontenegro, GETUTCDATE(), 0
	FROM Equipos e
	WHERE e.Nombre IN ('Bon Voyage', 'Data Stargazers')
	AND NOT EXISTS (
		SELECT 1 FROM EquipoLider el 
		WHERE el.EquipoId = e.Id AND el.ConsultorId = @DiegoMontenegro
	);
	PRINT '✅ Diego Montenegro asignado como líder';
END
ELSE
	PRINT '⚠️ Diego Montenegro no encontrado (Cédula: 80088963)';

-- Ingrid Porras → MindShift
DECLARE @IngridPorras INT = (SELECT Id FROM Consultores WHERE Cedula = '52414874');

IF @IngridPorras IS NOT NULL
BEGIN
	INSERT INTO EquipoLider (EquipoId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT e.Id, @IngridPorras, GETUTCDATE(), 0
	FROM Equipos e
	WHERE e.Nombre = 'MindShift'
	AND NOT EXISTS (
		SELECT 1 FROM EquipoLider el 
		WHERE el.EquipoId = e.Id AND el.ConsultorId = @IngridPorras
	);
	PRINT '✅ Ingrid Porras asignada como líder';
END
ELSE
	PRINT '⚠️ Ingrid Porras no encontrada (Cédula: 52414874)';

-- Maria Kamila Redondo → Wakanda
DECLARE @MariaRedondo INT = (SELECT Id FROM Consultores WHERE Cedula = '1020713148');

IF @MariaRedondo IS NOT NULL
BEGIN
	INSERT INTO EquipoLider (EquipoId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT e.Id, @MariaRedondo, GETUTCDATE(), 0
	FROM Equipos e
	WHERE e.Nombre = 'Wakanda'
	AND NOT EXISTS (
		SELECT 1 FROM EquipoLider el 
		WHERE el.EquipoId = e.Id AND el.ConsultorId = @MariaRedondo
	);
	PRINT '✅ Maria Kamila Redondo asignada como líder';
END
ELSE
	PRINT '⚠️ Maria Kamila Redondo no encontrada (Cédula: 1020713148)';

-- Hugo Bermudez → Enterprise Team, Wakanda
DECLARE @HugoBermudez INT = (SELECT Id FROM Consultores WHERE Cedula = '79568718');

IF @HugoBermudez IS NOT NULL
BEGIN
	INSERT INTO EquipoLider (EquipoId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT e.Id, @HugoBermudez, GETUTCDATE(), 0
	FROM Equipos e
	WHERE e.Nombre IN ('Enterprise Team', 'Wakanda')
	AND NOT EXISTS (
		SELECT 1 FROM EquipoLider el 
		WHERE el.EquipoId = e.Id AND el.ConsultorId = @HugoBermudez
	);
	PRINT '✅ Hugo Bermudez asignado como líder';
END
ELSE
	PRINT '⚠️ Hugo Bermudez no encontrado (Cédula: 79568718)';

-- Cesar Pachon → Data Stargazers
DECLARE @CesarPachon INT = (SELECT Id FROM Consultores WHERE Cedula = '1022355992');

IF @CesarPachon IS NOT NULL
BEGIN
	INSERT INTO EquipoLider (EquipoId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT e.Id, @CesarPachon, GETUTCDATE(), 0
	FROM Equipos e
	WHERE e.Nombre = 'Data Stargazers'
	AND NOT EXISTS (
		SELECT 1 FROM EquipoLider el 
		WHERE el.EquipoId = e.Id AND el.ConsultorId = @CesarPachon
	);
	PRINT '✅ Cesar Pachon asignado como líder';
END
ELSE
	PRINT '⚠️ Cesar Pachon no encontrado (Cédula: 1022355992)';

-- Karol Rubiano → Aurora
DECLARE @KarolRubiano INT = (SELECT Id FROM Consultores WHERE Cedula = '1082843183');

IF @KarolRubiano IS NOT NULL
BEGIN
	INSERT INTO EquipoLider (EquipoId, ConsultorId, FechaAsignacion, EsLiderPrincipal)
	SELECT e.Id, @KarolRubiano, GETUTCDATE(), 0
	FROM Equipos e
	WHERE e.Nombre = 'Aurora'
	AND NOT EXISTS (
		SELECT 1 FROM EquipoLider el 
		WHERE el.EquipoId = e.Id AND el.ConsultorId = @KarolRubiano
	);
	PRINT '✅ Karol Rubiano asignada como líder';
END
ELSE
	PRINT '⚠️ Karol Rubiano no encontrada (Cédula: 1082843183)';

PRINT '';
PRINT '✅ Asignación de líderes completada';

-- ============================================================================
-- PARTE 3: ASIGNAR CONSULTORES A EQUIPOS (EQUIPO PRINCIPAL)
-- ============================================================================

PRINT '';
PRINT '🔧 Asignando consultores a equipos principales...';

-- Enterprise Team
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Enterprise Team')
WHERE Cedula IN (
	'1023928928', '1030635496', '779729', '1040327881', '1030697393',
	'1105786797', '1000538611', '53006451', '1128417080', '1082843183',
	'1001344075', '79568718'
) AND EquipoId IS NULL;
PRINT '  → Enterprise Team: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Nova
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Nova')
WHERE Cedula IN (
	'1018471934', '1033720903', '80075269', '80920988', '1030608602',
	'1067946006', '1073238911', '1022333350', '80852689', '1144037797',
	'52856823', '1110487049', '80127568'
) AND EquipoId IS NULL;
PRINT '  → Nova: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Bon Voyage
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Bon Voyage')
WHERE Cedula IN (
	'1030635496', '1033720903', '43186711', '1235245612', '1214715394',
	'1083019159', '1110515353', '1111791640', '1032416617', '1040327881'
) AND EquipoId IS NULL;
PRINT '  → Bon Voyage: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- MindShift
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'MindShift')
WHERE Cedula IN (
	'80075269', '1085297237', '1023918569', '1033776027', '1073710856',
	'1016082141', '1233907650', '52414874', '1026294015', '1192910852',
	'1214724608', '1019075102'
) AND EquipoId IS NULL;
PRINT '  → MindShift: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Wakanda
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Wakanda')
WHERE Cedula IN (
	'1140870751', '779729', '1007165946', '1026288243', '1001344075',
	'53006451', '1012403476', '79568718', '1007449064', '1020713148'
) AND EquipoId IS NULL;
PRINT '  → Wakanda: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- DevSecOps
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'DevSecOps')
WHERE Cedula IN (
	'1000874819', '1003557777', '1022353132', '1032458577', '1053773898',
	'1040327881', '1041611184', '80127568'
) AND EquipoId IS NULL;
PRINT '  → DevSecOps: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Data Stargazers
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Data Stargazers')
WHERE Cedula IN (
	'1083023296', '1192794693', '1073238911', '1073238911', '1022355992',
	'52776078', '80088963', '1019075102'
) AND EquipoId IS NULL;
PRINT '  → Data Stargazers: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Maya
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Maya')
WHERE Cedula IN (
	'1234097180', '1016098923', '1014199932', '1010215539', '1016051882',
	'1041611184', '72002153', '1072666410', '80826699', '1152200960',
	'1234988522'
) AND EquipoId IS NULL;
PRINT '  → Maya: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Aurora
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Aurora')
WHERE Cedula IN (
	'1012317652', '1082843183', '80127568'
) AND EquipoId IS NULL;
PRINT '  → Aurora: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Polaris Software Team
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Polaris Software Team')
WHERE Cedula IN (
	'52765886', '1030569638', '1072666410', '1234988522', '1018491399',
	'1023009270', '1016051882', '1073710856', '1001998132', '52533807',
	'1140876545', '80826699'
) AND EquipoId IS NULL;
PRINT '  → Polaris Software Team: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Seguridad
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Seguridad')
WHERE Cedula IN ('1082864596') AND EquipoId IS NULL;
PRINT '  → Seguridad: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Administrativo
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Administrativo')
WHERE Cedula IN ('1010093635') AND EquipoId IS NULL;
PRINT '  → Administrativo: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Transversal Calidad
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Transversal Calidad')
WHERE Cedula IN ('1033776027', '1110484616', '1023900544') AND EquipoId IS NULL;
PRINT '  → Transversal Calidad: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Dirección Desarrollo
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Dirección Desarrollo')
WHERE Cedula IN (
	'1073324641', '1006661952', '1061819651', '1022938000', '1031809624',
	'79694723', '1007449064'
) AND EquipoId IS NULL;
PRINT '  → Dirección Desarrollo: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Facturador
UPDATE Consultores 
SET EquipoId = (SELECT Id FROM Equipos WHERE Nombre = 'Facturador')
WHERE Cedula IN ('1035428518', '14327144') AND EquipoId IS NULL;
PRINT '  → Facturador: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';

-- Asignar "Sin Asignar" a los que no tienen equipo
DECLARE @SinAsignarId INT = (SELECT Id FROM Equipos WHERE Nombre = 'Sin Asignar');
IF @SinAsignarId IS NOT NULL
BEGIN
	UPDATE Consultores 
	SET EquipoId = @SinAsignarId
	WHERE EquipoId IS NULL AND Estado = 'Activo';
	PRINT '  → Sin Asignar: ' + CAST(@@ROWCOUNT AS NVARCHAR(10)) + ' consultores';
END

PRINT '';
PRINT '✅ Asignación de consultores completada';

-- ============================================================================
-- PARTE 4: VALIDACIONES
-- ============================================================================

PRINT '';
PRINT '🔍 Ejecutando validaciones...';
PRINT '';

-- Validación 1: Consultores activos sin equipo
DECLARE @SinEquipo INT = (SELECT COUNT(*) FROM Consultores WHERE EquipoId IS NULL AND Estado = 'Activo');
PRINT '📊 Consultores activos sin equipo: ' + CAST(@SinEquipo AS NVARCHAR(10));

-- Validación 2: Equipos sin líder
PRINT '📊 Equipos sin líder:';
SELECT e.Nombre
FROM Equipos e
LEFT JOIN EquipoLider el ON e.Id = el.EquipoId
WHERE el.EquipoId IS NULL AND e.Activo = 1;

-- Validación 3: Distribución por equipo
PRINT '';
PRINT '📊 Distribución de consultores por equipo:';
SELECT 
	e.Nombre AS Equipo,
	COUNT(c.Id) AS TotalMiembros,
	COUNT(CASE WHEN c.Estado = 'Activo' THEN 1 END) AS Activos
FROM Equipos e
LEFT JOIN Consultores c ON c.EquipoId = e.Id
WHERE e.Activo = 1
GROUP BY e.Id, e.Nombre
ORDER BY TotalMiembros DESC;

-- Validación 4: Líderes por equipo
PRINT '';
PRINT '📊 Líderes por equipo:';
SELECT 
	e.Nombre AS Equipo,
	c.Nombre AS Lider,
	c.Cedula,
	el.EsLiderPrincipal
FROM Equipos e
INNER JOIN EquipoLider el ON e.Id = el.EquipoId
INNER JOIN Consultores c ON el.ConsultorId = c.Id
ORDER BY e.Nombre, el.EsLiderPrincipal DESC;

PRINT '';
PRINT '========================================';
PRINT '✅ MIGRACIÓN COMPLETADA';
PRINT '========================================';
GO

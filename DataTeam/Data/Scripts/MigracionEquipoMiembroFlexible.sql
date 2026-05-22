-- ============================================================================
-- SCRIPT OPCIONAL: MODELO FLEXIBLE CON TABLA EquipoMiembro
-- Descripción: Permite múltiples equipos con % participación
-- ============================================================================

USE DataTeamDB;
GO

-- ============================================================================
-- CREAR TABLA EquipoMiembro
-- ============================================================================

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'EquipoMiembro')
BEGIN
	CREATE TABLE EquipoMiembro (
		Id INT IDENTITY(1,1) PRIMARY KEY,
		EquipoId INT NOT NULL,
		ConsultorId INT NOT NULL,
		PorcentajeParticipacion INT NOT NULL DEFAULT 100,
		EsMiembroPrincipal BIT NOT NULL DEFAULT 0,
		FechaAsignacion DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
		FechaDesasignacion DATETIME2 NULL,
		Activo BIT NOT NULL DEFAULT 1,

		CONSTRAINT FK_EquipoMiembro_Equipo FOREIGN KEY (EquipoId) 
			REFERENCES Equipos(Id) ON DELETE CASCADE,
		CONSTRAINT FK_EquipoMiembro_Consultor FOREIGN KEY (ConsultorId) 
			REFERENCES Consultores(Id) ON DELETE CASCADE,
		CONSTRAINT UQ_EquipoMiembro_Consultor UNIQUE (EquipoId, ConsultorId),
		CONSTRAINT CK_EquipoMiembro_Porcentaje CHECK (PorcentajeParticipacion > 0 AND PorcentajeParticipacion <= 100)
	);

	CREATE INDEX IX_EquipoMiembro_Equipo ON EquipoMiembro(EquipoId);
	CREATE INDEX IX_EquipoMiembro_Consultor ON EquipoMiembro(ConsultorId);
	CREATE INDEX IX_EquipoMiembro_Activo ON EquipoMiembro(Activo);

	PRINT '✅ Tabla EquipoMiembro creada';
END
ELSE
BEGIN
	PRINT '⚠️ Tabla EquipoMiembro ya existe';
END
GO

-- ============================================================================
-- POBLAR EquipoMiembro CON ASIGNACIONES ACTUALES
-- ============================================================================

PRINT '';
PRINT '📦 Poblando EquipoMiembro con asignaciones actuales...';

-- Insertar todos los consultores con su equipo principal
INSERT INTO EquipoMiembro (EquipoId, ConsultorId, PorcentajeParticipacion, EsMiembroPrincipal, FechaAsignacion)
SELECT 
	c.EquipoId,
	c.Id,
	100, -- Por defecto 100% si solo tiene un equipo
	1,   -- Es miembro principal
	GETUTCDATE()
FROM Consultores c
WHERE c.EquipoId IS NOT NULL
AND NOT EXISTS (
	SELECT 1 FROM EquipoMiembro em 
	WHERE em.EquipoId = c.EquipoId AND em.ConsultorId = c.Id
);

PRINT '✅ Consultores insertados: ' + CAST(@@ROWCOUNT AS NVARCHAR(10));

-- ============================================================================
-- ASIGNACIONES MÚLTIPLES (CASOS ESPECIALES)
-- ============================================================================

PRINT '';
PRINT '🔄 Procesando asignaciones múltiples...';

-- Cecilio Trinidad: Enterprise Team (50%) + Wakanda (50%)
DECLARE @Cecilio INT = (SELECT Id FROM Consultores WHERE Cedula = '779729');
DECLARE @EnterpriseTeam INT = (SELECT Id FROM Equipos WHERE Nombre = 'Enterprise Team');
DECLARE @Wakanda INT = (SELECT Id FROM Equipos WHERE Nombre = 'Wakanda');

IF @Cecilio IS NOT NULL AND @EnterpriseTeam IS NOT NULL AND @Wakanda IS NOT NULL
BEGIN
	-- Actualizar porcentaje en Enterprise Team (principal)
	UPDATE EquipoMiembro 
	SET PorcentajeParticipacion = 50, EsMiembroPrincipal = 1
	WHERE ConsultorId = @Cecilio AND EquipoId = @EnterpriseTeam;

	-- Agregar Wakanda (50%)
	IF NOT EXISTS (SELECT 1 FROM EquipoMiembro WHERE ConsultorId = @Cecilio AND EquipoId = @Wakanda)
	BEGIN
		INSERT INTO EquipoMiembro (EquipoId, ConsultorId, PorcentajeParticipacion, EsMiembroPrincipal)
		VALUES (@Wakanda, @Cecilio, 50, 0);
	END
	PRINT '  ✅ Cecilio Trinidad: Enterprise Team (50%) + Wakanda (50%)';
END

-- Hugo Bermudez: Enterprise Team (50%) + Wakanda (50%)
DECLARE @Hugo INT = (SELECT Id FROM Consultores WHERE Cedula = '79568718');

IF @Hugo IS NOT NULL AND @EnterpriseTeam IS NOT NULL AND @Wakanda IS NOT NULL
BEGIN
	UPDATE EquipoMiembro 
	SET PorcentajeParticipacion = 50, EsMiembroPrincipal = 1
	WHERE ConsultorId = @Hugo AND EquipoId = @EnterpriseTeam;

	IF NOT EXISTS (SELECT 1 FROM EquipoMiembro WHERE ConsultorId = @Hugo AND EquipoId = @Wakanda)
	BEGIN
		INSERT INTO EquipoMiembro (EquipoId, ConsultorId, PorcentajeParticipacion, EsMiembroPrincipal)
		VALUES (@Wakanda, @Hugo, 50, 0);
	END
	PRINT '  ✅ Hugo Bermudez: Enterprise Team (50%) + Wakanda (50%)';
END

-- Esneider Gualtero: Polaris (50%) + Maya (50%)
DECLARE @Esneider INT = (SELECT Id FROM Consultores WHERE Cedula = '1072666410');
DECLARE @Polaris INT = (SELECT Id FROM Equipos WHERE Nombre = 'Polaris Software Team');
DECLARE @Maya INT = (SELECT Id FROM Equipos WHERE Nombre = 'Maya');

IF @Esneider IS NOT NULL AND @Polaris IS NOT NULL AND @Maya IS NOT NULL
BEGIN
	UPDATE EquipoMiembro 
	SET PorcentajeParticipacion = 50, EsMiembroPrincipal = 1
	WHERE ConsultorId = @Esneider AND EquipoId = @Polaris;

	IF NOT EXISTS (SELECT 1 FROM EquipoMiembro WHERE ConsultorId = @Esneider AND EquipoId = @Maya)
	BEGIN
		INSERT INTO EquipoMiembro (EquipoId, ConsultorId, PorcentajeParticipacion, EsMiembroPrincipal)
		VALUES (@Maya, @Esneider, 50, 0);
	END
	PRINT '  ✅ Esneider Gualtero: Polaris (50%) + Maya (50%)';
END

-- Diana Saavedra: Enterprise Team (50%) + Wakanda (50%)
DECLARE @Diana INT = (SELECT Id FROM Consultores WHERE Cedula = '53006451');

IF @Diana IS NOT NULL AND @EnterpriseTeam IS NOT NULL AND @Wakanda IS NOT NULL
BEGIN
	UPDATE EquipoMiembro 
	SET PorcentajeParticipacion = 50, EsMiembroPrincipal = 1
	WHERE ConsultorId = @Diana AND EquipoId = @EnterpriseTeam;

	IF NOT EXISTS (SELECT 1 FROM EquipoMiembro WHERE ConsultorId = @Diana AND EquipoId = @Wakanda)
	BEGIN
		INSERT INTO EquipoMiembro (EquipoId, ConsultorId, PorcentajeParticipacion, EsMiembroPrincipal)
		VALUES (@Wakanda, @Diana, 50, 0);
	END
	PRINT '  ✅ Diana Saavedra: Enterprise Team (50%) + Wakanda (50%)';
END

-- Alejandra Jimenez: MindShift (50%) + Data Stargazers (50%)
DECLARE @Alejandra INT = (SELECT Id FROM Consultores WHERE Cedula = '1019075102');
DECLARE @MindShift INT = (SELECT Id FROM Equipos WHERE Nombre = 'MindShift');
DECLARE @DataStargazers INT = (SELECT Id FROM Equipos WHERE Nombre = 'Data Stargazers');

IF @Alejandra IS NOT NULL AND @MindShift IS NOT NULL AND @DataStargazers IS NOT NULL
BEGIN
	UPDATE EquipoMiembro 
	SET PorcentajeParticipacion = 50, EsMiembroPrincipal = 1
	WHERE ConsultorId = @Alejandra AND EquipoId = @MindShift;

	IF NOT EXISTS (SELECT 1 FROM EquipoMiembro WHERE ConsultorId = @Alejandra AND EquipoId = @DataStargazers)
	BEGIN
		INSERT INTO EquipoMiembro (EquipoId, ConsultorId, PorcentajeParticipacion, EsMiembroPrincipal)
		VALUES (@DataStargazers, @Alejandra, 50, 0);
	END
	PRINT '  ✅ Alejandra Jimenez: MindShift (50%) + Data Stargazers (50%)';
END

-- Andres Rojas: Maya (50%) + Polaris (50%)
DECLARE @Andres INT = (SELECT Id FROM Consultores WHERE Cedula = '80826699');

IF @Andres IS NOT NULL AND @Maya IS NOT NULL AND @Polaris IS NOT NULL
BEGIN
	UPDATE EquipoMiembro 
	SET PorcentajeParticipacion = 50, EsMiembroPrincipal = 1
	WHERE ConsultorId = @Andres AND EquipoId = @Maya;

	IF NOT EXISTS (SELECT 1 FROM EquipoMiembro WHERE ConsultorId = @Andres AND EquipoId = @Polaris)
	BEGIN
		INSERT INTO EquipoMiembro (EquipoId, ConsultorId, PorcentajeParticipacion, EsMiembroPrincipal)
		VALUES (@Polaris, @Andres, 50, 0);
	END
	PRINT '  ✅ Andres Rojas: Maya (50%) + Polaris (50%)';
END

PRINT '';
PRINT '✅ Asignaciones múltiples procesadas';

-- ============================================================================
-- VALIDACIONES
-- ============================================================================

PRINT '';
PRINT '🔍 Validaciones del modelo flexible...';
PRINT '';

-- Validación 1: Consultores con suma de porcentajes != 100%
PRINT '📊 Consultores con % participación != 100%:';
SELECT 
	c.Cedula,
	c.Nombre,
	SUM(em.PorcentajeParticipacion) AS TotalPorcentaje
FROM Consultores c
INNER JOIN EquipoMiembro em ON c.Id = em.ConsultorId
WHERE em.Activo = 1
GROUP BY c.Id, c.Cedula, c.Nombre
HAVING SUM(em.PorcentajeParticipacion) != 100;

-- Validación 2: Consultores con múltiples equipos
PRINT '';
PRINT '📊 Consultores con múltiples equipos:';
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

-- Validación 3: Distribución por equipo (usando EquipoMiembro)
PRINT '';
PRINT '📊 Distribución de consultores por equipo (modelo flexible):';
SELECT 
	e.Nombre AS Equipo,
	COUNT(DISTINCT em.ConsultorId) AS TotalMiembros,
	CAST(AVG(CAST(em.PorcentajeParticipacion AS FLOAT)) AS DECIMAL(5,2)) AS PromedioParticipacion
FROM Equipos e
LEFT JOIN EquipoMiembro em ON e.Id = em.EquipoId AND em.Activo = 1
WHERE e.Activo = 1
GROUP BY e.Id, e.Nombre
ORDER BY TotalMiembros DESC;

PRINT '';
PRINT '========================================';
PRINT '✅ MODELO FLEXIBLE IMPLEMENTADO';
PRINT '========================================';
GO

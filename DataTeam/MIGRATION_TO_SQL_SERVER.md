# 📋 INSTRUCCIONES PARA CAMBIAR A SQL SERVER EN PRODUCCIÓN

Este documento explica cómo migrar de la base de datos **InMemory** (desarrollo) a **SQL Server** (producción).

---

## ⚠️ ESTADO ACTUAL
- ✅ Base de datos **InMemory** activa (desarrollo)
- ✅ Código SQL Server **comentado** y listo
- ✅ Todo funciona localmente SIN SQL Server

---

## 🚀 PASOS PARA ACTIVAR SQL SERVER EN PRODUCCIÓN

### **1️⃣ Configurar la Cadena de Conexión**

Editar el archivo `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=TU_SERVIDOR;Database=DataTeamDB;User Id=TU_USUARIO;Password=TU_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=True;Encrypt=True"
  }
}
```

**Ejemplos:**

#### 🔹 Azure SQL Database
```
Server=tcp:miservidor.database.windows.net,1433;Database=DataTeamDB;User Id=admin@miservidor;Password=MiContraseña123!;
```

#### 🔹 SQL Server Local (Autenticación Windows)
```
Server=SERVIDOR\\SQLEXPRESS;Database=DataTeamDB;Integrated Security=true;MultipleActiveResultSets=true;TrustServerCertificate=True
```

#### 🔹 SQL Server Local (Usuario/Contraseña)
```
Server=localhost;Database=DataTeamDB;User Id=sa;Password=MiContraseña123!;MultipleActiveResultSets=true;TrustServerCertificate=True
```

---

### **2️⃣ Descomentar el Código SQL en `Program.cs`**

#### 📍 **Líneas 18-37** - Cambiar configuración de DbContext

**COMENTAR ESTO (InMemory):**
```csharp
// ----- OPCIÓN 1: BASE DE DATOS EN MEMORIA (ACTUAL - DESARROLLO) -----
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseInMemoryDatabase("DataTeamInMemoryDB"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
```

**DESCOMENTAR ESTO (SQL Server):**
```csharp
// ----- OPCIÓN 2: SQL SERVER (PRODUCCIÓN) -----
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseSqlServer(connectionString,
		sqlOptions => sqlOptions.EnableRetryOnFailure(
			maxRetryCount: 5,
			maxRetryDelay: TimeSpan.FromSeconds(30),
			errorNumbersToAdd: null)));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
```

---

#### 📍 **Líneas 50-78** - Cambiar Hangfire Storage

**COMENTAR ESTO (InMemory):**
```csharp
// ----- OPCIÓN 1: HANGFIRE CON MEMORIA (ACTUAL - DESARROLLO) -----
builder.Services.AddHangfire(configuration => configuration
	.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
	.UseInMemoryStorage());
```

**DESCOMENTAR ESTO (SQL Server):**
```csharp
// ----- OPCIÓN 2: HANGFIRE CON SQL SERVER (PRODUCCIÓN) -----
builder.Services.AddHangfire(configuration => configuration
	.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
	.UseSimpleAssemblyNameTypeSerializer()
	.UseRecommendedSerializerSettings()
	.UseSqlServerStorage(connectionString, new Hangfire.SqlServer.SqlServerStorageOptions
	{
		CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
		SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
		QueuePollInterval = TimeSpan.Zero,
		UseRecommendedIsolationLevel = true,
		DisableGlobalLocks = true
	}));
```

---

### **3️⃣ Crear la Base de Datos con Migraciones**

Abrir **PowerShell** en la raíz del proyecto y ejecutar:

```powershell
# Crear la primera migración
dotnet ef migrations add InitialCreate

# Aplicar las migraciones y crear la base de datos
dotnet ef database update
```

**✅ Esto creará:**
- La base de datos `DataTeamDB`
- Todas las tablas (Consultores, Celulas, AuditoriaLogs, etc.)
- Índices y relaciones
- Tablas de Identity (Usuarios, Roles)
- Tablas de Hangfire (trabajos programados)

---

### **4️⃣ Verificar que Todo Funciona**

1. **Ejecutar la aplicación:**
   ```powershell
   dotnet run
   ```

2. **Verificar conexión a SQL Server:**
   - Los logs deberían mostrar: `"Aplicación iniciada con SQL Server"`
   - Verificar en SQL Server Management Studio (SSMS) que la base de datos existe

3. **Verificar Hangfire Dashboard:**
   - Ir a: `https://localhost:5001/hangfire`
   - Deben aparecer los trabajos programados

4. **Verificar datos de empleados:**
   - Los datos del CSV se cargarán automáticamente en la base de datos

---

## 🔄 CAMBIOS QUE SE APLICARÁN

| **Característica**          | **Desarrollo (InMemory)** | **Producción (SQL Server)** |
|-----------------------------|---------------------------|------------------------------|
| **Persistencia de datos**   | ❌ Se pierden al reiniciar | ✅ Datos persistentes        |
| **Trabajos Hangfire**        | ❌ Se pierden al reiniciar | ✅ Persistentes              |
| **Requiere SQL Server**      | ❌ No                      | ✅ Sí                        |
| **Migraciones**              | ❌ No necesarias           | ✅ Necesarias                |
| **Rendimiento**              | ⚡ Rápido (todo en RAM)    | 🗄️ Producción-ready         |

---

## 📦 PAQUETES YA INSTALADOS

✅ Todos los paquetes necesarios ya están en el proyecto:
- `Microsoft.EntityFrameworkCore.SqlServer` (10.0.8)
- `Hangfire.SqlServer` (1.8.20)
- `Microsoft.EntityFrameworkCore.Design` (10.0.7)
- `Microsoft.EntityFrameworkCore.Tools` (10.0.7)

**NO se necesita instalar nada adicional.**

---

## 🛠️ COMANDOS ÚTILES DE ENTITY FRAMEWORK

```powershell
# Ver lista de migraciones
dotnet ef migrations list

# Crear una nueva migración (después de cambios en modelos)
dotnet ef migrations add NombreDeLaMigracion

# Aplicar migraciones pendientes
dotnet ef database update

# Revertir a una migración específica
dotnet ef database update NombreDeLaMigracion

# Eliminar la última migración (si no se ha aplicado)
dotnet ef migrations remove

# Ver el SQL que generará una migración
dotnet ef migrations script

# Eliminar la base de datos (⚠️ CUIDADO - borra todos los datos)
dotnet ef database drop
```

---

## 🔒 SEGURIDAD EN PRODUCCIÓN

### **⚠️ NO hardcodear contraseñas en `appsettings.Production.json`**

**Usar User Secrets en desarrollo:**
```powershell
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Password=MiPassword;"
```

**Usar Variables de Entorno en producción (Azure/IIS):**
```
ConnectionStrings__DefaultConnection=Server=...;Password=MiPassword;
```

---

## ✅ CHECKLIST FINAL

Antes de desplegar en producción, verificar:

- [ ] Cadena de conexión configurada en `appsettings.Production.json`
- [ ] Código InMemory **comentado** en `Program.cs`
- [ ] Código SQL Server **descomentado** en `Program.cs`
- [ ] Migraciones ejecutadas: `dotnet ef database update`
- [ ] Base de datos creada en SQL Server
- [ ] Aplicación ejecutándose correctamente
- [ ] Hangfire Dashboard funcionando (`/hangfire`)
- [ ] Datos de empleados cargados desde CSV
- [ ] Contraseñas en User Secrets o Variables de Entorno (NO en código)

---

## 🆘 SOLUCIÓN DE PROBLEMAS

### **Error: "Cannot open database 'DataTeamDB'"**
✅ Ejecutar: `dotnet ef database update`

### **Error: "Login failed for user"**
✅ Verificar usuario y contraseña en la cadena de conexión

### **Error: "A network-related or instance-specific error"**
✅ Verificar que SQL Server esté corriendo y el nombre del servidor sea correcto

### **Error: "The EntityFrameworkCore tools version is older"**
✅ Ejecutar: `dotnet tool update --global dotnet-ef`

---

## 📞 SOPORTE

Si tienes problemas con la migración:
1. Revisar los logs de la aplicación
2. Verificar que SQL Server esté accesible
3. Revisar los permisos del usuario de la base de datos
4. Consultar la documentación oficial: https://learn.microsoft.com/ef/core/

---

**🎉 ¡Listo! Con estos pasos tu aplicación estará funcionando con SQL Server en producción.**

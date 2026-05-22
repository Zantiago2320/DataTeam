# 🚀 GUÍA DE PRODUCCIÓN: SISTEMA DE CORREOS EN NUBE/SERVIDOR

## 📋 ÍNDICE
1. [Servicios SMTP Profesionales](#servicios-smtp-profesionales)
2. [Configuración Segura de Credenciales](#configuración-segura-de-credenciales)
3. [Despliegue en Azure App Service](#despliegue-en-azure-app-service)
4. [Despliegue en Servidor IIS Windows](#despliegue-en-servidor-iis-windows)
5. [Configuración de Hangfire en Producción](#configuración-de-hangfire-en-producción)
6. [Monitoreo y Troubleshooting](#monitoreo-y-troubleshooting)
7. [Checklist de Despliegue](#checklist-de-despliegue)

---

## 🌐 SERVICIOS SMTP PROFESIONALES

### ❌ **NO usar en producción:**
- Gmail personal (límite de 500 correos/día, puede bloquear la cuenta)
- Outlook.com personal (límite de 300 correos/día)
- Servicios gratuitos no confiables

### ✅ **Opciones recomendadas para producción:**

---

### **1. Azure Communication Services Email** (RECOMENDADO para Azure)

#### **Ventajas:**
- ✅ Integración nativa con Azure
- ✅ 10,000 correos/mes GRATIS
- ✅ Alta deliverability (tasa de entrega)
- ✅ Soporte para dominio personalizado
- ✅ Dashboard con métricas en tiempo real

#### **Costos:**
```
Gratis: 10,000 correos/mes
Adicionales: $0.05 USD por cada 100 correos
```

#### **Configuración:**
```json
"Email": {
  "Provider": "AzureCommunicationServices",
  "AzureCS": {
	"ConnectionString": "endpoint=https://tu-recurso.communication.azure.com/;accesskey=...",
	"SenderAddress": "DoNotReply@tu-dominio-verificado.com"
  }
}
```

#### **Código de integración:**
```csharp
// Instalar: dotnet add package Azure.Communication.Email

using Azure.Communication.Email;

public class AzureEmailService : IEmailService
{
	private readonly EmailClient _emailClient;

	public AzureEmailService(IConfiguration configuration)
	{
		var connectionString = configuration["Email:AzureCS:ConnectionString"];
		_emailClient = new EmailClient(connectionString);
	}

	public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
	{
		var emailMessage = new EmailMessage(
			senderAddress: configuration["Email:AzureCS:SenderAddress"],
			recipientAddress: destinatario,
			content: new EmailContent(asunto) { Html = cuerpoHtml }
		);

		await _emailClient.SendAsync(WaitUntil.Started, emailMessage);
	}
}
```

---

### **2. SendGrid** (Popular y Confiable)

#### **Ventajas:**
- ✅ 100 correos/día GRATIS (3,000/mes)
- ✅ Excelente documentación
- ✅ APIs robustas con SDKs para .NET
- ✅ Analytics avanzado
- ✅ Plantillas HTML integradas

#### **Costos:**
```
Gratis: 100 correos/día
Essentials: $19.95/mes (hasta 50,000 correos/mes)
Pro: Desde $89.95/mes
```

#### **Configuración:**
```json
"Email": {
  "Provider": "SendGrid",
  "SendGrid": {
	"ApiKey": "SG.xxxxxxxxxxxxxxxxxxxxx",
	"FromEmail": "noreply@tuempresa.com",
	"FromName": "DataTeam"
  }
}
```

#### **Código de integración:**
```csharp
// Instalar: dotnet add package SendGrid

using SendGrid;
using SendGrid.Helpers.Mail;

public class SendGridEmailService : IEmailService
{
	private readonly SendGridClient _client;
	private readonly IConfiguration _configuration;

	public SendGridEmailService(IConfiguration configuration)
	{
		_configuration = configuration;
		var apiKey = configuration["Email:SendGrid:ApiKey"];
		_client = new SendGridClient(apiKey);
	}

	public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
	{
		var from = new EmailAddress(
			_configuration["Email:SendGrid:FromEmail"],
			_configuration["Email:SendGrid:FromName"]
		);
		var to = new EmailAddress(destinatario);
		var msg = MailHelper.CreateSingleEmail(from, to, asunto, "", cuerpoHtml);

		var response = await _client.SendEmailAsync(msg);

		if (!response.IsSuccessStatusCode)
		{
			throw new Exception($"Error al enviar correo: {response.StatusCode}");
		}
	}
}
```

---

### **3. Office 365 / Microsoft 365** (Para empresas con cuenta corporativa)

#### **Ventajas:**
- ✅ Ya incluido si la empresa tiene licencias M365
- ✅ Autenticación con cuenta corporativa
- ✅ Envío desde correo real de empleado
- ✅ Cumple políticas de seguridad empresarial

#### **Configuración:**
```json
"Email": {
  "Provider": "Office365",
  "SmtpHost": "smtp.office365.com",
  "SmtpPort": "587",
  "SmtpUser": "correo-aplicacion@tuempresa.com",
  "SmtpPassword": "password-desde-keyvault"
}
```

#### **Límites:**
```
Office 365 Business: 10,000 correos/día
Exchange Online: 30 mensajes/minuto
```

---

### **4. AWS SES (Simple Email Service)** (Para despliegue en AWS)

#### **Ventajas:**
- ✅ Muy económico ($0.10 por cada 1,000 correos)
- ✅ 62,000 correos/mes GRATIS (desde EC2)
- ✅ Alta escalabilidad

#### **Configuración:**
```json
"Email": {
  "Provider": "AWSSES",
  "AWS": {
	"AccessKeyId": "AKIAIOSFODNN7EXAMPLE",
	"SecretAccessKey": "desde-secrets-manager",
	"Region": "us-east-1",
	"FromAddress": "noreply@tuempresa.com"
  }
}
```

---

### **📊 Comparativa de Servicios:**

| Servicio | Gratis/Mes | Precio Adicional | Deliverability | Facilidad | Recomendado Para |
|----------|------------|------------------|----------------|-----------|------------------|
| **Azure CS** | 10,000 | $0.05/100 | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | Apps en Azure |
| **SendGrid** | 3,000 | $19.95/50K | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | Cualquier cloud |
| **Office 365** | 10,000/día | Incluido en M365 | ⭐⭐⭐⭐ | ⭐⭐⭐ | Empresas con M365 |
| **AWS SES** | 62,000* | $0.10/1,000 | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | Apps en AWS |

_*Gratis solo desde instancias EC2_

---

## 🔐 CONFIGURACIÓN SEGURA DE CREDENCIALES

### ❌ **NUNCA hacer esto en producción:**
```json
// appsettings.json - NO HACER ESTO
{
  "Email": {
	"SmtpPassword": "MiPasswordSecreta123"  // ❌ PELIGROSO
  }
}
```

### ✅ **Opciones seguras:**

---

### **Opción 1: Azure Key Vault** (MEJOR para Azure)

#### **Paso 1: Crear Key Vault en Azure**
```bash
# Azure CLI
az keyvault create \
  --name datateam-keyvault \
  --resource-group datateam-rg \
  --location eastus
```

#### **Paso 2: Agregar secrets**
```bash
az keyvault secret set \
  --vault-name datateam-keyvault \
  --name "Email--SmtpUser" \
  --value "correo@empresa.com"

az keyvault secret set \
  --vault-name datateam-keyvault \
  --name "Email--SmtpPassword" \
  --value "password-seguro"
```

#### **Paso 3: Habilitar Managed Identity en App Service**
```bash
az webapp identity assign \
  --name datateam-app \
  --resource-group datateam-rg
```

#### **Paso 4: Dar permisos al App Service**
```bash
az keyvault set-policy \
  --name datateam-keyvault \
  --object-id <managed-identity-object-id> \
  --secret-permissions get list
```

#### **Paso 5: Integrar en Program.cs**
```csharp
// Instalar: dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets

var builder = WebApplication.CreateBuilder(args);

// Configurar Key Vault
if (builder.Environment.IsProduction())
{
	var keyVaultUrl = builder.Configuration["KeyVaultUrl"]; // https://datateam-keyvault.vault.azure.net/

	builder.Configuration.AddAzureKeyVault(
		new Uri(keyVaultUrl),
		new DefaultAzureCredential()
	);
}

// Ahora puedes usar: _configuration["Email--SmtpPassword"]
```

---

### **Opción 2: Variables de Entorno** (Para IIS o Linux)

#### **En Azure App Service:**
```bash
# Portal Azure > App Service > Configuration > Application Settings

Email__SmtpUser = correo@empresa.com
Email__SmtpPassword = password-seguro
Email__TalentoHumano__0 = rrhh1@empresa.com
Email__TalentoHumano__1 = rrhh2@empresa.com
```

#### **En servidor Windows/IIS:**
```powershell
# PowerShell (como Administrador)
[System.Environment]::SetEnvironmentVariable(
	"Email__SmtpPassword",
	"password-seguro",
	[System.EnvironmentVariableTarget]::Machine
)

# Reiniciar IIS
iisreset
```

#### **En servidor Linux:**
```bash
# /etc/environment
export Email__SmtpUser="correo@empresa.com"
export Email__SmtpPassword="password-seguro"

# Recargar
source /etc/environment
sudo systemctl restart datateam.service
```

#### **Acceso desde código:**
```csharp
// ASP.NET Core lee automáticamente variables de entorno
var smtpPassword = builder.Configuration["Email:SmtpPassword"];

// O con valores por defecto
var smtpPort = builder.Configuration.GetValue<int>("Email:SmtpPort", 587);
```

---

### **Opción 3: User Secrets** (SOLO para desarrollo local)

```bash
# Solo para desarrollo local - NO USAR EN PRODUCCIÓN
dotnet user-secrets init
dotnet user-secrets set "Email:SmtpPassword" "password-dev"
```

---

## ☁️ DESPLIEGUE EN AZURE APP SERVICE

### **Arquitectura recomendada:**

```
Azure App Service (Web App)
	↓ usa
Azure SQL Database (Hangfire + datos)
	↓ usa
Azure Key Vault (credenciales)
	↓ envía con
Azure Communication Services (correos)
	↓ monitorea con
Application Insights (logs)
```

---

### **Paso 1: Crear recursos en Azure**

```bash
# Variables
RESOURCE_GROUP="datateam-rg"
LOCATION="eastus"
APP_NAME="datateam-app"
SQL_SERVER="datateam-sql"
DB_NAME="DataTeamDB"

# Crear grupo de recursos
az group create --name $RESOURCE_GROUP --location $LOCATION

# Crear App Service Plan (B1 mínimo para producción)
az appservice plan create \
  --name datateam-plan \
  --resource-group $RESOURCE_GROUP \
  --sku B1 \
  --is-linux

# Crear Web App
az webapp create \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --plan datateam-plan \
  --runtime "DOTNET|8.0"

# Crear SQL Server
az sql server create \
  --name $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --admin-user sqladmin \
  --admin-password "Secure-Password-123!"

# Crear base de datos
az sql db create \
  --name $DB_NAME \
  --server $SQL_SERVER \
  --resource-group $RESOURCE_GROUP \
  --service-objective S0 \
  --backup-storage-redundancy Local
```

---

### **Paso 2: Configurar Connection String en App Service**

```bash
# Desde Azure CLI
az webapp config connection-string set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --connection-string-type SQLAzure \
  --settings DefaultConnection="Server=tcp:$SQL_SERVER.database.windows.net,1433;Initial Catalog=$DB_NAME;Persist Security Info=False;User ID=sqladmin;Password=Secure-Password-123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

O desde **Portal Azure:**
```
App Service > Configuration > Connection strings
Name: DefaultConnection
Value: Server=tcp:datateam-sql.database.windows.net,1433;...
Type: SQLAzure
```

---

### **Paso 3: Configurar Application Settings (Variables de Entorno)**

```bash
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings \
	ASPNETCORE_ENVIRONMENT="Production" \
	Email__FromName="DataTeam" \
	Email__FromAddress="noreply@tuempresa.com" \
	Email__SmtpHost="smtp.office365.com" \
	Email__SmtpPort="587" \
	Email__TalentoHumano__0="rrhh1@empresa.com" \
	Email__TalentoHumano__1="rrhh2@empresa.com"
```

---

### **Paso 4: Configurar Managed Identity y Key Vault**

```bash
# Habilitar managed identity
az webapp identity assign \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP

# Crear Key Vault
az keyvault create \
  --name datateam-kv \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION

# Obtener object ID del managed identity
OBJECT_ID=$(az webapp identity show --name $APP_NAME --resource-group $RESOURCE_GROUP --query principalId -o tsv)

# Dar permisos
az keyvault set-policy \
  --name datateam-kv \
  --object-id $OBJECT_ID \
  --secret-permissions get list

# Agregar secrets
az keyvault secret set --vault-name datateam-kv --name "Email--SmtpUser" --value "correo@empresa.com"
az keyvault secret set --vault-name datateam-kv --name "Email--SmtpPassword" --value "password-seguro"

# Configurar URL del Key Vault en App Service
az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings KeyVaultUrl="https://datateam-kv.vault.azure.net/"
```

---

### **Paso 5: Habilitar Application Insights (Logs)**

```bash
# Crear recurso
az monitor app-insights component create \
  --app datateam-insights \
  --location $LOCATION \
  --resource-group $RESOURCE_GROUP

# Conectar con App Service
INSTRUMENTATION_KEY=$(az monitor app-insights component show --app datateam-insights --resource-group $RESOURCE_GROUP --query instrumentationKey -o tsv)

az webapp config appsettings set \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --settings APPLICATIONINSIGHTS_CONNECTION_STRING="InstrumentationKey=$INSTRUMENTATION_KEY"
```

---

### **Paso 6: Deploy desde Visual Studio o CLI**

#### **Opción A: Desde Visual Studio**
```
1. Click derecho en proyecto > Publish
2. Target: Azure
3. Specific target: Azure App Service (Windows/Linux)
4. Seleccionar tu App Service
5. Publish
```

#### **Opción B: Desde CLI**
```bash
# Publicar localmente
dotnet publish -c Release -o ./publish

# Crear zip
cd publish
zip -r ../app.zip .

# Subir a Azure
az webapp deployment source config-zip \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --src ../app.zip
```

#### **Opción C: CI/CD con GitHub Actions** (ver archivo en Step 7)

---

### **Paso 7: Ejecutar migraciones en SQL Azure**

```bash
# Opción 1: Desde local (con VPN o firewall abierto)
dotnet ef database update --connection "Server=tcp:datateam-sql.database.windows.net,..."

# Opción 2: Desde Kudu Console (https://datateam-app.scm.azurewebsites.net)
cd site/wwwroot
dotnet DataTeam.dll ef database update
```

---

## 🖥️ DESPLIEGUE EN SERVIDOR IIS WINDOWS

### **Requisitos previos:**
- Windows Server 2016 o superior
- IIS 10 instalado
- .NET 8 Hosting Bundle instalado
- SQL Server 2019 o superior

---

### **Paso 1: Instalar prerrequisitos**

```powershell
# Como Administrador

# Instalar IIS
Install-WindowsFeature -name Web-Server -IncludeManagementTools

# Descargar e instalar .NET 8 Hosting Bundle
Invoke-WebRequest -Uri "https://download.visualstudio.microsoft.com/download/pr/.../dotnet-hosting-8.0-win.exe" -OutFile "dotnet-hosting.exe"
Start-Process -FilePath "dotnet-hosting.exe" -Args "/quiet /norestart" -Wait

# Reiniciar IIS
iisreset
```

---

### **Paso 2: Publicar la aplicación**

```bash
# En tu máquina de desarrollo
dotnet publish -c Release -o C:\publish\DataTeam
```

---

### **Paso 3: Copiar archivos al servidor**

```powershell
# En el servidor, crear carpeta
New-Item -Path "C:\inetpub\datateam" -ItemType Directory

# Copiar archivos publicados a C:\inetpub\datateam
# (usar RDP, compartir red, o FTP)
```

---

### **Paso 4: Configurar Application Pool**

```powershell
Import-Module WebAdministration

# Crear Application Pool
New-WebAppPool -Name "DataTeamPool"

# Configurar .NET CLR
Set-ItemProperty -Path "IIS:\AppPools\DataTeamPool" -Name "managedRuntimeVersion" -Value ""

# Configurar identidad (usar cuenta de servicio)
Set-ItemProperty -Path "IIS:\AppPools\DataTeamPool" -Name "processModel.identityType" -Value "ApplicationPoolIdentity"
```

---

### **Paso 5: Crear sitio web en IIS**

```powershell
# Crear sitio
New-Website -Name "DataTeam" `
  -PhysicalPath "C:\inetpub\datateam" `
  -ApplicationPool "DataTeamPool" `
  -Port 80

# Agregar binding HTTPS (requiere certificado SSL)
New-WebBinding -Name "DataTeam" -IPAddress "*" -Port 443 -Protocol https

# Asignar certificado SSL
$cert = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object {$_.Subject -like "*tuempresa.com*"}
New-Item -Path "IIS:\SslBindings\0.0.0.0!443" -Value $cert
```

---

### **Paso 6: Configurar variables de entorno**

```powershell
# Configurar variables de sistema
[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production", [System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable("Email__SmtpUser", "correo@empresa.com", [System.EnvironmentVariableTarget]::Machine)
[System.Environment]::SetEnvironmentVariable("Email__SmtpPassword", "password-seguro", [System.EnvironmentVariableTarget]::Machine)

# Reiniciar IIS para aplicar cambios
iisreset
```

---

### **Paso 7: Configurar SQL Server**

```sql
-- Crear base de datos
CREATE DATABASE DataTeamDB;
GO

-- Crear usuario para la aplicación
CREATE LOGIN datateam_app WITH PASSWORD = 'Password-Seguro-123!';
GO

USE DataTeamDB;
CREATE USER datateam_app FOR LOGIN datateam_app;
ALTER ROLE db_owner ADD MEMBER datateam_app;
GO
```

```json
// appsettings.Production.json
{
  "ConnectionStrings": {
	"DefaultConnection": "Server=localhost;Database=DataTeamDB;User Id=datateam_app;Password=password-desde-variable-entorno;TrustServerCertificate=True"
  }
}
```

---

### **Paso 8: Dar permisos a carpetas**

```powershell
# Dar permisos al Application Pool
$acl = Get-Acl "C:\inetpub\datateam"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\DataTeamPool", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl "C:\inetpub\datateam" $acl

# Permisos específicos para uploads
New-Item -Path "C:\inetpub\datateam\wwwroot\uploads" -ItemType Directory -Force
$acl = Get-Acl "C:\inetpub\datateam\wwwroot\uploads"
$rule = New-Object System.Security.AccessControl.FileSystemAccessRule("IIS AppPool\DataTeamPool", "FullControl", "ContainerInherit,ObjectInherit", "None", "Allow")
$acl.SetAccessRule($rule)
Set-Acl "C:\inetpub\datateam\wwwroot\uploads" $acl
```

---

## ⚙️ CONFIGURACIÓN DE HANGFIRE EN PRODUCCIÓN

### **Migrar de InMemory a SQL Server**

#### **Antes (Desarrollo):**
```csharp
// Program.cs
builder.Services.AddHangfire(config => config
	.UseInMemoryStorage());  // ❌ NO USAR EN PRODUCCIÓN
```

#### **Después (Producción):**
```csharp
// Program.cs
builder.Services.AddHangfire(config =>
{
	if (builder.Environment.IsProduction())
	{
		// Usar SQL Server en producción
		config.UseSqlServerStorage(
			builder.Configuration.GetConnectionString("DefaultConnection"),
			new SqlServerStorageOptions
			{
				CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
				SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
				QueuePollInterval = TimeSpan.Zero,
				UseRecommendedIsolationLevel = true,
				DisableGlobalLocks = true,
				SchemaName = "Hangfire"
			});
	}
	else
	{
		// InMemory solo en desarrollo
		config.UseInMemoryStorage();
	}
});
```

---

### **Crear esquema de Hangfire en SQL Server**

```sql
-- Ejecutar solo la primera vez
-- Hangfire creará automáticamente las tablas al iniciar
```

O manualmente:

```bash
# Instalar herramienta CLI de Hangfire
dotnet tool install --global Hangfire.SqlServer.Tools

# Inicializar esquema
hangfire-sqlserver install --connection "Server=...;Database=DataTeamDB;..." --schema Hangfire
```

---

### **Proteger Dashboard de Hangfire**

```csharp
// Program.cs

// ❌ NO HACER ESTO EN PRODUCCIÓN:
app.UseHangfireDashboard("/hangfire");  // Acceso público

// ✅ HACER ESTO:
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
	Authorization = new[] { new HangfireAuthorizationFilter() }
});

// Crear filtro personalizado
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
	public bool Authorize(DashboardContext context)
	{
		var httpContext = context.GetHttpContext();

		// Solo usuarios autenticados con rol SuperAdmin o Admin
		return httpContext.User.Identity?.IsAuthenticated == true &&
			   (httpContext.User.IsInRole(AppRoles.SuperAdmin) || 
				httpContext.User.IsInRole(AppRoles.Admin));
	}
}
```

---

### **Configurar Worker para Jobs de Fondo**

```csharp
// Program.cs

// Configurar servidor de Hangfire con opciones de producción
builder.Services.AddHangfireServer(options =>
{
	options.WorkerCount = Environment.ProcessorCount * 2;  // Más workers en producción
	options.ServerName = Environment.MachineName;
	options.Queues = new[] { "default", "emails", "reports" };  // Colas por prioridad
});
```

---

### **Usar colas específicas para correos**

```csharp
// En CumpleanosJob.cs y ReporteMensualJob.cs

[Queue("emails")]  // Atributo para usar cola específica
[AutomaticRetry(Attempts = 3, DelaysInSeconds = new[] { 60, 300, 3600 })]  // Reintentos con delays progresivos
public async Task EnviarCorreosCumpleanosDelMesAsync()
{
	// ... código existente
}
```

---

## 📊 MONITOREO Y TROUBLESHOOTING

### **1. Configurar Application Insights (Azure)**

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]);

// En EmailService.cs - agregar telemetría
public class EmailService : IEmailService
{
	private readonly TelemetryClient _telemetry;

	public EmailService(IConfiguration configuration, ILogger<EmailService> logger, TelemetryClient telemetry)
	{
		_configuration = configuration;
		_logger = logger;
		_telemetry = telemetry;
	}

	public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
	{
		var stopwatch = Stopwatch.StartNew();

		try
		{
			await EnviarCorreoMultipleAsync(new List<string> { destinatario }, asunto, cuerpoHtml);

			// Registrar métrica de éxito
			_telemetry.TrackMetric("Email.Sent.Success", 1);
			_telemetry.TrackEvent("EmailSent", new Dictionary<string, string>
			{
				{ "Destinatario", destinatario },
				{ "Asunto", asunto },
				{ "DuracionMs", stopwatch.ElapsedMilliseconds.ToString() }
			});
		}
		catch (Exception ex)
		{
			_telemetry.TrackMetric("Email.Sent.Failure", 1);
			_telemetry.TrackException(ex);
			throw;
		}
	}
}
```

---

### **2. Logs estructurados con Serilog**

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.AzureAnalytics  # Para Azure
```

```csharp
// Program.cs
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configurar Serilog
Log.Logger = new LoggerConfiguration()
	.ReadFrom.Configuration(builder.Configuration)
	.Enrich.FromLogContext()
	.Enrich.WithProperty("Application", "DataTeam")
	.Enrich.WithProperty("Environment", builder.Environment.EnvironmentName)
	.WriteTo.Console()
	.WriteTo.File("logs/datateam-.txt", rollingInterval: RollingInterval.Day)
	.WriteTo.AzureAnalytics(
		workspaceId: builder.Configuration["AzureLogAnalytics:WorkspaceId"],
		authenticationId: builder.Configuration["AzureLogAnalytics:Key"]
	)
	.CreateLogger();

builder.Host.UseSerilog();
```

```json
// appsettings.Production.json
{
  "Serilog": {
	"MinimumLevel": {
	  "Default": "Information",
	  "Override": {
		"Microsoft": "Warning",
		"System": "Warning",
		"Hangfire": "Information"
	  }
	}
  }
}
```

---

### **3. Health Checks**

```bash
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
dotnet add package AspNetCore.HealthChecks.Hangfire
dotnet add package AspNetCore.HealthChecks.Smtp
```

```csharp
// Program.cs
builder.Services.AddHealthChecks()
	.AddDbContextCheck<ApplicationDbContext>()
	.AddHangfire(options => options.MinimumAvailableServers = 1)
	.AddSmtpHealthCheck(options =>
	{
		options.Host = builder.Configuration["Email:SmtpHost"];
		options.Port = int.Parse(builder.Configuration["Email:SmtpPort"] ?? "587");
		options.ConnectionType = SmtpConnectionType.TLS;
	});

// Exponer endpoint
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
	Predicate = check => check.Tags.Contains("ready")
});
```

---

### **4. Dashboard de monitoreo de correos**

Crear tabla de auditoría de correos:

```sql
CREATE TABLE EmailLog (
	Id INT IDENTITY(1,1) PRIMARY KEY,
	Destinatarios NVARCHAR(MAX),
	Asunto NVARCHAR(500),
	FechaEnvio DATETIME2,
	Estado NVARCHAR(50),  -- Enviado, Fallido, Pendiente
	MensajeError NVARCHAR(MAX),
	DuracionMs INT,
	JobId NVARCHAR(100)
);
```

```csharp
// Modificar EmailService para registrar en BD
public async Task EnviarCorreoMultipleAsync(List<string> destinatarios, string asunto, string cuerpoHtml)
{
	var log = new EmailLog
	{
		Destinatarios = string.Join(";", destinatarios),
		Asunto = asunto,
		FechaEnvio = DateTime.UtcNow,
		Estado = "Pendiente"
	};

	var stopwatch = Stopwatch.StartNew();

	try
	{
		// ... código de envío existente ...

		log.Estado = "Enviado";
		log.DuracionMs = (int)stopwatch.ElapsedMilliseconds;
	}
	catch (Exception ex)
	{
		log.Estado = "Fallido";
		log.MensajeError = ex.Message;
		_logger.LogError(ex, "Error al enviar correo");
		throw;
	}
	finally
	{
		await _context.EmailLogs.AddAsync(log);
		await _context.SaveChangesAsync();
	}
}
```

---

## ✅ CHECKLIST DE DESPLIEGUE

### **Pre-Despliegue:**
- [ ] Código compilado sin errores
- [ ] Migraciones EF Core generadas
- [ ] Pruebas locales exitosas
- [ ] Secrets removidos de appsettings.json
- [ ] README actualizado con instrucciones de producción

### **Infraestructura:**
- [ ] Servidor/VM provisionado
- [ ] SQL Server configurado
- [ ] Firewall/NSG configurado (puertos 443, 1433)
- [ ] Certificado SSL instalado
- [ ] Dominio DNS configurado

### **Servicios Externos:**
- [ ] Servicio SMTP contratado y configurado
- [ ] Cuenta de correo verificada
- [ ] Dominio de correo autenticado (SPF, DKIM, DMARC)
- [ ] Key Vault/Secrets Manager configurado

### **Aplicación:**
- [ ] Connection string a SQL Server configurado
- [ ] Variables de entorno configuradas
- [ ] Hangfire migrado a SQL Server
- [ ] Dashboard de Hangfire protegido
- [ ] Application Insights habilitado
- [ ] Health checks funcionando

### **Correos:**
- [ ] Credenciales SMTP en Key Vault
- [ ] Lista de correos de Talento Humano configurada
- [ ] Jobs de Hangfire registrados y programados
- [ ] Prueba de envío manual exitosa
- [ ] Logs de correos funcionando

### **Post-Despliegue:**
- [ ] Ejecutar migraciones en BD de producción
- [ ] Seed de datos iniciales (usuarios, roles, células)
- [ ] Ejecutar job de cumpleaños manualmente (prueba)
- [ ] Verificar logs en Application Insights
- [ ] Monitorear Dashboard de Hangfire por 24h
- [ ] Configurar alertas de errores

---

## 🔥 TROUBLESHOOTING COMÚN

### **Problema: Jobs de Hangfire no se ejecutan**

```bash
# Verificar en Hangfire Dashboard:
1. /hangfire/recurring  # ¿Jobs están registrados?
2. /hangfire/servers    # ¿Hay servidores activos?
3. /hangfire/failed     # ¿Hay jobs fallidos?

# Logs a revisar:
SELECT TOP 100 * FROM [Hangfire].[Job] ORDER BY CreatedAt DESC
SELECT TOP 100 * FROM [Hangfire].[State] ORDER BY CreatedAt DESC
```

**Soluciones:**
- Verificar que connection string esté correcto
- Reiniciar App Service / IIS
- Verificar que HangfireServer esté configurado en Program.cs

---

### **Problema: Correos no se envían**

```csharp
// Agregar más logging en EmailService
_logger.LogInformation($"Intentando enviar correo a {destinatario}");
_logger.LogInformation($"SMTP Host: {smtpHost}, Port: {smtpPort}");
_logger.LogInformation($"Usuario SMTP: {smtpUser?.Substring(0, 3)}***");

try
{
	await smtp.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls);
	_logger.LogInformation("Conexión SMTP exitosa");

	await smtp.AuthenticateAsync(smtpUser, smtpPassword);
	_logger.LogInformation("Autenticación SMTP exitosa");

	await smtp.SendAsync(mensaje);
	_logger.LogInformation("Correo enviado exitosamente");
}
catch (Exception ex)
{
	_logger.LogError(ex, "Error detallado en envío de correo");
	throw;
}
```

**Verificaciones:**
1. ✅ Credenciales correctas en Key Vault
2. ✅ Firewall permite salida por puerto 587/465
3. ✅ Servicio SMTP no bloqueó la cuenta
4. ✅ Destinatarios válidos (no correos de prueba)

---

### **Problema: Credenciales no se leen desde Key Vault**

```csharp
// En Program.cs, agregar logs de configuración
var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddAzureKeyVault(
	new Uri(builder.Configuration["KeyVaultUrl"]),
	new DefaultAzureCredential()
);

// Verificar que se cargó
var smtpUser = builder.Configuration["Email--SmtpUser"];
if (string.IsNullOrEmpty(smtpUser))
{
	throw new Exception("No se pudo leer Email--SmtpUser desde Key Vault");
}
```

**Soluciones:**
- Verificar Managed Identity habilitada
- Verificar permisos en Key Vault
- Usar `--` en lugar de `:` para nombres jerárquicos

---

## 📚 RECURSOS ADICIONALES

### **Documentación oficial:**
- [Azure App Service](https://learn.microsoft.com/azure/app-service/)
- [Azure Communication Services Email](https://learn.microsoft.com/azure/communication-services/quickstarts/email/send-email)
- [SendGrid para .NET](https://docs.sendgrid.com/for-developers/sending-email/quickstart-csharp)
- [Hangfire Documentation](https://docs.hangfire.io/)
- [MailKit Documentation](https://github.com/jstedfast/MailKit)

### **Herramientas útiles:**
- [Mail Tester](https://www.mail-tester.com/) - Verificar deliverability
- [MX Toolbox](https://mxtoolbox.com/) - Diagnóstico DNS/SMTP
- [Hangfire Dashboard](https://docs.hangfire.io/en/latest/configuration/using-dashboard.html)

---

## 🎯 RESUMEN RÁPIDO

### **Para Azure (Recomendado):**
```
1. Usar Azure Communication Services para correos
2. Credenciales en Key Vault con Managed Identity
3. SQL Azure para Hangfire y datos
4. Application Insights para logs
5. Deploy con GitHub Actions o Azure DevOps
```

### **Para Servidor IIS:**
```
1. Usar Office 365 o SendGrid para correos
2. Credenciales en variables de entorno del sistema
3. SQL Server local para Hangfire
4. Serilog con archivos rotados
5. Deploy manual o con scripts PowerShell
```

### **Configuración mínima requerida:**
```json
{
  "Email": {
	"SmtpHost": "desde-variable-entorno",
	"SmtpUser": "desde-keyvault",
	"SmtpPassword": "desde-keyvault",
	"TalentoHumano": ["rrhh@empresa.com"]
  },
  "ConnectionStrings": {
	"DefaultConnection": "desde-variable-entorno-o-keyvault"
  }
}
```

---

✅ **Siguiente paso:** Crear archivo `appsettings.Production.json` en tu proyecto

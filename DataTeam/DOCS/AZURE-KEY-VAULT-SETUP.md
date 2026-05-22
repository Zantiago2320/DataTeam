# 🔐 GUÍA DE INTEGRACIÓN CON AZURE KEY VAULT

## 📋 ¿QUÉ ES AZURE KEY VAULT?

Azure Key Vault es un servicio de Azure que permite almacenar y acceder de forma segura a:
- 🔑 **Secrets**: Contraseñas, cadenas de conexión, API keys
- 🔐 **Keys**: Claves criptográficas para cifrado
- 📜 **Certificates**: Certificados SSL/TLS

### **Ventajas sobre variables de entorno:**
- ✅ Auditoría completa (quién accedió, cuándo)
- ✅ Versionado de secrets (historial de cambios)
- ✅ Rotación automática de contraseñas
- ✅ Integración nativa con Azure App Service
- ✅ Managed Identity (sin necesidad de credenciales)
- ✅ Control de acceso granular por servicio

---

## 🚀 CONFIGURACIÓN PASO A PASO

### **FASE 1: Crear y Configurar Key Vault en Azure**

#### **Opción A: Desde Azure Portal (Visual)**

1. **Ir a Azure Portal** → https://portal.azure.com
2. Click en **"Create a resource"** → Buscar **"Key Vault"**
3. **Configurar:**
   ```
   Subscription: Tu suscripción
   Resource Group: datateam-rg (o crear nuevo)
   Key Vault Name: datateam-keyvault (debe ser único globalmente)
   Region: East US (misma que tu App Service)
   Pricing Tier: Standard
   ```
4. **Pestaña "Access configuration":**
   - Access policy model: **Vault access policy** (más simple)
   - Permission model: **Vault access policy**
5. Click **"Review + Create"** → **"Create"**

---

#### **Opción B: Desde Azure CLI (Automatizado)**

```bash
# 1. Login en Azure
az login

# 2. Variables
RESOURCE_GROUP="datateam-rg"
LOCATION="eastus"
KEYVAULT_NAME="datateam-keyvault"  # Debe ser único globalmente

# 3. Crear grupo de recursos (si no existe)
az group create --name $RESOURCE_GROUP --location $LOCATION

# 4. Crear Key Vault
az keyvault create \
  --name $KEYVAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enable-rbac-authorization false \
  --enabled-for-deployment true \
  --enabled-for-template-deployment true
```

---

### **FASE 2: Agregar Secrets al Key Vault**

#### **Opción A: Desde Azure Portal**

1. Ir a tu Key Vault → **"Secrets"** (menú izquierdo)
2. Click **"+ Generate/Import"**
3. **Agregar cada secret:**

```
Name: Email--SmtpUser
Value: correo-app@tuempresa.com
Content type: (vacío)
Enabled: Yes
```

```
Name: Email--SmtpPassword
Value: tu-password-seguro-aqui
Content type: password
Enabled: Yes
```

```
Name: ConnectionStrings--DefaultConnection
Value: Server=tcp:datateam-sql.database.windows.net,1433;Initial Catalog=DataTeamDB;User ID=sqladmin;Password=Password123!;...
Content type: connection-string
Enabled: Yes
```

**⚠️ IMPORTANTE:**
- Usar **`--`** (doble guion) en lugar de `:` para niveles jerárquicos
- Ejemplo: `Email--SmtpUser` se mapea a `Email:SmtpUser` en appsettings.json

---

#### **Opción B: Desde Azure CLI**

```bash
# Variables
KEYVAULT_NAME="datateam-keyvault"

# Agregar secrets
az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "Email--SmtpUser" \
  --value "correo-app@tuempresa.com"

az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "Email--SmtpPassword" \
  --value "password-seguro-123"

az keyvault secret set \
  --vault-name $KEYVAULT_NAME \
  --name "ConnectionStrings--DefaultConnection" \
  --value "Server=tcp:datateam-sql.database.windows.net,1433;Initial Catalog=DataTeamDB;User ID=sqladmin;Password=Password123!;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

---

#### **Opción C: Desde PowerShell**

```powershell
# Instalar módulo (si no lo tienes)
Install-Module -Name Az -AllowClobber -Scope CurrentUser

# Login
Connect-AzAccount

# Agregar secrets
$vaultName = "datateam-keyvault"

Set-AzKeyVaultSecret -VaultName $vaultName -Name "Email--SmtpUser" -SecretValue (ConvertTo-SecureString "correo@empresa.com" -AsPlainText -Force)

Set-AzKeyVaultSecret -VaultName $vaultName -Name "Email--SmtpPassword" -SecretValue (ConvertTo-SecureString "password-seguro" -AsPlainText -Force)
```

---

### **FASE 3: Configurar Managed Identity en Azure App Service**

#### **¿Qué es Managed Identity?**
Es una identidad automática que Azure crea para tu aplicación, permitiéndole acceder a recursos (como Key Vault) **sin necesidad de guardar credenciales**.

---

#### **Opción A: Desde Azure Portal**

1. Ir a tu **App Service** (datateam-app)
2. Menú izquierdo → **"Identity"**
3. Pestaña **"System assigned"**
4. **Status**: Cambiar a **On**
5. Click **"Save"**
6. Azure generará un **Object (principal) ID** → **Copiarlo** (lo necesitarás en el siguiente paso)

---

#### **Opción B: Desde Azure CLI**

```bash
APP_NAME="datateam-app"
RESOURCE_GROUP="datateam-rg"

# Habilitar managed identity
az webapp identity assign \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP

# Obtener el Object ID (guardar este valor)
OBJECT_ID=$(az webapp identity show \
  --name $APP_NAME \
  --resource-group $RESOURCE_GROUP \
  --query principalId \
  --output tsv)

echo "Object ID: $OBJECT_ID"
```

---

### **FASE 4: Dar Permisos al App Service para Acceder al Key Vault**

#### **Opción A: Desde Azure Portal**

1. Ir a tu **Key Vault** (datateam-keyvault)
2. Menú izquierdo → **"Access policies"**
3. Click **"+ Add Access Policy"**
4. **Configurar:**
   ```
   Secret permissions: Get, List
   Key permissions: (ninguno)
   Certificate permissions: (ninguno)
   Select principal: Buscar el nombre de tu App Service (datateam-app)
   ```
5. Click **"Add"** → **"Save"**

---

#### **Opción B: Desde Azure CLI**

```bash
KEYVAULT_NAME="datateam-keyvault"
OBJECT_ID="<object-id-del-paso-anterior>"

# Dar permisos de lectura de secrets
az keyvault set-policy \
  --name $KEYVAULT_NAME \
  --object-id $OBJECT_ID \
  --secret-permissions get list
```

---

### **FASE 5: Integrar Key Vault en tu Aplicación ASP.NET Core**

#### **Paso 1: Instalar paquetes NuGet**

```bash
cd DataTeam
dotnet add package Azure.Extensions.AspNetCore.Configuration.Secrets
dotnet add package Azure.Identity
```

---

#### **Paso 2: Modificar Program.cs**

```csharp
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// ✅ AGREGAR ESTA CONFIGURACIÓN ANTES DE builder.Services
if (builder.Environment.IsProduction())
{
	// Obtener URL del Key Vault desde configuración
	var keyVaultUrl = builder.Configuration["KeyVaultUrl"];

	if (!string.IsNullOrEmpty(keyVaultUrl))
	{
		// DefaultAzureCredential usa automáticamente la Managed Identity en Azure
		builder.Configuration.AddAzureKeyVault(
			new Uri(keyVaultUrl),
			new DefaultAzureCredential()
		);

		builder.Logging.AddConsole();
		builder.Logging.LogInformation($"Azure Key Vault configurado: {keyVaultUrl}");
	}
}

// ... resto de tu código existente (builder.Services.AddDbContext, etc.)
```

---

#### **Paso 3: Configurar KeyVaultUrl en App Service**

**Desde Azure Portal:**
1. Ir a tu App Service → **"Configuration"**
2. **Application settings** → Click **"+ New application setting"**
3. **Name**: `KeyVaultUrl`
4. **Value**: `https://datateam-keyvault.vault.azure.net/`
5. Click **"OK"** → **"Save"**

**Desde Azure CLI:**
```bash
az webapp config appsettings set \
  --name datateam-app \
  --resource-group datateam-rg \
  --settings KeyVaultUrl="https://datateam-keyvault.vault.azure.net/"
```

---

#### **Paso 4: Actualizar appsettings.Production.json**

```json
{
  "KeyVaultUrl": "https://datateam-keyvault.vault.azure.net/",
  "ConnectionStrings": {
	"DefaultConnection": "WILL-BE-REPLACED-BY-KEYVAULT"
  },
  "Email": {
	"SmtpUser": "WILL-BE-REPLACED-BY-KEYVAULT",
	"SmtpPassword": "WILL-BE-REPLACED-BY-KEYVAULT"
  }
}
```

---

### **FASE 6: Usar los Secrets en tu Código**

#### **✅ Acceso directo en servicios:**

```csharp
public class EmailService : IEmailService
{
	private readonly IConfiguration _configuration;

	public EmailService(IConfiguration configuration)
	{
		_configuration = configuration;
	}

	public async Task EnviarCorreoAsync(string destinatario, string asunto, string cuerpoHtml)
	{
		// Estos valores vienen AUTOMÁTICAMENTE desde Key Vault en producción
		var smtpHost = _configuration["Email:SmtpHost"];
		var smtpUser = _configuration["Email:SmtpUser"];  // 🔐 Desde Key Vault: Email--SmtpUser
		var smtpPassword = _configuration["Email:SmtpPassword"];  // 🔐 Desde Key Vault: Email--SmtpPassword

		// ... resto del código
	}
}
```

#### **✅ Connection strings:**

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
	// GetConnectionString busca primero en Key Vault (si está configurado)
	var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
	options.UseSqlServer(connectionString);
});
```

---

## 🧪 TESTING Y VERIFICACIÓN

### **1. Probar localmente con User Secrets (Desarrollo)**

```bash
# En tu máquina de desarrollo
dotnet user-secrets init
dotnet user-secrets set "KeyVaultUrl" "https://datateam-keyvault.vault.azure.net/"

# Para testing local, necesitas autenticarte con tu cuenta Azure
az login

# Ahora puedes ejecutar la app localmente y leerá desde Key Vault
dotnet run
```

---

### **2. Verificar que la app lee correctamente los secrets**

Agregar logs temporales en `Program.cs`:

```csharp
if (builder.Environment.IsProduction())
{
	var keyVaultUrl = builder.Configuration["KeyVaultUrl"];
	builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrl), new DefaultAzureCredential());

	// ✅ AGREGAR ESTE LOG TEMPORAL
	var smtpUser = builder.Configuration["Email:SmtpUser"];
	var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

	builder.Logging.AddConsole();
	Console.WriteLine($"🔐 Key Vault URL: {keyVaultUrl}");
	Console.WriteLine($"📧 SMTP User configurado: {smtpUser?.Substring(0, 3)}***");
	Console.WriteLine($"🗄️ Connection String configurado: {(connectionString != null ? "✅ Sí" : "❌ No")}");
}
```

**⚠️ Remover estos logs después de verificar en producción.**

---

### **3. Verificar acceso desde Kudu Console**

1. Ir a: `https://datateam-app.scm.azurewebsites.net`
2. **Debug console** → **PowerShell**
3. Ejecutar:
   ```powershell
   # Ver variables de entorno
   Get-ChildItem Env:

   # Verificar que KeyVaultUrl está configurada
   $env:KeyVaultUrl
   ```

---

## 🔄 ROTACIÓN DE SECRETS (Cambiar contraseñas)

### **Escenario: Necesitas cambiar la contraseña del correo**

#### **Paso 1: Crear nueva versión del secret**

```bash
# Azure CLI
az keyvault secret set \
  --vault-name datateam-keyvault \
  --name "Email--SmtpPassword" \
  --value "nueva-password-segura-123"
```

O desde Azure Portal:
1. Key Vault → Secrets → `Email--SmtpPassword`
2. Click **"+ New Version"**
3. Ingresar nueva contraseña → **"Create"**

#### **Paso 2: Reiniciar App Service**

```bash
az webapp restart --name datateam-app --resource-group datateam-rg
```

O desde Portal:
1. App Service → **Overview** → Click **"Restart"**

**⚠️ La aplicación leerá automáticamente la versión más reciente del secret.**

---

## 🛡️ SEGURIDAD Y MEJORES PRÁCTICAS

### ✅ **DO (Hacer):**

1. **Usar Managed Identity siempre que sea posible**
   - Evita almacenar credenciales de acceso al Key Vault

2. **Principio de menor privilegio**
   - Solo dar permisos `Get` y `List` para secrets
   - No dar permisos de escritura a la aplicación

3. **Habilitar auditoría**
   ```bash
   # Habilitar logs de acceso
   az monitor diagnostic-settings create \
	 --resource $(az keyvault show --name datateam-keyvault --query id -o tsv) \
	 --name "KeyVault-Diagnostics" \
	 --logs '[{"category": "AuditEvent","enabled": true}]' \
	 --workspace <log-analytics-workspace-id>
   ```

4. **Usar soft-delete y purge protection**
   ```bash
   az keyvault update \
	 --name datateam-keyvault \
	 --enable-soft-delete true \
	 --enable-purge-protection true
   ```

5. **Documentar todos los secrets**
   - Mantener un inventario de qué secrets existen y para qué sirven

---

### ❌ **DON'T (No hacer):**

1. **❌ NO poner secrets en appsettings.json en producción**
   ```json
   // ❌ MALO
   "Email": {
	 "SmtpPassword": "password-en-texto-plano"
   }
   ```

2. **❌ NO compartir el mismo Key Vault entre ambientes**
   - Usar Key Vaults separados: `datateam-kv-dev`, `datateam-kv-prod`

3. **❌ NO dar permisos de `Set` o `Delete` a la aplicación**
   - Solo lectura (`Get`, `List`)

4. **❌ NO loguear valores de secrets**
   ```csharp
   // ❌ PELIGROSO
   _logger.LogInformation($"Password: {smtpPassword}");

   // ✅ CORRECTO
   _logger.LogInformation($"Password configurado: {!string.IsNullOrEmpty(smtpPassword)}");
   ```

---

## 🚨 TROUBLESHOOTING

### **Error: "Access denied to Key Vault"**

**Síntomas:**
```
Azure.RequestFailedException: Access denied. Caller was not found on any access policy.
```

**Solución:**
1. Verificar que Managed Identity esté habilitada en App Service
2. Verificar que el Object ID tenga permisos en Key Vault
3. Esperar 5 minutos (propagación de permisos)
4. Reiniciar App Service

```bash
# Verificar permisos actuales
az keyvault show --name datateam-keyvault --query properties.accessPolicies
```

---

### **Error: "DefaultAzureCredential failed to retrieve a token"**

**Síntomas:**
```
Azure.Identity.CredentialUnavailableException: DefaultAzureCredential failed to retrieve a token
```

**Solución:**

En **desarrollo local**:
```bash
# Autenticarse con Azure CLI
az login

# O configurar variables de entorno
$env:AZURE_TENANT_ID="tu-tenant-id"
$env:AZURE_CLIENT_ID="tu-client-id"
$env:AZURE_CLIENT_SECRET="tu-secret"
```

En **Azure App Service** (debería funcionar automáticamente):
```bash
# Verificar que Managed Identity esté activa
az webapp identity show --name datateam-app --resource-group datateam-rg
```

---

### **Error: Secret no se actualiza después de cambiar valor**

**Causa:** La configuración se carga al inicio de la aplicación.

**Solución:**
```bash
# Reiniciar App Service para recargar configuración
az webapp restart --name datateam-app --resource-group datateam-rg
```

**Alternativa:** Implementar recarga periódica (avanzado):
```csharp
builder.Configuration.AddAzureKeyVault(
	new Uri(keyVaultUrl),
	new DefaultAzureCredential(),
	new AzureKeyVaultConfigurationOptions
	{
		ReloadInterval = TimeSpan.FromMinutes(30)  // Recargar cada 30 minutos
	}
);
```

---

## 📊 COMPARATIVA: KEY VAULT vs VARIABLES DE ENTORNO

| Característica | Azure Key Vault | Variables de Entorno | appsettings.json |
|----------------|-----------------|----------------------|------------------|
| **Seguridad** | ⭐⭐⭐⭐⭐ Máxima | ⭐⭐⭐ Media | ⭐ Baja |
| **Auditoría** | ✅ Completa | ❌ No | ❌ No |
| **Versionado** | ✅ Historial | ❌ No | ⚠️ Con Git |
| **Rotación** | ✅ Automática | ⚠️ Manual | ⚠️ Manual |
| **Costo** | 💰 ~$0.03/10K ops | Gratis | Gratis |
| **Complejidad** | ⭐⭐⭐ Alta | ⭐⭐ Media | ⭐ Baja |
| **Recomendado para** | Producción | Dev/Test | Solo desarrollo |

---

## 📚 RECURSOS ADICIONALES

### **Documentación oficial:**
- [Azure Key Vault Quickstart](https://learn.microsoft.com/azure/key-vault/general/quick-create-portal)
- [Managed Identity Documentation](https://learn.microsoft.com/azure/active-directory/managed-identities-azure-resources/overview)
- [ASP.NET Core Key Vault Configuration](https://learn.microsoft.com/aspnet/core/security/key-vault-configuration)

### **Herramientas:**
- [Azure Key Vault Explorer](https://github.com/microsoft/AzureKeyVaultExplorer) (GUI para gestión)
- [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli)
- [Azure PowerShell](https://learn.microsoft.com/powershell/azure/install-az-ps)

---

## ✅ CHECKLIST DE IMPLEMENTACIÓN

### **Setup inicial:**
- [ ] Key Vault creado en Azure
- [ ] Secrets agregados (SmtpUser, SmtpPassword, ConnectionString)
- [ ] Managed Identity habilitada en App Service
- [ ] Permisos configurados en Key Vault
- [ ] `KeyVaultUrl` configurada en App Service

### **Código:**
- [ ] Paquetes NuGet instalados (`Azure.Extensions.AspNetCore.Configuration.Secrets`, `Azure.Identity`)
- [ ] `Program.cs` actualizado con `AddAzureKeyVault`
- [ ] `appsettings.Production.json` con `KeyVaultUrl`
- [ ] Logs temporales para verificar lectura (remover después)

### **Testing:**
- [ ] Prueba local con `az login`
- [ ] Deploy a Azure y verificar logs
- [ ] Envío de correo de prueba exitoso
- [ ] Verificar auditoría en Key Vault

### **Seguridad:**
- [ ] Soft-delete habilitado
- [ ] Purge protection habilitado
- [ ] Auditoría configurada
- [ ] Secrets removidos de código fuente

---

## 🎯 PRÓXIMOS PASOS

1. ✅ **Implementar Key Vault** (sigue esta guía)
2. 📧 **Modificar jobs de correo** para usar lista de Talento Humano desde configuración
3. 🗄️ **Migrar Hangfire a SQL Server**
4. 📊 **Configurar Application Insights**
5. 🚀 **Deploy a producción**

---

✅ **Con esta configuración, tu aplicación estará lista para producción con seguridad enterprise-grade.**

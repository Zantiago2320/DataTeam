# DateTeam - Sistema de Gestión de Consultores

Sistema de directorio de personal (consultores) desarrollado en ASP.NET Core MVC con .NET 10.

## 📋 Características Implementadas

### Modelos de Datos
- ✅ **Consultor**: Entidad principal con todos los campos requeridos (cédula, nombre, correo, cargo, foto, fechas, célula, rol, capacidad, etc.)
- ✅ **Célula**: Equipos/células de trabajo con consultores asignados
- ✅ **AuditoriaLog**: Registro de todos los cambios realizados en el sistema

### Servicios Implementados
- ✅ **AuditoriaService**: Registro automático de cambios con información de usuario, valores anteriores/nuevos, IP
- ✅ **FileService**: Manejo de subida y eliminación de fotos de perfil con validación
- ✅ **ExcelService**: Exportación de datos a Excel usando ClosedXML
- ✅ **EmailService**: Envío de correos usando MailKit

### Trabajos en Segundo Plano (Hangfire)
- ✅ **CumpleanosJob**: Envío automático de correos el primer día hábil del mes con cumpleaños del mes
- ✅ **ReporteMensualJob**: Envío de reporte estadístico el día 15 de cada mes

### Configuración
- ✅ Base de datos SQL Server configurada (DateTeamDB)
- ✅ Identity para autenticación
- ✅ Hangfire Dashboard en `/hangfire`
- ✅ Estructura de carpetas creada

## 🗂️ Estructura del Proyecto

```
DataTeam/
├── Controllers/          (Pendiente: CRUD de consultores y células)
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
├── Models/
│   ├── Consultor.cs
│   ├── Celula.cs
│   └── AuditoriaLog.cs
├── Services/
│   ├── AuditoriaService.cs
│   ├── FileService.cs
│   ├── ExcelService.cs
│   ├── EmailService.cs
│   └── BackgroundJobs/
│       ├── CumpleanosJob.cs
│       └── ReporteMensualJob.cs
├── ViewModels/
│   ├── ConsultorViewModel.cs
│   └── OrganigramaViewModel.cs
├── Views/               (Pendiente: Vistas CRUD y organigrama)
└── wwwroot/
    ├── images/
    └── uploads/fotos/
```

## 🔧 Configuración Inicial

### 1. Configurar Base de Datos

Asegúrate de tener SQL Server instalado y accesible. La cadena de conexión está en `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=DateTeamDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
```

### 2. Ejecutar Migraciones

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### 3. Configurar Email (appsettings.json)

Actualiza las credenciales SMTP para el envío de correos:

```json
"Email": {
  "FromName": "DateTeam",
  "FromAddress": "tu-correo@dominio.com",
  "SmtpHost": "smtp.gmail.com",
  "SmtpPort": "587",
  "SmtpUser": "tu-correo@gmail.com",
  "SmtpPassword": "tu-contraseña-de-app"
}
```

**Nota para Gmail**: Debes usar una "Contraseña de aplicación" en lugar de tu contraseña normal.

## 📦 Paquetes NuGet Instalados

- Microsoft.EntityFrameworkCore.SqlServer (10.0.7)
- Microsoft.EntityFrameworkCore.Tools (10.0.7)
- Microsoft.EntityFrameworkCore.Design (10.0.7)
- ClosedXML (0.104.2)
- MailKit (4.9.0)
- Hangfire.AspNetCore (1.8.20)
- Hangfire.SqlServer (1.8.20)

## 🚀 Próximos Pasos

### Controllers y Vistas (Pendiente)
1. **ConsultoresController**: CRUD completo con manejo de fotos y auditoría
2. **CelulasController**: Gestión de células/equipos
3. **OrganigramaController**: Vista de organigrama visual
4. **AuditoriaController**: Visualización de logs

### Vistas (Pendiente)
1. Index de consultores con búsqueda y filtros
2. Create/Edit con subida de foto
3. Details con perfil completo del consultor
4. Organigrama visual con tarjetas por célula
5. Dashboard con estadísticas

### Funcionalidades Adicionales
- Validación de cédula única
- Generación automática de avatar si no hay foto
- Búsqueda avanzada por múltiples criterios
- Paginación en listados
- Filtros por célula, estado, etc.

## 🎯 Características del Sistema

### Estados del Consultor
- **Activo**: Consultor trabajando actualmente
- **Retirado**: Consultor que ya no está en la empresa

### Trabajos Programados
- **Cumpleaños**: Se ejecuta el 1ro al 7mo día del mes (verifica primer día hábil) a las 8:00 AM
- **Reporte Mensual**: Se ejecuta el día 15 de cada mes a las 9:00 AM

### Auditoría
Todos los cambios en consultores se registran automáticamente con:
- Usuario que realizó el cambio
- Valores anteriores y nuevos
- Fecha y hora
- Dirección IP

## 📊 Dashboard de Hangfire

Accede al dashboard en: `https://localhost:xxxx/hangfire`

Aquí podrás:
- Ver trabajos programados
- Ejecutar trabajos manualmente
- Ver historial de ejecuciones
- Monitorear errores

## 🔒 Seguridad

- Autenticación mediante ASP.NET Core Identity
- Validación de archivos subidos (tipo y tamaño)
- Índices únicos en cédula y correo
- Protección contra inyección SQL mediante Entity Framework

## 📝 Notas

- Las fotos se guardan en `wwwroot/uploads/fotos/`
- Tamaño máximo de foto: 5 MB
- Formatos permitidos: jpg, jpeg, png, gif
- La base de datos incluye una célula "Sin Asignar" por defecto

---

**Desarrollado con ASP.NET Core MVC + .NET 10**

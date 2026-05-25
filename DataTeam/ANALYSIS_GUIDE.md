# Guía de Análisis: Fortify y SonarQube - DataTeam

## 🎯 Objetivo
Alcanzar puntuación ≥ 70/100 en análisis de seguridad (Fortify) y calidad de código (SonarQube).

---

## 📋 Preparación Completada

### ✅ Cambios Implementados

#### Seguridad (Fortify)
1. ✅ Rate limiting (100 req/min)
2. ✅ Security headers (CSP, X-Frame-Options, X-XSS-Protection, etc.)
3. ✅ Contraseñas robustas (mín 8 caracteres, complejidad requerida)
4. ✅ Account lockout (5 intentos, 15 min bloqueo)
5. ✅ Cookies seguras (HttpOnly, Secure, SameSite=Strict)
6. ✅ Paquetes actualizados (MailKit/MimeKit 4.10.1)
7. ✅ Security analyzers instalados

#### Calidad (SonarQube)
1. ✅ .editorconfig con reglas de estilo
2. ✅ Roslyn analyzers (NetAnalyzers, SecurityCodeScan)
3. ✅ sonar-project.properties configurado
4. ✅ Exclusiones apropiadas (wwwroot, bin, obj, Migrations)
5. ✅ Compilación sin errores

---

## 🔍 Paso 1: Análisis con Fortify

### Prerrequisitos
- Micro Focus Fortify SCA instalado
- Licencia válida de Fortify

### Comandos

```bash
# Navegar al directorio del proyecto
cd C:\Users\USER\OneDrive\Desktop\proyectos\DataTeam\DataTeam

# Limpiar análisis previo
sourceanalyzer -b DataTeam -clean

# Traducir el código fuente
sourceanalyzer -b DataTeam dotnet build

# Ejecutar el análisis
sourceanalyzer -b DataTeam -scan -f results\DataTeam-Fortify.fpr

# Ver resultados (GUI)
auditworkbench results\DataTeam-Fortify.fpr
```

### Métricas Esperadas

| Categoría | Meta | Estimado |
|-----------|------|----------|
| **Vulnerabilidades Críticas** | 0 | 0 |
| **Vulnerabilidades Altas** | < 5 | 0-2 |
| **Vulnerabilidades Medias** | < 20 | 5-10 |
| **Vulnerabilidades Bajas** | < 50 | 10-20 |
| **Puntuación General** | ≥ 70/100 | 75-80/100 |

### Issues Comunes Esperados
✅ **SQL Injection**: Ninguno (uso de EF Core parametrizado)
✅ **XSS**: Ninguno (Razor auto-encoding)
✅ **CSRF**: Protegido (anti-forgery tokens)
⚠️ **Information Disclosure**: Posibles warnings en logs
⚠️ **Weak Cryptography**: Revisar si usa SHA1/MD5

---

## 🔍 Paso 2: Análisis con SonarQube

### Opción A: SonarQube Local

#### Instalación
```bash
# Descargar SonarQube Community
# https://www.sonarqube.org/downloads/

# Instalar Java JDK 11 o superior (requerido)
# https://adoptium.net/

# Iniciar SonarQube
cd sonarqube-x.x.x\bin\windows-x86-64
StartSonar.bat

# Acceder a http://localhost:9000
# Login: admin / admin (cambiar contraseña)
```

#### Configurar Proyecto
1. Crear nuevo proyecto en SonarQube UI
2. Generar token de análisis
3. Copiar comando de análisis

#### Ejecutar Análisis
```bash
# Instalar SonarScanner para .NET
dotnet tool install --global dotnet-sonarscanner

# Navegar al directorio de la solución
cd C:\Users\USER\OneDrive\Desktop\proyectos\DataTeam

# Iniciar análisis
dotnet sonarscanner begin /k:"datateam" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="TU_TOKEN_AQUI"

# Compilar
dotnet build DataTeam\DataTeam.csproj

# Finalizar análisis
dotnet sonarscanner end /d:sonar.login="TU_TOKEN_AQUI"

# Ver resultados en http://localhost:9000/dashboard?id=datateam
```

### Opción B: SonarCloud (Online)

```bash
# Registrarse en https://sonarcloud.io (gratis para open source)
# Conectar con GitHub

# Ejecutar análisis
dotnet sonarscanner begin \
  /k:"Zantiago2320_DataTeam" \
  /o:"tu-organizacion" \
  /d:sonar.host.url="https://sonarcloud.io" \
  /d:sonar.login="TU_TOKEN_AQUI"

dotnet build DataTeam\DataTeam.csproj

dotnet sonarscanner end /d:sonar.login="TU_TOKEN_AQUI"
```

### Métricas Esperadas

| Métrica | Meta | Estimado |
|---------|------|----------|
| **Bugs** | < 5 | 0-3 |
| **Vulnerabilidades** | 0 | 0-1 |
| **Code Smells** | < 50 | 20-40 |
| **Duplicación** | < 3% | 2-3% |
| **Cobertura** | > 50% | N/A (sin tests) |
| **Deuda Técnica** | < 1 día | < 1 día |
| **Puntuación General** | ≥ 70/100 | 70-75/100 |

### Categorías de Issues

#### Bugs (Estimado: 0-3)
- Posibles null reference exceptions
- Operaciones asíncronas incorrectas

#### Vulnerabilidades (Estimado: 0-1)
- Potencial deserialización insegura
- Cookies sin flags de seguridad (ya corregido)

#### Code Smells (Estimado: 20-40)
- Métodos con complejidad ciclomática > 10
- Código duplicado en controllers
- Falta de documentación XML
- Warnings de nullabilidad

#### Security Hotspots
- Revisión de configuración de cookies
- Validación de entradas de usuario
- Manejo de excepciones

---

## 📊 Paso 3: Interpretar Resultados

### Fortify

#### Severidad Critical/High (Prioridad 1)
✅ **Action**: Corregir inmediatamente
- SQL Injection: Verificar todas las consultas
- XSS: Revisar uso de @Html.Raw
- Hardcoded Secrets: Mover a User Secrets/Key Vault

#### Severidad Medium (Prioridad 2)
⚠️ **Action**: Revisar y corregir si aplica
- Information Disclosure: Ocultar stack traces en producción
- Insecure Cookies: Verificar flags HttpOnly/Secure
- Weak Cryptography: Usar algoritmos modernos

#### Severidad Low (Prioridad 3)
ℹ️ **Action**: Documentar o aceptar riesgo
- Missing Security Headers: Ya implementados
- Password Policy: Ya fortalecida

### SonarQube

#### Quality Gate: Passed / Failed
✅ **Passed**: Cumple umbrales mínimos
❌ **Failed**: Corregir issues bloqueantes

#### Prioridad por Tipo
1. **Bugs**: Corregir todos
2. **Vulnerabilidades**: Corregir todas
3. **Code Smells - Blocker/Critical**: Refactorizar
4. **Code Smells - Major/Minor**: Evaluar caso por caso
5. **Security Hotspots**: Revisar y aceptar/corregir

---

## 🛠️ Paso 4: Correcciones Iterativas

### Si Puntuación < 70

#### Fortify: Acciones Correctivas
```csharp
// Ejemplo: Corregir información sensible en logs
// ❌ Antes
_logger.LogError($"Login failed for {email} with password {password}");

// ✅ Después
_logger.LogError($"Login failed for user");
```

#### SonarQube: Acciones Correctivas
```csharp
// Ejemplo: Reducir complejidad ciclomática
// ❌ Antes (complejidad = 20)
public IActionResult ProcessData(int type) {
    if (type == 1) { /* ... */ }
    else if (type == 2) { /* ... */ }
    // ... 10 más if/else
}

// ✅ Después (complejidad = 5)
public IActionResult ProcessData(int type) {
    var processor = _processorFactory.Get(type);
    return processor.Process();
}
```

### Ejecutar Segundo Análisis
```bash
# Repetir Fortify scan
sourceanalyzer -b DataTeam -clean
sourceanalyzer -b DataTeam dotnet build
sourceanalyzer -b DataTeam -scan -f results\DataTeam-Fortify-v2.fpr

# Repetir SonarQube scan
dotnet sonarscanner begin /k:"datateam" /d:sonar.host.url="http://localhost:9000" /d:sonar.login="TOKEN"
dotnet build
dotnet sonarscanner end /d:sonar.login="TOKEN"
```

---

## ✅ Paso 5: Documentar Resultados

### Crear Reporte
```markdown
# Análisis de Seguridad y Calidad - DataTeam v1.1.0

## Fortify Scan
- **Fecha**: YYYY-MM-DD
- **Puntuación**: XX/100
- **Vulnerabilidades Críticas**: 0
- **Vulnerabilidades Altas**: X
- **Vulnerabilidades Medias**: X

## SonarQube Scan
- **Fecha**: YYYY-MM-DD
- **Puntuación**: XX/100
- **Quality Gate**: Passed/Failed
- **Bugs**: X
- **Vulnerabilidades**: X
- **Code Smells**: X
- **Duplicación**: X%

## Acciones Tomadas
1. [Lista de correcciones aplicadas]

## Riesgos Aceptados
1. [Issues que no se corregirán con justificación]
```

---

## 📚 Referencias

### Fortify
- [Fortify SCA Documentation](https://www.microfocus.com/documentation/fortify-static-code-analyzer-and-tools/)
- [OWASP Top 10](https://owasp.org/www-project-top-ten/)
- [CWE/SANS Top 25](https://cwe.mitre.org/top25/)

### SonarQube
- [SonarQube C# Rules](https://rules.sonarsource.com/csharp/)
- [.NET Code Analysis](https://docs.microsoft.com/en-us/dotnet/fundamentals/code-analysis/)
- [Clean Code](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)

---

## 📞 Soporte

Si encuentras problemas durante el análisis:
1. Revisa logs de Fortify/SonarQube
2. Consulta documentación oficial
3. Verifica que las herramientas estén correctamente instaladas
4. Asegúrate de tener las versiones compatibles de Java/.NET

---

**Estado Actual**: ✅ Listo para análisis  
**Próximo Paso**: Ejecutar Fortify y SonarQube scans  
**Meta**: Puntuación ≥ 70/100 en ambos  

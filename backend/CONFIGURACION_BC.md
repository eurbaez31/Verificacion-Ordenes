# Guía de Configuración: Business Central

Esta guía te ayudará a configurar la conexión entre el backend y Business Central mediante Azure AD.

## 📋 Requisitos Previos

- Acceso a [Azure Portal](https://portal.azure.com) con permisos de administrador
- Acceso a Business Central (Online o On-Premises con API habilitada)
- Conocer tu Tenant ID de Azure AD (puedes verlo en Azure Portal → Azure Active Directory → Overview)

---

## 🔐 Paso 1: Registrar Aplicación en Azure AD

### 1.1. Crear Nueva Registración

1. Ve a [Azure Portal](https://portal.azure.com)
2. Busca y selecciona **Azure Active Directory**
3. En el menú lateral, haz clic en **App registrations**
4. Haz clic en **+ New registration**

### 1.2. Configurar la Aplicación

Completa el formulario con:

- **Name**: `Order Verification API` (o el nombre que prefieras)
- **Supported account types**: 
  - Selecciona **Accounts in this organizational directory only** (Single tenant)
- **Redirect URI**: 
  - **Platform**: Web
  - **URI**: `http://localhost:9000` (solo para desarrollo, no se usa realmente)

5. Haz clic en **Register**

### 1.3. Guardar el Client ID y Tenant ID

Después de crear la aplicación, verás la página **Overview**:

- **Application (client) ID**: Copia este valor → será tu `ClientId`
- **Directory (tenant) ID**: Copia este valor → será tu `TenantId`

> 💡 **Nota**: Guarda estos valores, los necesitarás más adelante.

---

## 🔑 Paso 2: Crear Client Secret

### 2.1. Generar el Secret

1. En la página de tu aplicación, ve a **Certificates & secrets** (en el menú lateral)
2. Haz clic en **+ New client secret**
3. Completa:
   - **Description**: `Order Verification API Secret`
   - **Expires**: Selecciona la duración (recomendado: 24 meses para desarrollo, 12 meses para producción)
4. Haz clic en **Add**

### 2.2. Copiar el Secret

⚠️ **IMPORTANTE**: El valor del secret solo se muestra una vez. Cópialo inmediatamente.

- **Value**: Copia este valor → será tu `ClientSecret`

> 💡 **Nota**: Si pierdes este valor, tendrás que crear un nuevo secret.

---

## 🔐 Paso 3: Configurar Permisos de API

### 3.1. Agregar Permiso de Business Central

1. En la página de tu aplicación, ve a **API permissions**
2. Haz clic en **+ Add a permission**
3. Selecciona **APIs my organization uses**
4. Busca y selecciona **Dynamics 365 Business Central**
5. Selecciona **Application permissions** (no Delegated)
6. Expande y marca:
   - ✅ **API.ReadWrite.All** (o al menos **API.Read.All** si solo necesitas lectura)
7. Haz clic en **Add permissions**

### 3.2. Otorgar Consentimiento del Administrador

1. Verás que el permiso aparece con un ⚠️ indicando que requiere consentimiento
2. Haz clic en **Grant admin consent for [tu organización]**
3. Confirma haciendo clic en **Yes**

> ✅ Deberías ver una marca de verificación verde indicando que el consentimiento fue otorgado.

---

## 🏢 Paso 4: Obtener Company ID de Business Central

El Company ID es el identificador único de tu compañía en Business Central. Hay dos formas de obtenerlo:

### Método 1: Desde Business Central (Recomendado)

1. Inicia sesión en Business Central
2. Ve a **Configuración** → **Empresa** → **Información de la empresa**
3. Busca el campo **ID** o **Company ID** (puede estar en formato GUID)

### Método 2: Desde la API de Business Central

Si tienes acceso temporal a la API, puedes obtenerlo con:

```bash
# Reemplaza {tenantId} y {environment} con tus valores
curl -X GET "https://api.businesscentral.dynamics.com/v2.0/{tenantId}/{environment}/api/v2.0/companies" \
  -H "Authorization: Bearer {tu-token-de-acceso}"
```

La respuesta será un JSON con todas las compañías y sus IDs:

```json
{
  "value": [
    {
      "id": "12345678-1234-1234-1234-123456789012",
      "name": "CRONUS International Ltd.",
      ...
    }
  ]
}
```

### Método 3: Usar PowerShell (Si tienes acceso)

```powershell
# Instalar módulo si no lo tienes
Install-Module -Name AzureAD

# Conectarte
Connect-AzureAD

# Obtener token y listar compañías
$token = (Get-AzAccessToken -ResourceUrl "https://api.businesscentral.dynamics.com").Token
Invoke-RestMethod -Uri "https://api.businesscentral.dynamics.com/v2.0/{tenantId}/{environment}/api/v2.0/companies" -Headers @{Authorization="Bearer $token"}
```

---

## ⚙️ Paso 5: Configurar el Backend

### 5.1. Editar appsettings.Development.json

Abre el archivo `backend/appsettings.Development.json` y reemplaza los valores:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "OrderVerificationApi": "Debug"
    }
  },
  "BusinessCentral": {
    "TenantId": "TU_TENANT_ID_AQUI",
    "ClientId": "TU_CLIENT_ID_AQUI",
    "ClientSecret": "TU_CLIENT_SECRET_AQUI",
    "CompanyId": "TU_COMPANY_ID_AQUI",
    "Environment": "Sandbox"
  }
}
```

**Valores a reemplazar:**
- `TU_TENANT_ID_AQUI` → El **Directory (tenant) ID** del Paso 1.3
- `TU_CLIENT_ID_AQUI` → El **Application (client) ID** del Paso 1.3
- `TU_CLIENT_SECRET_AQUI` → El **Value** del secret del Paso 2.2
- `TU_COMPANY_ID_AQUI` → El **Company ID** del Paso 4
- `Environment` → `"Sandbox"` para desarrollo o `"Production"` para producción

### 5.2. Editar appsettings.json (Producción)

Para producción, edita `backend/appsettings.json` con los mismos valores pero usando `"Environment": "Production"`.

---

## ✅ Paso 6: Verificar la Configuración

### 6.1. Probar la Conexión

1. Ejecuta el backend:
   ```bash
   cd backend
   dotnet run
   ```

2. Abre tu navegador en: `http://localhost:9000/swagger`

3. Prueba el endpoint de health:
   - GET `/api/verify-order/health` → Debería responder `{"status": "healthy"}`

### 6.2. Probar Verificación de Orden

1. En Swagger, prueba el endpoint:
   - GET `/api/verify-order/{orderCode}`
   - Reemplaza `{orderCode}` con un número de orden real de Business Central (ej: `PO-10023`)

2. Si todo está bien configurado, deberías recibir:
   - ✅ **200 OK** con los datos de la orden, o
   - ✅ **404 Not Found** si la orden no existe

3. Si hay errores:
   - **401 Unauthorized**: Revisa Client ID, Client Secret y Tenant ID
   - **403 Forbidden**: Verifica que los permisos de API estén otorgados
   - **404 Not Found**: Verifica Company ID y Environment

---

## 🔍 Solución de Problemas Comunes

### Error: "AADSTS7000215: Invalid client secret"

**Causa**: El Client Secret es incorrecto o expiró.

**Solución**: 
1. Ve a Azure Portal → Tu App → Certificates & secrets
2. Crea un nuevo secret
3. Actualiza `ClientSecret` en `appsettings.json`

### Error: "AADSTS700016: Application was not found"

**Causa**: El Client ID es incorrecto.

**Solución**: Verifica que el `ClientId` en `appsettings.json` coincida exactamente con el Application (client) ID de Azure Portal.

### Error: "Company not found" o "404"

**Causa**: El Company ID es incorrecto o el Environment no coincide.

**Solución**: 
1. Verifica el `CompanyId` usando el Método 2 del Paso 4
2. Asegúrate de que `Environment` sea `"Sandbox"` o `"Production"` según corresponda

### Error: "Insufficient privileges"

**Causa**: Los permisos de API no están otorgados.

**Solución**: 
1. Ve a Azure Portal → Tu App → API permissions
2. Verifica que `API.ReadWrite.All` esté marcado
3. Asegúrate de que aparezca "Granted for [tu organización]" con marca verde

---

## 📝 Resumen de Valores Necesarios

Antes de comenzar, asegúrate de tener:

- [ ] **Tenant ID**: Directorio de Azure AD
- [ ] **Client ID**: ID de la aplicación registrada
- [ ] **Client Secret**: Secret generado (¡cópialo antes de cerrar la ventana!)
- [ ] **Company ID**: ID de la compañía en Business Central
- [ ] **Environment**: `Sandbox` o `Production`

---

## 🔒 Seguridad

⚠️ **IMPORTANTE**: 

- **NUNCA** subas `appsettings.json` o `appsettings.Development.json` con credenciales reales a Git
- Usa variables de entorno o Azure Key Vault en producción
- Rota los Client Secrets periódicamente
- Usa permisos mínimos necesarios (solo lectura si no necesitas escribir)

---

## 📚 Recursos Adicionales

- [Documentación de Business Central API](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/api-reference/v2.0/)
- [Autenticación Azure AD para Business Central](https://learn.microsoft.com/en-us/dynamics365/business-central/dev-itpro/administration/authenticate-web-services-using-oauth)
- [Registrar aplicación en Azure AD](https://learn.microsoft.com/en-us/azure/active-directory/develop/quickstart-register-app)

---

¿Necesitas ayuda? Revisa los logs del backend con nivel `Debug` para ver mensajes detallados de error.


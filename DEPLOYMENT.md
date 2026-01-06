# 🚀 Guía de Despliegue en Azure

Esta guía explica cómo desplegar el **Verificador de Órdenes** en Azure App Service.

## 📋 Arquitectura de Despliegue

```
┌─────────────────────────────┐
│  Azure Static Web Apps      │  ← Frontend (React + Vite)
│  portal-melcon-web           │
└──────────────┬──────────────┘
               │ HTTPS
               ▼
┌─────────────────────────────┐
│   Azure App Service          │  ← Backend (.NET 8 API)
│   portal-melcon-api          │
└──────────────┬──────────────┘
               │ OAuth2
               ▼
┌─────────────────────────────┐
│   Business Central           │
│   (API REST)                 │
└─────────────────────────────┘
```

## 🎯 Prerrequisitos

- Cuenta de Azure con suscripción activa
- Azure CLI instalado (opcional, para automatización)
- Acceso a Business Central con credenciales configuradas
- (Opcional) Azure AD B2C configurado para autenticación

## 📦 Parte 1: Desplegar el Backend (Azure App Service)

### 1.1. Crear la Web App en Azure Portal

1. Ve a [Azure Portal](https://portal.azure.com)
2. Busca **App Services** y haz clic en **Create**
3. Completa la configuración:

   **Project Details:**
   - **Subscription**: Tu suscripción de Azure
   - **Resource Group**: `Portal-Clientes` (o crea uno nuevo)

   **Instance Details:**
   - **Name**: `portal-melcon-api` (debe ser único globalmente)
   - **Publish**: Code
   - **Runtime stack**: .NET 8 (LTS)
   - **Operating System**: Windows (o Linux)
   - **Region**: Elige la región más cercana (ej: Canada Central)

   **App Service Plan:**
   - Selecciona o crea un plan (para desarrollo, el tier **Free** o **Basic B1** es suficiente)

4. Haz clic en **Review + create** y luego **Create**

### 1.2. Configurar Variables de Entorno

Una vez creada la Web App:

1. Ve a tu App Service → **Configuration** → **Application settings**
2. Agrega las siguientes variables de entorno:

   ```
   ASPNETCORE_ENVIRONMENT = Production
   
   BusinessCentral__TenantId = {tu-tenant-id}
   BusinessCentral__ClientId = {tu-client-id}
   BusinessCentral__ClientSecret = {tu-client-secret}
   BusinessCentral__CompanyId = {tu-company-id}
   BusinessCentral__Environment = Production
   
   Cors__AllowedOrigins__0 = https://portal-melcon-web.azurestaticapps.net
   Cors__AllowedOrigins__1 = https://*.azurestaticapps.net
   
   AppService__FrontendUrl = https://portal-melcon-web.azurestaticapps.net
   ```

   > ⚠️ **Importante**: Los valores con `{...}` deben reemplazarse con tus valores reales. El formato `__` (doble guion bajo) es la forma en que .NET Core lee configuraciones anidadas desde variables de entorno.

3. Si tienes Azure AD B2C configurado, agrega también:

   ```
   AzureAdB2C__Instance = https://{tenant-name}.b2clogin.com
   AzureAdB2C__Domain = {tenant-name}.onmicrosoft.com
   AzureAdB2C__TenantId = {b2c-tenant-id}
   AzureAdB2C__ClientId = {backend-api-client-id}
   AzureAdB2C__SignUpSignInPolicyId = B2C_1_signupsignin
   AzureAdB2C__Audience = api://{backend-client-id}
   ```

4. Haz clic en **Save**

### 1.3. Configurar Always On (Recomendado)

1. Ve a **Configuration** → **General settings**
2. Activa **Always On**
3. Haz clic en **Save**

Esto evita que la aplicación entre en "cold start" después de períodos de inactividad.

### 1.4. Desplegar el Código

#### Opción A: Desde Visual Studio / VS Code

1. Abre el proyecto en Visual Studio
2. Clic derecho en el proyecto `OrderVerificationApi`
3. Selecciona **Publish**
4. Elige **Azure** → **Azure App Service (Windows)** o **Azure App Service (Linux)**
5. Selecciona tu App Service `portal-melcon-api`
6. Haz clic en **Publish**

#### Opción B: Desde Azure CLI

```bash
cd backend
dotnet publish -c Release
cd bin/Release/net8.0/publish
az webapp deploy --resource-group Portal-Clientes --name portal-melcon-api --src-path .
```

#### Opción C: Desde GitHub Actions (Ver sección de CI/CD más abajo)

### 1.5. Verificar el Despliegue

1. Ve a tu App Service → **Overview**
2. Copia la **URL** (ej: `https://portal-melcon-api.azurewebsites.net`)
3. Abre en el navegador: `https://portal-melcon-api.azurewebsites.net`
4. Deberías ver: `{"service":"Order Verification API","status":"running",...}`
5. Prueba Swagger: `https://portal-melcon-api.azurewebsites.net/swagger`

## 🌐 Parte 2: Desplegar el Frontend (Azure Static Web Apps)

### 2.1. Crear Azure Static Web App

1. En Azure Portal, busca **Static Web Apps** y haz clic en **Create**
2. Completa la configuración:

   **Project Details:**
   - **Subscription**: Tu suscripción
   - **Resource Group**: `Portal-Clientes` (mismo que el backend)

   **Static Web App Details:**
   - **Name**: `portal-melcon-web`
   - **Plan type**: Free (suficiente para desarrollo)
   - **Region**: Misma región que el backend

   **Deployment Details:**
   - **Source**: GitHub (recomendado) o Azure DevOps
   - Si usas GitHub, autoriza la conexión y selecciona:
     - **Organization**: Tu organización
     - **Repository**: Tu repositorio
     - **Branch**: `main` o `master`
   - **Build Presets**: Custom

3. En **Build Details**, configura:

   ```
   App location: frontend
   Api location: (dejar vacío)
   Output location: dist
   ```

4. Haz clic en **Review + create** y luego **Create**

### 2.2. Configurar Variables de Entorno del Frontend

1. Ve a tu Static Web App → **Configuration** → **Application settings**
2. Agrega las siguientes variables:

   ```
   VITE_API_URL = https://portal-melcon-api.azurewebsites.net
   VITE_B2C_CLIENT_ID = {tu-b2c-frontend-client-id}
   VITE_B2C_AUTHORITY = https://{tenant-name}.b2clogin.com/{tenant-name}.onmicrosoft.com/B2C_1_signupsignin
   VITE_B2C_REDIRECT_URI = https://portal-melcon-web.azurestaticapps.net
   VITE_B2C_KNOWN_AUTHORITY = {tenant-name}.b2clogin.com
   VITE_B2C_SCOPES = api://{backend-client-id}/access_as_user
   ```

   > ⚠️ **Nota**: Si no usas Azure AD B2C, puedes omitir las variables `VITE_B2C_*` y establecer `VITE_AUTH_DISABLED=true`

3. Haz clic en **Save**

### 2.3. Actualizar CORS en el Backend

Asegúrate de que el backend tenga configurado CORS para permitir peticiones desde tu Static Web App:

1. Ve a tu App Service del backend → **Configuration** → **Application settings**
2. Verifica que `Cors__AllowedOrigins__0` incluya la URL de tu Static Web App
3. La URL será algo como: `https://portal-melcon-web.azurestaticapps.net`

### 2.4. Desplegar el Frontend

Si configuraste GitHub Actions en el paso 2.1, el despliegue será automático al hacer push a la rama principal.

Si prefieres desplegar manualmente:

```bash
cd frontend
npm install
npm run build
# Luego usa Azure CLI o el portal para subir los archivos de frontend/dist
```

## 🔄 Parte 3: Configurar CI/CD (Opcional pero Recomendado)

### 3.1. GitHub Actions para Backend

Crea `.github/workflows/deploy-backend.yml`:

```yaml
name: Deploy Backend to Azure

on:
  push:
    branches: [ main ]
    paths:
      - 'backend/**'
  workflow_dispatch:

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Restore dependencies
        run: dotnet restore backend/OrderVerificationApi.csproj
      
      - name: Build
        run: dotnet build backend/OrderVerificationApi.csproj --configuration Release --no-restore
      
      - name: Publish
        run: dotnet publish backend/OrderVerificationApi.csproj --configuration Release --no-build --output ./publish
      
      - name: Deploy to Azure Web App
        uses: azure/webapps-deploy@v2
        with:
          app-name: 'portal-melcon-api'
          publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
          package: ./publish
```

### 3.2. Obtener Publish Profile

1. Ve a tu App Service → **Get publish profile**
2. Descarga el archivo `.PublishSettings`
3. En GitHub, ve a **Settings** → **Secrets** → **Actions**
4. Crea un nuevo secret llamado `AZURE_WEBAPP_PUBLISH_PROFILE`
5. Pega el contenido completo del archivo `.PublishSettings`

### 3.3. GitHub Actions para Frontend

Si usas Azure Static Web Apps con GitHub, el workflow se crea automáticamente. Solo necesitas hacer push a la rama principal.

## ✅ Verificación Post-Despliegue

### Backend

1. **Health Check**: `https://portal-melcon-api.azurewebsites.net/`
   - Debe retornar: `{"service":"Order Verification API","status":"running",...}`

2. **Swagger**: `https://portal-melcon-api.azurewebsites.net/swagger`
   - Debe mostrar la documentación de la API

3. **Endpoint de Verificación**: `https://portal-melcon-api.azurewebsites.net/api/verify-order/{orderCode}`
   - Reemplaza `{orderCode}` con un código de orden real

### Frontend

1. Abre la URL de tu Static Web App: `https://portal-melcon-web.azurestaticapps.net`
2. Verifica que la página carga correctamente
3. Intenta verificar una orden de compra
4. Verifica en la consola del navegador que no hay errores de CORS

## 🔧 Troubleshooting

### Error: CORS bloqueado

**Síntoma**: Error en consola del navegador: `Access to fetch at '...' has been blocked by CORS policy`

**Solución**:
1. Verifica que la URL del frontend esté en `Cors__AllowedOrigins` del backend
2. Asegúrate de que el backend tenga `app.UseCors("FrontendPolicy")` en el pipeline
3. Reinicia el App Service después de cambiar configuración

### Error: 500 Internal Server Error

**Síntoma**: El backend retorna error 500

**Solución**:
1. Ve a **App Service** → **Log stream** para ver logs en tiempo real
2. Verifica que todas las variables de entorno estén configuradas correctamente
3. Verifica que el formato de las variables de entorno use `__` (doble guion bajo) para secciones anidadas

### Error: Frontend no se conecta al backend

**Síntoma**: El frontend muestra "Error de conexión"

**Solución**:
1. Verifica que `VITE_API_URL` en Static Web Apps apunte a la URL correcta del backend
2. Verifica que el backend esté funcionando (health check)
3. Verifica que no haya problemas de CORS

### Error: Variables de entorno no se aplican

**Síntoma**: Los cambios en Application Settings no se reflejan

**Solución**:
1. Después de cambiar Application Settings, haz clic en **Save**
2. Reinicia el App Service: **Overview** → **Restart**
3. Espera 1-2 minutos para que los cambios se apliquen

## 📝 Checklist de Despliegue

- [ ] Backend desplegado en Azure App Service
- [ ] Variables de entorno del backend configuradas
- [ ] Always On habilitado en el backend
- [ ] Frontend desplegado en Azure Static Web Apps
- [ ] Variables de entorno del frontend configuradas
- [ ] CORS configurado correctamente
- [ ] Health check del backend funciona
- [ ] Frontend se conecta al backend correctamente
- [ ] Swagger accesible
- [ ] Verificación de órdenes funciona end-to-end
- [ ] (Opcional) CI/CD configurado

## 🔐 Seguridad

### Secrets y Credenciales

- ⚠️ **NUNCA** commitees secrets en el código
- Usa **Azure Key Vault** para secrets sensibles en producción
- Usa **Application Settings** en Azure Portal para desarrollo
- Rota los Client Secrets periódicamente

### HTTPS

- Azure App Service y Static Web Apps proporcionan HTTPS automáticamente
- Los certificados SSL se renuevan automáticamente
- No es necesario configurar certificados manualmente

## 💰 Costos Estimados

Para desarrollo/testing:

- **Azure App Service (Free tier)**: $0 USD/mes
  - Limitado a 1 GB de RAM, 1 GB de almacenamiento
  - No incluye Always On
- **Azure Static Web Apps (Free tier)**: $0 USD/mes
  - 100 GB de ancho de banda, 100 GB de almacenamiento

Para producción, considera:

- **Azure App Service (Basic B1)**: ~$13 USD/mes
  - 1.75 GB RAM, 10 GB almacenamiento
  - Incluye Always On
- **Azure Static Web Apps (Standard)**: ~$9 USD/mes
  - 100 GB ancho de banda incluido, luego $0.08/GB

## 📚 Referencias

- [Documentación de Azure App Service](https://docs.microsoft.com/azure/app-service/)
- [Documentación de Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps/)
- [Configuración de Business Central](backend/CONFIGURACION_BC.md)
- [Configuración de Azure AD B2C](backend/B2C_SETUP.md)


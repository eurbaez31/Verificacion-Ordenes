# GitHub Actions Workflows

Este directorio contiene los workflows de CI/CD para desplegar automáticamente el proyecto en Azure.

## Workflows Disponibles

### 1. `deploy-backend.yml`

Despliega el backend (.NET 8) a Azure App Service.

**Trigger:**
- Push a las ramas `main` o `master` cuando hay cambios en `backend/`
- Ejecución manual desde GitHub Actions

**Secrets Requeridos:**
- `AZURE_WEBAPP_PUBLISH_PROFILE`: Perfil de publicación de Azure App Service

**Cómo obtener el Publish Profile:**
1. Ve a tu App Service en Azure Portal
2. Haz clic en **Get publish profile**
3. Descarga el archivo `.PublishSettings`
4. En GitHub, ve a **Settings** → **Secrets and variables** → **Actions**
5. Crea un nuevo secret llamado `AZURE_WEBAPP_PUBLISH_PROFILE`
6. Pega el contenido completo del archivo `.PublishSettings`

### 2. `deploy-frontend.yml`

Despliega el frontend (React + Vite) a Azure Static Web Apps.

**Trigger:**
- Push a las ramas `main` o `master` cuando hay cambios en `frontend/`
- Ejecución manual desde GitHub Actions

**Secrets Requeridos:**
- `AZURE_STATIC_WEB_APPS_API_TOKEN`: Token de API de Azure Static Web Apps
- `VITE_API_URL`: (Opcional) URL del backend API
- `VITE_B2C_*`: (Opcional) Variables de Azure AD B2C

**Cómo obtener el API Token:**
1. Ve a tu Static Web App en Azure Portal
2. Ve a **Manage deployment token**
3. Copia el token
4. En GitHub, crea un secret llamado `AZURE_STATIC_WEB_APPS_API_TOKEN`

**Nota:** Si usas Azure Static Web Apps con integración de GitHub, el workflow se crea automáticamente. Puedes usar este workflow como alternativa o para personalización.

## Configuración Inicial

1. **Crear los secrets en GitHub:**
   - Ve a tu repositorio → **Settings** → **Secrets and variables** → **Actions**
   - Agrega todos los secrets mencionados arriba

2. **Ajustar nombres de recursos (si es necesario):**
   - Edita los archivos `.yml` y cambia `AZURE_WEBAPP_NAME` si tu App Service tiene otro nombre
   - Ajusta las rutas y configuraciones según tu estructura de proyecto

3. **Hacer push a la rama principal:**
   - Los workflows se ejecutarán automáticamente cuando detecten cambios

## Troubleshooting

### El workflow falla en "Deploy to Azure Web App"

- Verifica que el secret `AZURE_WEBAPP_PUBLISH_PROFILE` esté configurado correctamente
- Asegúrate de que el nombre de la App Service en el workflow coincida con el nombre real
- Verifica que tengas permisos para desplegar en el App Service

### El workflow falla en "Deploy to Azure Static Web Apps"

- Verifica que el secret `AZURE_STATIC_WEB_APPS_API_TOKEN` esté configurado
- Asegúrate de que la Static Web App esté configurada para usar GitHub Actions
- Verifica que las rutas `app_location` y `output_location` sean correctas

### El build del frontend falla

- Verifica que todas las variables de entorno necesarias estén configuradas como secrets
- Revisa los logs del workflow para ver errores específicos de compilación
- Asegúrate de que `package-lock.json` esté commiteado


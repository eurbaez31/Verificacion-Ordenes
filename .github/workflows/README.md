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

### 2. `azure-static-web-apps-red-smoke-01601b70f.yml`

Despliega el frontend (React + Vite) a Azure Static Web Apps. Este workflow es creado y gestionado automáticamente por Azure.

**Trigger:**
- Push a la rama `main`
- Pull requests hacia `main`

**Secrets Requeridos:**
- `AZURE_STATIC_WEB_APPS_API_TOKEN_RED_SMOKE_01601B70F`: Token de API creado automáticamente por Azure Static Web Apps

**Configuración:**
Este workflow fue generado automáticamente al crear la Static Web App en Azure Portal con integración de GitHub. El token se configura automáticamente como secret en el repositorio.

**Variables de entorno:**
Las variables de entorno del frontend (como `VITE_API_URL`) deben configurarse en Azure Portal:
1. Ve a Azure Portal → Static Web Apps → `portal-melcon-web`
2. Ve a **Configuration** → **Application settings**
3. Agrega las variables necesarias

**Nota:** Este es el método recomendado para desplegar a Azure Static Web Apps ya que Azure lo mantiene actualizado automáticamente.

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

- Verifica que el secret `AZURE_STATIC_WEB_APPS_API_TOKEN_RED_SMOKE_01601B70F` esté configurado
- Asegúrate de que la Static Web App esté configurada para usar GitHub Actions
- Verifica que las rutas `app_location` y `output_location` sean correctas (deben ser `./frontend` y `dist`)

### El build del frontend falla

- Verifica que todas las variables de entorno estén configuradas en Azure Portal (no en GitHub secrets)
- Revisa los logs del workflow para ver errores específicos de compilación
- Asegúrate de que `package-lock.json` esté commiteado en el directorio `frontend/`


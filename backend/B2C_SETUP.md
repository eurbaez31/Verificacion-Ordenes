# Configuración de Azure AD B2C para Portal de Proveedores

Esta guía explica cómo configurar Azure AD B2C (Entra External ID) para habilitar la autenticación en el portal de proveedores.

## Requisitos Previos

- Acceso a Azure Portal con permisos de administrador
- Un tenant de Azure AD B2C creado
- Business Central configurado con el campo "Vendor ID" en la tabla Vendor

## Pasos de Configuración

### 1. Crear App Registration para Frontend (SPA)

1. En Azure Portal, ve a **Azure AD B2C** > **Registros de aplicaciones**
2. Haz clic en **Nueva inscripción**
3. Configura:
   - **Nombre**: `Portal Proveedores Frontend`
   - **Tipos de cuenta admitidos**: Cuentas en cualquier directorio de identidad o proveedor de identidades
   - **URI de redirección**: 
     - Desarrollo: `http://localhost:8081`
     - Producción: `https://tu-dominio.com`
   - **Permisos**: Marca "Conceder consentimiento de administrador"
4. Copia el **Id. de aplicación (cliente)** → será `VITE_B2C_CLIENT_ID`

### 2. Crear App Registration para Backend (API)

1. En Azure Portal, ve a **Azure AD B2C** > **Registros de aplicaciones**
2. Haz clic en **Nueva inscripción**
3. Configura:
   - **Nombre**: `Portal Proveedores API`
   - **Tipos de cuenta admitidos**: Cuentas en cualquier directorio de identidad o proveedor de identidades
   - **URI de redirección**: No necesario para API
4. Ve a **Exponer una API**:
   - Establece el **URI de id. de aplicación**: `api://{client-id}`
   - Agrega un scope: `access_as_user` con descripción "Acceso al portal de proveedores"
5. Ve a **Permisos de API** > **Agregar un permiso** > **Mis APIs**:
   - Selecciona `Portal Proveedores API`
   - Marca `access_as_user`
   - Haz clic en **Agregar permisos**
6. Copia el **Id. de aplicación (cliente)** → será `ClientId` en backend

### 3. Configurar Permisos del Frontend

1. En la app registration del frontend, ve a **Permisos de API**
2. Haz clic en **Agregar un permiso** > **Mis APIs**
3. Selecciona `Portal Proveedores API` y marca `access_as_user`
4. Haz clic en **Agregar permisos**
5. Haz clic en **Conceder consentimiento de administrador**

### 4. Crear Custom Attribute para Vendor ID

1. En Azure AD B2C, ve a **Atributos de usuario**
2. Haz clic en **Agregar**
3. Configura:
   - **Nombre**: `VendorID`
   - **Tipo de datos**: String
   - **Descripción**: "Vendor Portal ID (GUID)"
4. Copia el **Nombre completo** (ej: `extension_{app-id}_VendorID`) → será el claim `extension_vendorId`

### 5. Configurar User Flow o Custom Policy

1. Ve a **Flujos de usuario** o **Directivas personalizadas**
2. Si usas **Flujos de usuario**:
   - Crea o edita un flujo "Registro e inicio de sesión"
   - En **Atributos de usuario**, incluye el atributo `VendorID` que creaste
   - En **Aplicaciones**, selecciona ambas app registrations
   - Copia el nombre del flujo (ej: `B2C_1_signupsignin`)

### 6. Configurar Variables de Entorno

#### Frontend (.env)

```env
VITE_API_URL=http://localhost:9000
VITE_B2C_CLIENT_ID={frontend-client-id}
VITE_B2C_AUTHORITY=https://{tenant-name}.b2clogin.com/{tenant-name}.onmicrosoft.com/B2C_1_signupsignin
VITE_B2C_KNOWN_AUTHORITY={tenant-name}.b2clogin.com
VITE_B2C_REDIRECT_URI=http://localhost:8081
VITE_B2C_SCOPES=api://{backend-client-id}/access_as_user
```

#### Backend (appsettings.Development.json)

```json
{
  "AzureAdB2C": {
    "Instance": "https://{tenant-name}.b2clogin.com",
    "Domain": "{tenant-name}.onmicrosoft.com",
    "TenantId": "{b2c-tenant-id}",
    "ClientId": "{backend-api-client-id}",
    "SignUpSignInPolicyId": "B2C_1_signupsignin",
    "Audience": "api://{backend-client-id}"
  }
}
```

## Población del Vendor ID en Business Central

Cada proveedor en BC debe tener su campo "Vendor ID" poblado con un GUID determinístico generado desde el Vendor No.:

```csharp
// Ejemplo en C# (script o proceso)
var vendorId = VendorIdHelper.CreateFromVendorNo("PRV000069");
// Guardar este GUID en el campo "Vendor ID" del Vendor en BC
```

## Asignar Vendor ID a Usuarios B2C

Cuando un proveedor se registra en el portal:

1. Obtener su Vendor No. desde BC (puede ser manual o automático)
2. Generar el Vendor ID determinístico: `VendorIdHelper.CreateFromVendorNo(vendorNo)`
3. Asignar este GUID al atributo personalizado `extension_vendorId` del usuario en B2C

Esto puede hacerse:
- Manualmente desde Azure Portal (editar usuario > atributos)
- Via Microsoft Graph API desde el backend
- Durante el proceso de registro si tienes un endpoint de registro

## Verificación

1. Inicia sesión en el portal con un usuario que tenga `extension_vendorId` configurado
2. Verifica una orden que pertenezca a ese proveedor
3. Intenta descargar el PDF → debe funcionar
4. Ve a Historial → debe mostrar solo las órdenes del proveedor
5. Intenta descargar una orden de otro proveedor → debe dar 403 Forbidden

## Solución de Problemas

### Error: "Usuario no tiene Vendor ID asignado"
- Verifica que el usuario en B2C tenga el atributo `extension_vendorId` configurado
- Verifica que el nombre del claim en el código coincida con el nombre completo del atributo en B2C

### Error: "No se encontró el proveedor asociado"
- Verifica que el Vendor ID en BC coincida con el GUID del claim del usuario
- Verifica que la API Portal Vendors esté publicada y accesible

### Error: 401 Unauthorized
- Verifica que el scope en frontend coincida con el expuesto en backend
- Verifica que el Audience en backend coincida con el Client ID del backend API


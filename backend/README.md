# Order Verification API - Backend

API de verificación de órdenes de compra que conecta con Business Central.

## Arquitectura

```
Frontend React ←→ Este Backend (.NET Core) ←→ Business Central
```

## Requisitos

- .NET 8.0 SDK
- Credenciales de Azure AD para acceder a Business Central

## Configuración

### 1. Registrar App en Azure AD

1. Ir a [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations
2. Crear nueva aplicación
3. Configurar permisos de API para Business Central:
   - `Dynamics 365 Business Central` → `API.ReadWrite.All`
4. Crear un Client Secret

### 2. Configurar Variables de Entorno

Editar `appsettings.Development.json` (desarrollo) o `appsettings.json` (producción):

```json
{
  "BusinessCentral": {
    "TenantId": "tu-tenant-id-de-azure",
    "ClientId": "tu-client-id-de-la-app",
    "ClientSecret": "tu-client-secret",
    "CompanyId": "id-de-la-compania-en-bc",
    "Environment": "Sandbox"
  }
}
```

### 3. Obtener Company ID

El Company ID se puede obtener llamando a:
```
https://api.businesscentral.dynamics.com/v2.0/{tenantId}/{environment}/api/v2.0/companies
```

## Ejecutar

```bash
cd backend
dotnet run
```

La API estará disponible en: `http://localhost:9000`

## Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/verify-order/{orderCode}` | Verifica una orden de compra |
| GET | `/api/verify-order/health` | Health check |
| GET | `/swagger` | Documentación Swagger |

## Respuestas

### Orden Verificada
```json
{
  "success": true,
  "status": "verified",
  "data": {
    "orderNumber": "PO-10023",
    "date": "15 enero 2026",
    "vendor": "Proveedor XYZ",
    "department": "Compras",
    "items": [...],
    "total": 5800.00,
    "status": "Aprobada",
    "approvedBy": "Admin"
  }
}
```

### Orden No Encontrada
```json
{
  "success": false,
  "status": "not_found",
  "message": "Orden no encontrada"
}
```

## CORS

Configurado para permitir peticiones desde:
- `http://localhost:3000`
- `http://localhost:3002`
- `http://localhost:5173`

Para producción, agregar tu dominio en `Program.cs`.


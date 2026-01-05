# Prueba de Conexión con Business Central

## ✅ Estado Actual

- ✅ Backend compilado correctamente
- ✅ Configuración cargada desde `appsettings.Development.json`
- ✅ API respondiendo en `http://localhost:9000`

## 🧪 Probar la Conexión

### Opción 1: Usando Swagger UI (Recomendado)

1. Abre tu navegador en: `http://localhost:9000/swagger`

2. Busca el endpoint: `GET /api/verify-order/{orderCode}`

3. Haz clic en **Try it out**

4. Ingresa un número de orden real de Business Central (ej: `PO-10023` o el formato que uses)

5. Haz clic en **Execute**

6. Revisa la respuesta:
   - ✅ **200 OK**: Conexión exitosa, orden encontrada
   - ✅ **404 Not Found**: Conexión exitosa, pero la orden no existe
   - ❌ **401 Unauthorized**: Problema con credenciales (Client ID/Secret)
   - ❌ **403 Forbidden**: Permisos no otorgados
   - ❌ **500 Internal Server Error**: Revisa los logs del backend

### Opción 2: Usando PowerShell

```powershell
# Probar con un número de orden real
$orderCode = "PO-10023"  # Reemplaza con un número real
$response = Invoke-WebRequest -Uri "http://localhost:9000/api/verify-order/$orderCode" -UseBasicParsing
$response.Content | ConvertFrom-Json | ConvertTo-Json -Depth 10
```

### Opción 3: Usando curl (si tienes Git Bash o WSL)

```bash
curl -X GET "http://localhost:9000/api/verify-order/PO-10023" \
  -H "accept: application/json"
```

## 🔍 Verificar Logs

Si hay errores, revisa los logs del backend. Con el nivel `Debug` configurado, verás mensajes detallados:

- ✅ Token obtenido correctamente
- ✅ Consulta a Business Central exitosa
- ❌ Errores de autenticación
- ❌ Errores de conexión

## 📝 Formato del Número de Orden

El número de orden debe coincidir exactamente con el formato en Business Central:
- Puede ser: `PO-10023`, `10023`, `PO2024-001`, etc.
- Depende de cómo esté configurado en tu BC

## ⚠️ Errores Comunes

### Error 401: Unauthorized
**Causa**: Credenciales incorrectas
**Solución**: 
- Verifica `ClientId` y `ClientSecret` en `appsettings.Development.json`
- Asegúrate de que el secret no haya expirado

### Error 403: Forbidden
**Causa**: Permisos no otorgados
**Solución**: 
- Ve a Azure Portal → Tu App → API permissions
- Verifica que `API.ReadWrite.All` tenga consentimiento otorgado (marca verde)

### Error 404: Company not found
**Causa**: Company ID incorrecto o Environment incorrecto
**Solución**: 
- Verifica `CompanyId` en `appsettings.Development.json`
- Asegúrate de que `Environment` sea `"Sandbox"` o `"Production"` según corresponda

### Error 500: Internal Server Error
**Causa**: Error en la conexión o formato de datos
**Solución**: 
- Revisa los logs del backend para ver el error específico
- Verifica que Business Central esté accesible desde tu red

## ✅ Prueba Exitosa

Si todo funciona correctamente, deberías recibir una respuesta como:

```json
{
  "success": true,
  "status": "verified",
  "data": {
    "orderNumber": "PO-10023",
    "date": "15 enero 2026",
    "vendor": "Nombre del Proveedor",
    "department": "Compras",
    "items": [
      {
        "description": "Descripción del artículo",
        "quantity": 10,
        "unitPrice": 150.50
      }
    ],
    "total": 1505.00,
    "status": "Aprobada",
    "approvedBy": "Administrador"
  }
}
```

## 🚀 Siguiente Paso

Una vez que la conexión funcione correctamente:
1. Prueba desde el frontend: `http://localhost:5173`
2. Escanea un código QR o ingresa un número de orden manualmente
3. Verifica que los datos se muestren correctamente


# Depuración: Orden PC0008000 No Encontrada

## Problema

La orden `PC0008000` existe en Business Central pero la API devuelve "not_found".

## Pasos para Diagnosticar

### 1. Listar Órdenes Disponibles

**Endpoint de depuración**: `GET /api/verify-order/debug/list-orders`

En Swagger:
- Ve a `http://localhost:9000/swagger`
- Busca `GET /api/verify-order/debug/list-orders`
- Haz clic en "Try it out" → "Execute"

Esto te mostrará las primeras 10 órdenes que devuelve Business Central, incluyendo sus números.

**Verifica:**
- ¿Aparece `PC0008000` en la lista?
- ¿El formato del número es exactamente el mismo?
- ¿Hay espacios, guiones u otros caracteres?

### 2. Revisar los Logs del Backend

Cuando ejecutes la búsqueda de `PC0008000`, revisa la consola del backend. Deberías ver mensajes como:

```
Consultando Business Central para orden: PC0008000
Órdenes encontradas: 0
```

O si hay algún error:
```
Error al consultar Business Central: [StatusCode] - [Content]
```

### 3. Posibles Causas

#### A) Formato del Número Diferente
El número en BC podría tener:
- Espacios adicionales: `"PC0008000 "` o `" PC0008000"`
- Guiones: `"PC-0008000"`
- Formato diferente: `"0008000"` (sin el prefijo PC)

**Solución**: Compara el número exacto en la lista de depuración.

#### B) Tipo de Documento
El endpoint `purchaseOrders` podría filtrar por tipo de documento. Verifica en BC:
- ¿Es realmente una "Orden de Compra" (Purchase Order)?
- ¿O es otro tipo como "Pedido" o "Solicitud"?

#### C) Estado de la Orden
Algunas APIs de BC no devuelven órdenes en ciertos estados (borradores, canceladas, etc.).

**Verifica en BC:**
- ¿Cuál es el estado de la orden PC0008000?
- ¿Está "Released" (Liberada/Aprobada)?

#### D) Filtro OData
El filtro `number eq 'PC0008000'` podría no funcionar correctamente.

**Alternativa**: Podríamos intentar sin filtro y buscar en el cliente, o usar `contains` en lugar de `eq`.

### 4. Pruebas Adicionales

#### Probar sin filtro (obtener todas y filtrar en código)
Si el endpoint de depuración devuelve órdenes, pero el filtro no funciona, podríamos:
1. Obtener todas las órdenes
2. Filtrar por número en el código C#

#### Probar con filtro diferente
```odata
$filter=startswith(number, 'PC0008')
```
O
```odata
$filter=contains(number, 'PC0008000')
```

### 5. Verificar en Business Central API directamente

Si tienes acceso, puedes probar la API directamente:

```powershell
# Obtener token
$token = "tu-token-aqui"

# Probar el endpoint
Invoke-RestMethod -Uri "https://api.businesscentral.dynamics.com/v2.0/{tenantId}/{environment}/api/v2.0/companies({companyId})/purchaseOrders?`$filter=number eq 'PC0008000'" `
  -Headers @{Authorization="Bearer $token"}
```

## Solución Temporal

Si necesitas que funcione ya, podemos modificar el código para:
1. Obtener todas las órdenes (o las últimas N)
2. Filtrar por número en el código C# en lugar de usar OData filter

Esto es menos eficiente pero más confiable.

## Información Necesaria

Por favor, comparte:
1. El resultado de `/api/verify-order/debug/list-orders`
2. Los logs del backend al buscar PC0008000
3. El estado de la orden en Business Central
4. El tipo de documento en Business Central

Con esta información podremos identificar y solucionar el problema.


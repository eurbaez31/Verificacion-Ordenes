# Business Central Extension - Portal de Proveedores

Esta carpeta contiene los archivos AL necesarios para extender Business Central y habilitar las funcionalidades del portal de proveedores.

## Estructura del proyecto

Los archivos están organizados en carpetas por tipo de objeto:

```
bc-extension/
├── Tables/
│   ├── VendorTableExtension.al (60368)
│   └── PortalSetupTable.al (60370)
├── Pages/
│   └── PortalSetupPage.al (60370)
├── APIs/
│   ├── PortalVendorsAPI.al (60368)
│   └── PurchaseOrderPdfAPI.al (60369)
├── Codeunits/
│   └── QRCodeGenerator.al (60371)
├── Reports/
│   ├── PurchaseOrderReportExtension.al (60372)
│   └── Layouts/
│       └── Pedido-QR.rdlc
└── README.md
```

## Archivos incluidos

### Tables/

1. **VendorTableExtension.al** (60368)
   - Agrega el campo `Vendor ID` (Guid) a la tabla Vendor
   - Este campo almacenará el GUID determinístico generado desde el Vendor No.

2. **PortalSetupTable.al** (60370)
   - Tabla de configuración para almacenar la URL base del portal de verificación
   - Campos: Portal Base URL, Enabled
   - Singleton record (solo un registro con Primary Key = 'PORTAL')

### Pages/

3. **PortalSetupPage.al** (60370)
   - Página de configuración para gestionar la URL del portal
   - Permite configurar la URL base y habilitar/deshabilitar la generación de QR
   - Incluye acción para probar la URL

### APIs/

4. **PortalVendorsAPI.al** (60368)
   - API Page que expone información de proveedores para el portal
   - Permite mapear Vendor ID (GUID) -> Vendor No.
   - Filtra solo proveedores que tengan Vendor ID asignado

5. **PurchaseOrderPdfAPI.al** (60369)
   - API Page que genera el PDF del Purchase Order
   - Usa el mismo reporte configurado en Report Selections para Purchase Order
   - Devuelve el PDF en formato Base64

### Codeunits/

6. **QRCodeGenerator.al** (60371) - "Melcon QR Code Generator"
   - Codeunit para generar códigos QR usando servicio HTTP externo (api.qrserver.com)
   - Métodos: GetQRCodeAsBase64(), GetQRCodeImageURL()
   - Genera QR codes de 150x150 píxeles
   - Retorna imágenes en formato Base64 para uso en reportes RDLC

### Reports/

7. **PurchaseOrderReportExtension.al** (60372) - "Pedido-QR"
   - Extensión del reporte estándar 405 "Purchase Order" (Purchase - Order)
   - Agrega un layout RDLC adicional llamado "Pedido-QR"
   - Expone columnas en el dataset: PortalBaseUrl, VerificationUrl, QRCodeBase64
   - El layout RDLC incluye un PageFooter con:
     - Texto: "Validar órdenes en: {PortalBaseUrl}"
     - Imagen QR decodificada desde Base64
   - El QR contiene: {PortalBaseURL}?code={PurchaseOrderNo}

## Instrucciones de publicación

1. Copia estos archivos a tu proyecto AL de Business Central
2. Asegúrate de que los números de objeto (60368, 60369) no entren en conflicto con otros objetos
3. Compila y publica la extensión en tu entorno de Business Central
4. Asigna permisos apropiados a la aplicación Entra ID que usa el backend

## Población del campo Vendor ID

El campo `Vendor ID` debe poblarse usando el mismo algoritmo que `CustomerIdHelper` en la plataforma:

```csharp
// Ejemplo en C# (backend)
var vendorId = VendorIdHelper.CreateFromVendorNo("PRV000069");
```

Este GUID determinístico se guardará en el campo `Vendor ID` de cada Vendor en BC.

## Configuración de Códigos QR en Pedidos de Compra

### Requisitos
- Extensión publicada en Business Central
- Acceso a internet para el servicio de generación de QR (api.qrserver.com)

### Pasos de configuración

1. **Configurar la URL del Portal**:
   - Abre la página **"Portal Setup"** (búsqueda: 60370 o "Portal Setup")
   - Ingresa la **URL base del portal** (ej: `https://verificacion.melcon.com` o `http://localhost:8081` para desarrollo)
   - Marca la casilla **"Enabled"** para habilitar la generación de códigos QR
   - Opcional: Usa la acción **"Probar URL"** para verificar que la URL funciona

2. **Formato de la URL en el QR**:
   - El código QR generado contendrá: `{PortalBaseURL}?code={PurchaseOrderNo}`
   - Ejemplo: `https://verificacion.melcon.com?code=PC0007998`

3. **Generación automática**:
   - Al imprimir o visualizar un Purchase Order, el reporte extension se ejecuta automáticamente
   - Si está habilitado, genera el código QR y lo incluye en el reporte
   - El QR aparece en el layout del reporte (requiere modificar el layout RDLC/Word)

### Asignar el Layout "Pedido-QR" en Business Central

Después de publicar la extensión, debes asignar el layout RDLC manualmente:

1. **En Business Central**:
   - Ve a **Report Layout Selection** (búsqueda: "Report Layout Selection")
   - Selecciona **Usage = "P.Order"** (Purchase Order)
   - Crea un nuevo registro o edita el existente
   - En **Custom Report Layout**, selecciona o crea un layout con:
     - **Name**: "Pedido-QR"
     - **Type**: RDLC
     - **Layout File**: Sube el archivo `bc-extension/Reports/Layouts/Pedido-QR.rdlc`
   - Guarda y asigna este layout como predeterminado para Purchase Order

2. **Verificar el Layout**:
   - El layout ya incluye un **PageFooter** con:
     - Texto visible: "Validar órdenes en: {PortalBaseUrl}"
     - Imagen QR (2.0cm x 2.0cm) en la esquina inferior derecha
   - El QR se genera automáticamente si está habilitado en Portal Setup

### Notas importantes

- El servicio de generación de QR (api.qrserver.com) es gratuito pero requiere conexión a internet
- El tamaño del QR generado es 150x150 píxeles (configurable en QRCodeGenerator.al)
- En el layout RDLC, el QR se muestra a 2.0cm x 2.0cm (ajustable en Pedido-QR.rdlc)
- La generación de QR solo ocurre si está habilitado en Portal Setup
- El QR se genera dinámicamente para cada pedido de compra
- El layout "Pedido-QR" debe asignarse manualmente en Report Layout Selection después de publicar

## Permisos requeridos

La aplicación Entra ID usada por el backend debe tener permisos para:
- Leer Vendor (para Portal Vendors API)
- Leer Purchase Header (para Purchase Order PDF API)
- Ejecutar reportes (para generar PDFs)

Para la generación de QR:
- Acceso HTTP saliente (para conectar con api.qrserver.com)
- Permisos para leer Portal Setup


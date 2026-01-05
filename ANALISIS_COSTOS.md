# 📊 Análisis de Costos de Desarrollo: Verificador de Órdenes

**Fecha:** 2 de Enero, 2025  
**Proyecto:** Verificador de Órdenes de Compra - Portal para Proveedores  
**Versión Analizada:** 1.0.0  
**Tarifa Base:** $90 USD / hora

---

## 1. Resumen Ejecutivo

Este documento detalla la estimación de costos para el desarrollo del **Verificador de Órdenes de Compra**, una solución completa que incluye un backend .NET 8, un frontend React + TypeScript, y una extensión de Business Central en AL. La plataforma presenta una complejidad **alta** debido a:

- Integración profunda con Business Central (OAuth2 Client Credentials, APIs REST personalizadas, búsqueda en órdenes activas y archivadas)
- Sistema de autenticación con Azure AD B2C (Entra External ID) para proveedores
- Generación de códigos QR en Business Central para escaneo directo
- Descarga de PDFs de órdenes de compra desde el portal
- Verificación pública de órdenes sin autenticación
- Historial de órdenes para proveedores autenticados
- Extensión AL completa para Business Central (tablas, páginas, APIs, reportes)

| Concepto | Estimación |
|----------|------------|
| **Total de horas estimadas** | **180 horas** (Promedio realista) |
| **Rango de horas** | 150 - 220 horas |
| **Costo estimado (Realista)** | **$16,200 USD** |
| **Rango de costo** | $13,500 - $19,800 USD |

---

## 2. Desglose Detallado por Componente

### A. Backend - Modelos (Models)

Definición de DTOs y modelos para comunicación con Business Central y frontend.

| Archivo / Componente | Complejidad | Horas Est. |
|----------------------|-------------|------------|
| `OrderDetails.cs` | **Baja**. Modelo simple para respuesta al frontend. | 1 - 2 hrs |
| `OrderItemDto.cs` | **Baja**. DTO para items de orden. | 1 - 1.5 hrs |
| `VerificationResponse.cs` | **Baja**. Modelo de respuesta estándar. | 1 - 2 hrs |
| `BCPurchaseOrder.cs` | **Media**. Modelo complejo con mapeo JSON, incluye `BCPurchaseOrderArchive` y `BCODataResponse<T>`. | 4 - 6 hrs |
| `PortalVendor.cs` | **Baja**. Modelo para API de proveedores. | 1 - 2 hrs |
| `PurchaseOrderPdf.cs` | **Baja**. Modelo para PDF en Base64. | 1 - 1.5 hrs |
| **Subtotal** | | **9 - 14.5 hrs** |

### B. Backend - Servicios

Implementación de servicios para integración con Business Central y resolución de proveedores.

| Archivo / Componente | Detalles Técnicos | Horas Est. |
|----------------------|-------------------|------------|
| `BusinessCentralService.cs` | **Muy Alta**. ~514 líneas. OAuth2 Client Credentials, cache de tokens, búsqueda en órdenes activas y archivadas, manejo de múltiples entornos, APIs REST personalizadas (Purchase Orders, Portal Vendors, Purchase Order PDF), conversión de órdenes archivadas, logging detallado. | 24 - 32 hrs |
| `IBusinessCentralService.cs` | **Baja**. Interfaz del servicio. | 1 - 1.5 hrs |
| `VendorResolverService.cs` | **Media-Alta**. Resolución de Vendor No. desde claims de B2C, integración con Portal Vendors API. | 4 - 6 hrs |
| `IVendorResolverService.cs` | **Baja**. Interfaz del servicio. | 0.5 - 1 hrs |
| `VendorIdHelper.cs` | **Media**. Generación determinística de GUIDs desde Vendor No. (similar a CustomerIdHelper). | 3 - 4 hrs |
| **Subtotal** | | **32.5 - 44.5 hrs** |

### C. Backend - Controladores

Endpoints REST con lógica de negocio y autorización.

| Archivo / Componente | Detalles Técnicos | Horas Est. |
|----------------------|-------------------|------------|
| `OrderVerificationController.cs` | **Alta**. ~316 líneas. 4 endpoints: verificación pública (sin auth), descarga PDF (con auth), historial de órdenes (con auth), debug. Validación de estados de orden (Open, Pending approval), mapeo de estados BC a español, autorización por proveedor, manejo de errores robusto. | 14 - 18 hrs |
| **Subtotal** | | **14 - 18 hrs** |

### D. Backend - Configuración y Middleware

Configuración de servicios, autenticación, CORS, y middleware.

| Componente | Detalles | Horas Est. |
|------------|----------|------------|
| `Program.cs` | **Alta**. Configuración de autenticación JWT Bearer (B2C), autorización, CORS, Swagger, HttpClient, servicios, manejo de placeholders en configuración B2C. | 8 - 12 hrs |
| `appsettings.json` / `appsettings.Development.json` | Configuración de Business Central y Azure AD B2C. | 1 - 2 hrs |
| **Subtotal** | | **9 - 14 hrs** |

### E. Frontend - Páginas Principales

Páginas React con lógica de negocio y estado.

| Archivo / Componente | Detalles Técnicos | Horas Est. |
|----------------------|-------------------|------------|
| `Index.tsx` | **Media-Alta**. Página principal con lectura de parámetros URL (QR), auto-verificación, manejo de estados (idle, loading, result), sección de características. | 6 - 8 hrs |
| `Historial.tsx` | **Media-Alta**. Página de historial de órdenes con autenticación requerida, manejo de estados de carga, tabla de órdenes. | 6 - 8 hrs |
| `NotFound.tsx` | **Baja**. Página 404 simple. | 1 - 1.5 hrs |
| **Subtotal** | | **13 - 17.5 hrs** |

### F. Frontend - Componentes Reutilizables

Componentes UI y de negocio.

| Archivo / Componente | Detalles Técnicos | Horas Est. |
|----------------------|-------------------|------------|
| `VerificationResult.tsx` | **Alta**. Componente complejo con 4 estados (verified, not_found, not_approved, error), integración con MSAL para descarga PDF, UI condicional, mapeo de estados visuales. | 8 - 12 hrs |
| `VerificationForm.tsx` | **Media**. Formulario de verificación con validación, estados de carga. | 4 - 6 hrs |
| `Header.tsx` | **Media**. Header con navegación, botones de login/logout, link a historial condicional. | 3 - 5 hrs |
| `Footer.tsx` | **Baja**. Footer simple. | 1 - 2 hrs |
| Componentes UI (shadcn/ui) | Componentes instalados y configurados (Button, Card, Table, etc.). | 2 - 4 hrs |
| **Subtotal** | | **18 - 29 hrs** |

### G. Frontend - Servicios y Configuración

Servicios para comunicación con backend y configuración de autenticación.

| Archivo / Componente | Detalles Técnicos | Horas Est. |
|----------------------|-------------------|------------|
| `verificationService.ts` | **Media**. Servicio para verificación de órdenes, manejo de errores, tipos TypeScript. | 3 - 4 hrs |
| `pdfService.ts` | **Media-Alta**. Servicio para descarga de PDFs con MSAL, manejo de tokens, descarga de blobs. | 4 - 6 hrs |
| `orderHistoryService.ts` | **Media**. Servicio para historial de órdenes con autenticación. | 3 - 4 hrs |
| `msal.ts` | **Alta**. Configuración completa de MSAL, detección de errores de configuración, helpers para login (Microsoft, Google, Email/Password), inicialización asíncrona. | 6 - 8 hrs |
| `env.ts` | **Media-Alta**. Configuración de variables de entorno con validación, detección de placeholders, fallbacks para desarrollo. | 4 - 6 hrs |
| `App.tsx` | **Media**. Routing, configuración de QueryClient, integración con MSAL Provider. | 3 - 4 hrs |
| `main.tsx` | **Media**. Punto de entrada con inicialización de MSAL antes de render. | 2 - 3 hrs |
| Configuración Vite, TypeScript, ESLint | Configuración del proyecto. | 2 - 3 hrs |
| **Subtotal** | | **27 - 38 hrs** |

### H. Business Central Extension - Tablas

Extensiones de tablas y nuevas tablas en AL.

| Archivo / Componente | Detalles Técnicos | Horas Est. |
|----------------------|-------------------|------------|
| `VendorTableExtension.al` | **Media**. Extensión de tabla Vendor con campo "Vendor ID" (Guid), documentación XML. | 2 - 3 hrs |
| `PortalSetupTable.al` | **Alta**. Tabla singleton con validaciones, métodos públicos (GetPortalURL, IsQRCodeEnabled, GetVerificationURL), documentación XML. | 4 - 6 hrs |
| **Subtotal** | | **6 - 9 hrs** |

### I. Business Central Extension - Páginas

Páginas de configuración y APIs en AL.

| Archivo / Componente | Detalles Técnicos | Horas Est. |
|----------------------|-------------------|------------|
| `PortalSetupPage.al` | **Media-Alta**. Página de configuración con validaciones, acción para probar URL, ejemplo de URL dinámico, documentación XML. | 4 - 6 hrs |
| `PortalVendorsAPI.al` | **Media**. API Page para exponer proveedores con Vendor ID, filtros, documentación XML. | 3 - 4 hrs |
| `PurchaseOrderPdfAPI.al` | **Alta**. API Page para generar PDFs desde reportes BC, conversión a Base64, manejo de Report Selections, documentación XML. | 6 - 8 hrs |
| **Subtotal** | | **13 - 18 hrs** |

### J. Business Central Extension - Codeunits y Reportes

Lógica de negocio y extensión de reportes.

| Archivo / Componente | Detalles Técnicos | Horas Est. |
|----------------------|-------------------|------------|
| `QRCodeGenerator.al` | **Alta**. Codeunit para generación de QR usando servicio HTTP externo (api.qrserver.com), conversión a Base64, documentación XML. | 5 - 7 hrs |
| `PurchaseOrderReportExtension.al` | **Muy Alta**. Extensión del reporte 405, agregado de layout RDLC "Pedido-QR", columnas en dataset (PortalBaseUrl, VerificationUrl, QRCodeBase64), caché de valores, documentación XML. | 8 - 12 hrs |
| `Pedido-QR.rdlc` | **Alta**. Layout RDLC con PageFooter, imagen QR decodificada desde Base64, texto de URL del portal, diseño responsive. | 6 - 8 hrs |
| **Subtotal** | | **19 - 27 hrs** |

### K. Documentación y Configuración

Documentación del proyecto y guías de configuración.

| Componente | Detalles | Horas Est. |
|------------|----------|------------|
| `README.md` (raíz) | Documentación principal del proyecto. | 2 - 3 hrs |
| `bc-extension/README.md` | Documentación completa de la extensión AL, instrucciones de publicación, configuración de QR. | 3 - 4 hrs |
| `backend/B2C_SETUP.md` | Guía detallada de configuración de Azure AD B2C. | 2 - 3 hrs |
| `backend/CONFIGURACION_BC.md` | Guía de configuración de Business Central. | 2 - 3 hrs |
| Documentación XML en código AL | Comentarios XML en todos los objetos AL. | 2 - 3 hrs |
| **Subtotal** | | **11 - 16 hrs** |

---

## 3. Resumen por Categoría

| Categoría | Horas Mín. | Horas Máx. | Costo Mín. (USD) | Costo Máx. (USD) |
|-----------|------------|------------|------------------|------------------|
| Backend - Modelos | 9 | 14.5 | $810 | $1,305 |
| Backend - Servicios | 32.5 | 44.5 | $2,925 | $4,005 |
| Backend - Controladores | 14 | 18 | $1,260 | $1,620 |
| Backend - Configuración | 9 | 14 | $810 | $1,260 |
| Frontend - Páginas | 13 | 17.5 | $1,170 | $1,575 |
| Frontend - Componentes | 18 | 29 | $1,620 | $2,610 |
| Frontend - Servicios/Config | 27 | 38 | $2,430 | $3,420 |
| BC Extension - Tablas | 6 | 9 | $540 | $810 |
| BC Extension - Páginas/APIs | 13 | 18 | $1,170 | $1,620 |
| BC Extension - Codeunits/Reportes | 19 | 27 | $1,710 | $2,430 |
| Documentación | 11 | 16 | $990 | $1,440 |
| **TOTAL** | **171** | **237.5** | **$15,390** | **$21,375** |

**Nota**: El cálculo anterior incluye todas las categorías. El resumen ejecutivo usa un promedio más conservador basado en experiencia real de desarrollo.

---

## 4. Resumen Financiero

La siguiente tabla muestra tres escenarios posibles dependiendo de los imprevistos y la eficiencia del desarrollo.

| Escenario | Horas Totales | Costo Total (USD) | Descripción |
|-----------|---------------|-------------------|-------------|
| **Optimista** | 150 hrs | **$13,500** | Desarrollo fluido, sin errores mayores, requisitos claros desde el inicio, reutilización de componentes. |
| **Realista** | **180 hrs** | **$16,200** | Incluye tiempo para depuración normal, ajustes de lógica, pruebas de integración, debugging de APIs externas. |
| **Conservador** | 220 hrs | **$19,800** | Contempla alta complejidad en pruebas, cambios de alcance, debugging profundo de integraciones Azure y BC, refactorizaciones. |

---

## 5. Funcionalidades Principales Valoradas

### 5.1 Verificación Pública de Órdenes
- Verificación sin autenticación mediante código de orden o QR
- Búsqueda en órdenes activas y archivadas en Business Central
- Validación de estado de orden (solo órdenes aprobadas)
- Mapeo de estados de BC a español
- Interfaz moderna y responsive

### 5.2 Integración con Business Central
- **OAuth2 Client Credentials** para autenticación segura
- Búsqueda en múltiples entornos (Production, Sandbox, etc.)
- APIs REST personalizadas de Melcon (`/api/melcon/purchasing/v2.0/`)
- Búsqueda en Purchase Header y Purchase Header Archive
- Manejo robusto de errores (NoEnvironment, Unauthorized, etc.)
- Logging detallado para debugging

### 5.3 Sistema de Autenticación y Autorización
- **Integración con Azure AD B2C** (Entra External ID) para proveedores
- Login con Microsoft, Google, o Email/Password
- Resolución de Vendor No. desde claims de B2C
- Autorización por proveedor (solo pueden ver sus propias órdenes)
- Endpoints protegidos y públicos diferenciados

### 5.4 Descarga de PDFs
- Generación de PDFs desde Business Central usando reportes configurados
- Descarga segura mediante autenticación
- Validación de propiedad de orden por proveedor
- Mismo formato que los PDFs generados en BC

### 5.5 Historial de Órdenes
- Vista de historial para proveedores autenticados
- Filtrado por proveedor automático
- Tabla con información relevante (número, fecha, estado, total)

### 5.6 Generación de Códigos QR en Business Central
- Extensión de reporte de Purchase Order con layout "Pedido-QR"
- Generación automática de QR con URL del portal
- Integración con servicio externo (api.qrserver.com)
- Configuración mediante Portal Setup en BC
- QR en footer del reporte con texto informativo

### 5.7 Frontend React + TypeScript
- **SPA moderna** con React 18 + Vite
- **shadcn/ui** para componentes UI consistentes
- **MSAL React** para autenticación
- Routing con React Router
- Lectura automática de parámetros URL (para escaneo QR)
- Manejo de estados de carga y errores

### 5.8 Extensión AL para Business Central
- Tabla de configuración (Portal Setup)
- Extensión de tabla Vendor con Vendor ID
- APIs personalizadas (Portal Vendors, Purchase Order PDF)
- Codeunit para generación de QR
- Extensión de reporte con layout RDLC personalizado
- Documentación XML completa

---

## 6. Consideraciones Adicionales

Para entender el valor real de la inversión, considere los siguientes factores técnicos que elevan la complejidad (y el costo) de este desarrollo:

1. **Integración Multi-Cloud**: La solución integra Business Central, Azure AD B2C, y servicios externos (api.qrserver.com), requiriendo conocimiento experto en cada plataforma.

2. **OAuth2 y Autenticación**: La integración con Azure AD B2C y Business Central requiere manejo de tokens, refresh tokens, y manejo de errores específicos de Microsoft.

3. **Búsqueda en Múltiples Fuentes**: El sistema busca órdenes en Purchase Header (activas) y Purchase Header Archive (archivadas), requiriendo lógica de conversión y manejo de versiones.

4. **Generación de QR en BC**: La extensión de reportes en AL requiere conocimiento profundo de RDLC, generación de imágenes desde Base64, y configuración de layouts personalizados.

5. **Manejo de Estados de Orden**: Validación compleja de estados (Open, Pending approval, Released, etc.) con mapeo a español y lógica de negocio.

6. **Seguridad y Autorización**: Implementación de políticas de autorización granulares, validación de roles en frontend y backend, y protección de endpoints sensibles.

7. **Manejo de Archivos**: Generación y descarga de PDFs desde Business Central, conversión a Base64, y streaming al frontend.

8. **Testing y Debugging**: La complejidad de las integraciones requiere tiempo significativo para pruebas de integración y debugging de APIs externas.

9. **Documentación**: Documentación completa de APIs, READMEs, guías de configuración, y análisis de costos (este documento).

---

## 7. Costos Externos (No incluidos)

| Concepto | Costo Estimado |
|----------|----------------|
| Licencia Azure AD B2C | ~$0.00325 USD por autenticación (primeros 50k/mes gratis) |
| Azure App Service (Hosting) | ~$55-150 USD/mes según tier |
| Business Central License | Variable según plan |
| Dominio y SSL | ~$15-50 USD/año |
| Servicio QR (api.qrserver.com) | Gratuito (sin límites conocidos) |
| Soporte post-implementación | A cotizar por separado |
| Capacitación de usuarios | A cotizar por separado |

---

## 8. Conclusión

El costo sugerido para este proyecto se sitúa en el rango de **$15,000 - $17,000 USD**. Este presupuesto asegura:

- ✅ Integración funcional con Business Central (OAuth2, APIs REST personalizadas)
- ✅ Sistema de autenticación con Azure AD B2C
- ✅ Verificación pública de órdenes sin login
- ✅ Descarga de PDFs para proveedores autenticados
- ✅ Historial de órdenes por proveedor
- ✅ Generación de códigos QR en Business Central
- ✅ Frontend moderno con React + TypeScript
- ✅ Extensión AL completa para BC
- ✅ Depuración y pruebas de integración estándar
- ✅ Documentación completa

**Nota**: No incluye soporte post go-live, capacitación de usuarios, cambios de alcance significativos, ni costos de infraestructura Azure.

---

## 9. Desglose por Stack Tecnológico

### Backend (.NET 8)
| Componente | Horas | Costo (USD) |
|------------|-------|-------------|
| Modelos | 9-14.5 | $810 - $1,305 |
| Servicios | 32.5-44.5 | $2,925 - $4,005 |
| Controladores | 14-18 | $1,260 - $1,620 |
| Configuración | 9-14 | $810 - $1,260 |
| **Subtotal Backend** | **64.5-91** | **$5,805 - $8,190** |

### Frontend (React + TypeScript)
| Componente | Horas | Costo (USD) |
|------------|-------|-------------|
| Páginas | 13-17.5 | $1,170 - $1,575 |
| Componentes | 18-29 | $1,620 - $2,610 |
| Servicios/Config | 27-38 | $2,430 - $3,420 |
| **Subtotal Frontend** | **58-84.5** | **$5,220 - $7,605** |

### Business Central Extension (AL)
| Componente | Horas | Costo (USD) |
|------------|-------|-------------|
| Tablas | 6-9 | $540 - $810 |
| Páginas/APIs | 13-18 | $1,170 - $1,620 |
| Codeunits/Reportes | 19-27 | $1,710 - $2,430 |
| **Subtotal BC Extension** | **38-54** | **$3,420 - $4,860** |

---

## 10. Comparación con Proyectos Similares

Para contextualizar el costo, esta plataforma incluye:

- **Backend completo** con integración Business Central (~65-91 hrs)
- **Frontend SPA moderno** con React (~58-85 hrs)
- **Extensión AL completa** para Business Central (~38-54 hrs)
- **2 integraciones principales**: Business Central (OAuth2), Azure AD B2C
- **Generación de QR** en reportes de BC
- **Sistema de autenticación** con autorización por proveedor

Comparado con proyectos similares:
- Portal de verificación básico: ~$8,000-10,000 USD
- Portal con integración BC: ~$12,000-15,000 USD
- **Esta plataforma (completa)**: **$15,000-17,000 USD** ✅

El valor adicional proviene de la integración robusta con Business Central (búsqueda en activas y archivadas), generación de QR en BC, sistema de autenticación B2C, y extensión AL completa con documentación.

---

## 11. Historial de Versiones del Documento

| Versión | Fecha | Cambios |
|---------|-------|---------|
| 1.0 | 2025-01-02 | Análisis inicial completo. Incluye backend .NET 8, frontend React, extensión AL para BC, integraciones con Business Central y Azure AD B2C, generación de QR. |

---

**Documento generado el:** 2 de Enero, 2025  
**Última actualización:** 2 de Enero, 2025



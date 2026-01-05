## Monorepo: Verificador de Órdenes

Portal de verificación de órdenes de compra para proveedores con integración a Business Central.

### Funcionalidades

- ✅ **Verificación pública**: Cualquiera puede verificar una orden de compra sin login
- 🔐 **Descarga de PDF**: Los proveedores autenticados pueden descargar el PDF de sus órdenes (mismo formato que BC)
- 📋 **Historial**: Los proveedores autenticados pueden ver su historial de órdenes
- 📱 **Códigos QR**: Los pedidos de compra en BC pueden incluir códigos QR que apuntan al portal de verificación

### Arquitectura

- `backend/` ASP.NET Core Web API (puerto 9000)
- `frontend/` React + Vite (puerto 8081 por defecto)
- `bc-extension/` Archivos AL para Business Central (publicar en BC)

### Requisitos
- .NET 8 SDK
- Node.js 18+ y npm

### Comandos (desde la raíz)

```bash
npm run dev         # Levanta backend y frontend en paralelo
npm run dev:backend # Solo backend
npm run dev:frontend# Solo frontend
npm run build       # Build de ambos
```

### Configuración

#### Backend (Business Central)
📖 **Guía completa**: Ver [`backend/CONFIGURACION_BC.md`](backend/CONFIGURACION_BC.md)

Pasos rápidos:
1. Registrar aplicación en Azure AD
2. Crear Client Secret
3. Configurar permisos de API (Dynamics 365 Business Central)
4. Obtener Company ID
5. Editar `backend/appsettings.Development.json` con las credenciales

#### Frontend
- Copiar `frontend/.env.example` a `frontend/.env`
- Ajustar `VITE_API_URL` si es necesario (por defecto: `http://localhost:9000`)
- (Opcional) Configurar Azure AD B2C para habilitar login y descarga de PDFs - ver [`backend/B2C_SETUP.md`](backend/B2C_SETUP.md)

#### Business Central Extension
- Publicar los archivos AL de `bc-extension/` en tu entorno de Business Central
- Ver [`bc-extension/README.md`](bc-extension/README.md) para instrucciones

### Flujo
1. `npm run dev`
2. Abrir frontend en `http://localhost:5173`
3. Verifica contra API en `http://localhost:9000`



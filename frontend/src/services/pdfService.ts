import { msalInstance } from "@/lib/msal";
import { env } from "@/config/env";
import { AccountInfo, InteractionRequiredAuthError } from "@azure/msal-browser";

const API_BASE_URL = env.apiBaseUrl || "http://localhost:9000";

/**
 * Obtiene el token de acceso para llamar al backend
 */
async function getAccessToken(): Promise<string | null> {
  if (!msalInstance) {
    return null;
  }

  try {
    const accounts = msalInstance.getAllAccounts();
    if (accounts.length === 0) {
      return null;
    }

    const account = accounts[0];
    const request = {
      scopes: env.b2cScopes && env.b2cScopes.length > 0 
        ? env.b2cScopes 
        : [`${env.b2cClientId}/.default`],
      account: account as AccountInfo
    };

    const response = await msalInstance.acquireTokenSilent(request);
    return response.accessToken;
  } catch (error) {
    if (error instanceof InteractionRequiredAuthError) {
      // Redirigir al login si es necesario
      await msalInstance.loginRedirect({
        scopes: env.b2cScopes && env.b2cScopes.length > 0 
          ? env.b2cScopes 
          : [`${env.b2cClientId}/.default`]
      });
    }
    return null;
  }
}

/**
 * Descarga el PDF de una orden de compra
 */
export async function downloadPurchaseOrderPdf(orderCode: string): Promise<void> {
  const token = await getAccessToken();
  
  if (!token) {
    throw new Error("No se pudo obtener el token de acceso. Por favor, inicia sesión.");
  }

  try {
    const response = await fetch(`${API_BASE_URL}/api/verify-order/${encodeURIComponent(orderCode)}/pdf`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`
      }
    });

    if (!response.ok) {
      if (response.status === 401 || response.status === 403) {
        throw new Error("No tienes permiso para descargar esta orden");
      }
      throw new Error(`Error al descargar PDF: ${response.status}`);
    }

    const blob = await response.blob();
    const url = window.URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `Pedido_${orderCode}.pdf`;
    document.body.appendChild(a);
    a.click();
    window.URL.revokeObjectURL(url);
    document.body.removeChild(a);
  } catch (error) {
    console.error("Error al descargar PDF:", error);
    throw error;
  }
}


import { msalInstance } from "@/lib/msal";
import { env } from "@/config/env";
import { AccountInfo, InteractionRequiredAuthError } from "@azure/msal-browser";

const API_BASE_URL = env.apiBaseUrl || "http://localhost:9000";

export interface OrderHistoryItem {
  number: string;
  date: string;
  vendor: string;
  total: number;
  status: string;
}

export interface OrderHistoryResponse {
  vendorNo: string;
  count: number;
  orders: OrderHistoryItem[];
}

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
 * Obtiene el historial de órdenes del proveedor autenticado
 */
export async function getMyOrders(top: number = 50): Promise<OrderHistoryResponse> {
  const token = await getAccessToken();
  
  if (!token) {
    throw new Error("No se pudo obtener el token de acceso. Por favor, inicia sesión.");
  }

  try {
    const response = await fetch(`${API_BASE_URL}/api/verify-order/vendor/orders?top=${top}`, {
      method: 'GET',
      headers: {
        'Authorization': `Bearer ${token}`,
        'Content-Type': 'application/json'
      }
    });

    if (!response.ok) {
      if (response.status === 401 || response.status === 403) {
        throw new Error("No tienes permiso para ver el historial");
      }
      throw new Error(`Error al obtener historial: ${response.status}`);
    }

    const data: OrderHistoryResponse = await response.json();
    return data;
  } catch (error) {
    console.error("Error al obtener historial:", error);
    throw error;
  }
}


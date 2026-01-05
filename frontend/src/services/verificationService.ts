import type { OrderDetails } from "@/components/VerificationResult";

// API Configuration - URL del backend .NET Core
// Para desarrollo: http://localhost:9000
// Para producción: configurar VITE_API_URL en archivo .env
const API_BASE_URL = import.meta.env.VITE_API_URL || "http://localhost:9000";

export interface VerificationResponse {
  success: boolean;
  status: "verified" | "not_found" | "not_approved" | "error";
  data?: OrderDetails;
  message?: string;
}

/**
 * Verifies a purchase order against the Business Central API via backend
 * @param orderCode - The order code from QR or manual input
 */
export async function verifyPurchaseOrder(orderCode: string): Promise<VerificationResponse> {
  try {
    // Llamada real al backend .NET Core que conecta con Business Central
    const response = await fetch(`${API_BASE_URL}/api/verify-order/${encodeURIComponent(orderCode)}`, {
      method: 'GET',
    });

    if (!response.ok) {
      // Intentar parsear el body de error
      let errorData: VerificationResponse | null = null;
      try {
        errorData = await response.json();
      } catch {
        // Si no se puede parsear, usar error genérico
      }

      if (response.status === 404) {
        return errorData || { success: false, status: "not_found" };
      }
      
      return errorData || {
        success: false,
        status: "error",
        message: `Error del servidor: ${response.status}`,
      };
    }

    // El backend devuelve la estructura exacta que espera el frontend
    const result: VerificationResponse = await response.json();
    return result;
    
  } catch (error) {
    console.error("Verification error:", error);
    
    // Solo retornar error real, sin fallback a demo
    return {
      success: false,
      status: "error",
      message: error instanceof Error 
        ? `Error de conexión: ${error.message}`
        : "Error al conectar con el servidor de verificación",
    };
  }
}


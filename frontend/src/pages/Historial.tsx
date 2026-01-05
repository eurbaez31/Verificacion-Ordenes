import { useEffect, useState } from "react";
import { useMsal } from "@azure/msal-react";
import { useNavigate } from "react-router-dom";
import { getMyOrders, type OrderHistoryItem } from "@/services/orderHistoryService";
import { loginWithMicrosoft } from "@/lib/msal";
import { msalInstance, isAuthEnabled } from "@/lib/msal";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { FileText, Download, LogIn, Loader2 } from "lucide-react";
import { downloadPurchaseOrderPdf } from "@/services/pdfService";

const Historial = () => {
  const { instance, accounts } = useMsal();
  const navigate = useNavigate();
  const [orders, setOrders] = useState<OrderHistoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [downloadingOrder, setDownloadingOrder] = useState<string | null>(null);
  const isAuthenticated = accounts.length > 0;

  useEffect(() => {
    if (!isAuthEnabled) {
      setError("La autenticación no está configurada");
      setLoading(false);
      return;
    }

    if (!isAuthenticated) {
      setLoading(false);
      return;
    }

    loadOrders();
  }, [isAuthenticated]);

  const loadOrders = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await getMyOrders(50);
      setOrders(response.orders);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Error al cargar el historial");
    } finally {
      setLoading(false);
    }
  };

  const handleLogin = async () => {
    if (msalInstance) {
      await loginWithMicrosoft(msalInstance);
    }
  };

  const handleDownloadPdf = async (orderNumber: string) => {
    setDownloadingOrder(orderNumber);
    try {
      await downloadPurchaseOrderPdf(orderNumber);
    } catch (err) {
      alert(err instanceof Error ? err.message : "Error al descargar el PDF");
    } finally {
      setDownloadingOrder(null);
    }
  };

  if (!isAuthEnabled) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle>Autenticación no disponible</CardTitle>
            <CardDescription>
              La funcionalidad de historial requiere autenticación, pero no está configurada.
            </CardDescription>
          </CardHeader>
        </Card>
      </div>
    );
  }

  if (!isAuthenticated) {
    return (
      <div className="min-h-screen flex items-center justify-center p-4">
        <Card className="w-full max-w-md">
          <CardHeader>
            <CardTitle>Iniciar Sesión</CardTitle>
            <CardDescription>
              Necesitas iniciar sesión para ver tu historial de órdenes de compra.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button onClick={handleLogin} className="w-full" size="lg">
              <LogIn className="w-4 h-4 mr-2" />
              Iniciar Sesión
            </Button>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen p-4">
      <div className="max-w-4xl mx-auto">
        <div className="mb-6">
          <h1 className="text-3xl font-bold mb-2">Historial de Órdenes</h1>
          <p className="text-muted-foreground">
            Aquí puedes ver todas tus órdenes de compra y descargar los PDFs.
          </p>
        </div>

        {loading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="w-8 h-8 animate-spin text-primary" />
          </div>
        ) : error ? (
          <Card>
            <CardContent className="pt-6">
              <p className="text-destructive">{error}</p>
              <Button onClick={loadOrders} className="mt-4" variant="outline">
                Reintentar
              </Button>
            </CardContent>
          </Card>
        ) : orders.length === 0 ? (
          <Card>
            <CardContent className="pt-6">
              <p className="text-muted-foreground text-center py-8">
                No tienes órdenes de compra registradas.
              </p>
            </CardContent>
          </Card>
        ) : (
          <div className="space-y-4">
            {orders.map((order) => (
              <Card key={order.number}>
                <CardContent className="pt-6">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <div className="flex items-center gap-2 mb-2">
                        <FileText className="w-5 h-5 text-primary" />
                        <h3 className="font-semibold text-lg">{order.number}</h3>
                      </div>
                      <div className="space-y-1 text-sm text-muted-foreground">
                        <p>Fecha: {order.date}</p>
                        <p>Proveedor: {order.vendor}</p>
                        <p>Total: ${order.total.toLocaleString()}</p>
                        <p>Estado: {order.status}</p>
                      </div>
                    </div>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => handleDownloadPdf(order.number)}
                      disabled={downloadingOrder === order.number}
                    >
                      {downloadingOrder === order.number ? (
                        <Loader2 className="w-4 h-4 animate-spin" />
                      ) : (
                        <>
                          <Download className="w-4 h-4 mr-2" />
                          PDF
                        </>
                      )}
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default Historial;


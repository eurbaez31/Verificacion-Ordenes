import { CheckCircle2, XCircle, AlertCircle, ArrowLeft, FileText, Calendar, Building2, Package, DollarSign, Download } from "lucide-react";
import { Button } from "@/components/ui/button";
import { useMsal } from "@azure/msal-react";
import { downloadPurchaseOrderPdf } from "@/services/pdfService";
import { useState } from "react";
import { isAuthEnabled } from "@/lib/msal";

export interface OrderDetails {
  orderNumber: string;
  date: string;
  vendor: string;
  department: string;
  items: Array<{
    description: string;
    quantity: number;
    unitPrice: number;
  }>;
  total: number;
  status: string;
  approvedBy: string;
}

interface VerificationResultProps {
  status: "verified" | "not_found" | "not_approved" | "error";
  orderDetails?: OrderDetails;
  errorMessage?: string;
  onReset: () => void;
}

const VerificationResult = ({ status, orderDetails, errorMessage, onReset }: VerificationResultProps) => {
  const { instance, accounts } = useMsal();
  const [isDownloading, setIsDownloading] = useState(false);
  const isAuthenticated = accounts.length > 0;
  const authAvailable = isAuthEnabled && instance;

  const handleDownloadPdf = async () => {
    if (!orderDetails || !authAvailable) return;

    if (!isAuthenticated) {
      // Redirigir al login
      await instance!.loginRedirect({
        scopes: []
      });
      return;
    }

    setIsDownloading(true);
    try {
      await downloadPurchaseOrderPdf(orderDetails.orderNumber);
    } catch (error) {
      console.error("Error al descargar PDF:", error);
      alert(error instanceof Error ? error.message : "Error al descargar el PDF");
    } finally {
      setIsDownloading(false);
    }
  };

  const statusConfig = {
    verified: {
      icon: CheckCircle2,
      title: "Orden Verificada",
      subtitle: "Esta orden de compra es auténtica y válida",
      bgClass: "bg-success-bg",
      iconClass: "text-success",
      borderClass: "border-success/20",
    },
    not_found: {
      icon: XCircle,
      title: "Orden No Encontrada",
      subtitle: "No se encontró ninguna orden con este código",
      bgClass: "bg-destructive/10",
      iconClass: "text-destructive",
      borderClass: "border-destructive/20",
    },
    not_approved: {
      icon: AlertCircle,
      title: "Orden No Aprobada",
      subtitle: errorMessage || "Esta orden aún no ha sido aprobada. Solo se pueden verificar órdenes que hayan sido aprobadas.",
      bgClass: "bg-warning/10",
      iconClass: "text-warning",
      borderClass: "border-warning/20",
    },
    error: {
      icon: AlertCircle,
      title: "Error de Verificación",
      subtitle: errorMessage || "Ocurrió un error al verificar la orden",
      bgClass: "bg-warning/10",
      iconClass: "text-warning",
      borderClass: "border-warning/20",
    },
  };

  const config = statusConfig[status];
  const Icon = config.icon;

  return (
    <div className="w-full max-w-lg mx-auto animate-scale-in">
      <div className="glass-card rounded-2xl overflow-hidden shadow-card">
        {/* Status Header */}
        <div className={`p-6 ${config.bgClass} border-b ${config.borderClass}`}>
          <div className="flex items-center gap-4">
            <div className={`w-14 h-14 rounded-xl bg-card flex items-center justify-center shadow-sm`}>
              <Icon className={`w-7 h-7 ${config.iconClass}`} />
            </div>
            <div>
              <h2 className="font-display text-xl font-bold text-foreground">
                {config.title}
              </h2>
              <p className="text-sm text-muted-foreground">
                {config.subtitle}
              </p>
            </div>
          </div>
        </div>

        {/* Order Details */}
        {status === "verified" && orderDetails && (
          <div className="p-6 space-y-6">
            {/* Order Info Grid */}
            <div className="grid grid-cols-2 gap-4">
              <div className="space-y-1">
                <div className="flex items-center gap-2 text-muted-foreground text-xs">
                  <FileText className="w-3.5 h-3.5" />
                  Número de Orden
                </div>
                <p className="font-semibold text-foreground">{orderDetails.orderNumber}</p>
              </div>
              <div className="space-y-1">
                <div className="flex items-center gap-2 text-muted-foreground text-xs">
                  <Calendar className="w-3.5 h-3.5" />
                  Fecha
                </div>
                <p className="font-semibold text-foreground">{orderDetails.date}</p>
              </div>
              <div className="space-y-1">
                <div className="flex items-center gap-2 text-muted-foreground text-xs">
                  <Building2 className="w-3.5 h-3.5" />
                  Departamento
                </div>
                <p className="font-semibold text-foreground">{orderDetails.department}</p>
              </div>
              <div className="space-y-1">
                <div className="flex items-center gap-2 text-muted-foreground text-xs">
                  <Package className="w-3.5 h-3.5" />
                  Estado
                </div>
                <p className="font-semibold text-success">{orderDetails.status}</p>
              </div>
            </div>

            {/* Proveedor (reemplaza a Artículos) */}
            <div className="space-y-1">
              <div className="flex items-center gap-2 text-muted-foreground text-xs">
                <Building2 className="w-3.5 h-3.5" />
                Proveedor
              </div>
              <p className="font-semibold text-foreground">{orderDetails.vendor}</p>
            </div>

            {/* Total */}
            <div className="flex justify-between items-center p-4 bg-primary/5 rounded-xl border border-primary/10">
              <div className="flex items-center gap-2">
                <DollarSign className="w-5 h-5 text-primary" />
                <span className="font-semibold text-foreground">Total</span>
              </div>
              <span className="text-xl font-bold text-primary">
                ${orderDetails.total.toLocaleString()}
              </span>
            </div>

            {/* Approved By */}
            <div className="text-center text-xs text-muted-foreground border-t border-border pt-4">
              Aprobado por: <span className="font-medium text-foreground">{orderDetails.approvedBy}</span>
            </div>
          </div>
        )}

        {/* Error/Not Found Message */}
        {status !== "verified" && (
          <div className="p-6 text-center">
            <p className="text-muted-foreground text-sm mb-4">
              {status === "not_found"
                ? "Verifique que el código ingresado sea correcto e intente nuevamente."
                : "Por favor intente nuevamente más tarde o contacte a soporte técnico."}
            </p>
          </div>
        )}

        {/* Actions */}
        <div className="p-6 pt-0 space-y-3">
          {status === "verified" && authAvailable && (
            <Button
              variant="default"
              size="lg"
              className="w-full"
              onClick={handleDownloadPdf}
              disabled={isDownloading}
            >
              <Download className="w-4 h-4 mr-2" />
              {isDownloading ? "Descargando..." : isAuthenticated ? "Descargar PDF" : "Iniciar Sesión para Descargar PDF"}
            </Button>
          )}
          <Button
            variant="outline"
            size="lg"
            className="w-full"
            onClick={onReset}
          >
            <ArrowLeft className="w-4 h-4" />
            Verificar Otra Orden
          </Button>
        </div>
      </div>
    </div>
  );
};

export default VerificationResult;

import { useState, useEffect } from "react";
import { useSearchParams } from "react-router-dom";
import Header from "@/components/Header";
import Footer from "@/components/Footer";
import VerificationForm from "@/components/VerificationForm";
import VerificationResult, { type OrderDetails } from "@/components/VerificationResult";
import { verifyPurchaseOrder } from "@/services/verificationService";
import { Shield, CheckCircle, Clock, FileCheck } from "lucide-react";

type VerificationState = "idle" | "loading" | "result";
type ResultStatus = "verified" | "not_found" | "not_approved" | "error";

const Index = () => {
  const [searchParams, setSearchParams] = useSearchParams();
  const [state, setState] = useState<VerificationState>("idle");
  const [resultStatus, setResultStatus] = useState<ResultStatus>("verified");
  const [orderDetails, setOrderDetails] = useState<OrderDetails | undefined>();
  const [errorMessage, setErrorMessage] = useState<string | undefined>();

  // Leer código de la URL al cargar (para escaneo QR)
  useEffect(() => {
    const codeFromUrl = searchParams.get("code");
    if (codeFromUrl && state === "idle") {
      // Auto-verificar si hay código en la URL
      handleVerify(codeFromUrl);
      // Limpiar el parámetro de la URL después de leerlo
      setSearchParams({}, { replace: true });
    }
  }, [searchParams, state]);

  const handleVerify = async (code: string) => {
    setState("loading");
    
    const response = await verifyPurchaseOrder(code);
    
    setResultStatus(response.status);
    setOrderDetails(response.data);
    setErrorMessage(response.message);
    setState("result");
  };

  const handleReset = () => {
    setState("idle");
    setOrderDetails(undefined);
    setErrorMessage(undefined);
  };

  const features = [
    {
      icon: Shield,
      title: "Verificación Segura",
      description: "Conexión directa con Business Central",
    },
    {
      icon: CheckCircle,
      title: "Validación Instantánea",
      description: "Confirmación en tiempo real",
    },
    {
      icon: Clock,
      title: "Disponible 24/7",
      description: "Acceso en cualquier momento",
    },
    {
      icon: FileCheck,
      title: "Detalles Completos",
      description: "Información detallada de la orden",
    },
  ];

  return (
    <div className="min-h-screen flex flex-col bg-background">
      <Header />
      
      <main className="flex-1 pt-24 pb-12 px-4">
        <div className="max-w-7xl mx-auto">
          {/* Hero Section */}
          <div className="text-center mb-12 animate-fade-in">
            <div className="inline-flex items-center gap-2 px-4 py-2 rounded-full bg-accent border border-border/50 text-sm text-muted-foreground mb-6">
              <Shield className="w-4 h-4 text-primary" />
              Portal de Verificación Melcon
            </div>
            <h1 className="font-display text-4xl sm:text-5xl font-bold text-foreground mb-4">
              Verifique sus{" "}
              <span className="gradient-text">Órdenes de Compra</span>
            </h1>
            <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
              Confirme la autenticidad de las órdenes de compra emitidas por nuestra empresa
              mediante el código QR o el número de orden.
            </p>
          </div>

          {/* Main Content */}
          <div className="mb-16">
            {state === "idle" || state === "loading" ? (
              <VerificationForm 
                onVerify={handleVerify} 
                isLoading={state === "loading"} 
              />
            ) : (
              <VerificationResult
                status={resultStatus}
                orderDetails={orderDetails}
                errorMessage={errorMessage}
                onReset={handleReset}
              />
            )}
          </div>

          {/* Features Section */}
          {state === "idle" && (
            <div className="grid grid-cols-2 md:grid-cols-4 gap-4 animate-slide-up" style={{ animationDelay: "0.2s" }}>
              {features.map((feature, index) => (
                <div
                  key={index}
                  className="glass-card rounded-xl p-4 text-center transition-all duration-300 hover:shadow-lg hover:-translate-y-1"
                >
                  <div className="w-10 h-10 mx-auto mb-3 rounded-lg bg-accent flex items-center justify-center">
                    <feature.icon className="w-5 h-5 text-primary" />
                  </div>
                  <h3 className="font-semibold text-sm text-foreground mb-1">
                    {feature.title}
                  </h3>
                  <p className="text-xs text-muted-foreground">
                    {feature.description}
                  </p>
                </div>
              ))}
            </div>
          )}
        </div>
      </main>

      <Footer />
    </div>
  );
};

export default Index;

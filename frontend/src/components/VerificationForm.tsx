import { useState } from "react";
import { QrCode, Search, Loader2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";

interface VerificationFormProps {
  onVerify: (code: string) => void;
  isLoading: boolean;
}

const VerificationForm = ({ onVerify, isLoading }: VerificationFormProps) => {
  const [code, setCode] = useState("");

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (code.trim()) {
      onVerify(code.trim());
    }
  };

  return (
    <div className="w-full max-w-md mx-auto animate-slide-up">
      <div className="glass-card rounded-2xl p-8 shadow-card">
        <div className="text-center mb-8">
          <div className="w-16 h-16 mx-auto mb-4 rounded-2xl bg-accent flex items-center justify-center">
            <QrCode className="w-8 h-8 text-primary" />
          </div>
          <h2 className="font-display text-2xl font-bold text-foreground mb-2">
            Verificar Orden de Compra
          </h2>
          <p className="text-muted-foreground text-sm">
            Ingrese el código de la orden o escanee el código QR
          </p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-4">
          <div className="space-y-2">
            <label htmlFor="orderCode" className="text-sm font-medium text-foreground">
              Código de Orden
            </label>
            <Input
              id="orderCode"
              type="text"
              placeholder="Ej: PO-2024-001234"
              value={code}
              onChange={(e) => setCode(e.target.value)}
              className="h-12 text-base input-focus-ring"
              disabled={isLoading}
            />
          </div>

          <Button
            type="submit"
            variant="gradient"
            size="lg"
            className="w-full"
            disabled={!code.trim() || isLoading}
          >
            {isLoading ? (
              <>
                <Loader2 className="w-5 h-5 animate-spin" />
                Verificando...
              </>
            ) : (
              <>
                <Search className="w-5 h-5" />
                Verificar Orden
              </>
            )}
          </Button>
        </form>

        <div className="mt-6 pt-6 border-t border-border">
          <p className="text-xs text-center text-muted-foreground">
            Este sistema verifica la autenticidad de las órdenes de compra
            emitidas por la empresa.
          </p>
        </div>
      </div>
    </div>
  );
};

export default VerificationForm;

import { Shield, Lock } from "lucide-react";

const Footer = () => {
  return (
    <footer className="w-full py-6 px-6 border-t border-border/50 bg-card/30 backdrop-blur-sm mt-auto">
      <div className="max-w-7xl mx-auto">
        <div className="flex flex-col sm:flex-row items-center justify-between gap-4">
          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Lock className="w-4 h-4" />
            <span>Conexión segura</span>
          </div>
          
          <div className="flex items-center gap-4 text-xs text-muted-foreground">
            <span>© 2026 VerifyPO</span>
            <span className="hidden sm:inline">•</span>
            <span className="hidden sm:inline">Portal de Verificación Melcon</span>
          </div>

          <div className="flex items-center gap-2 text-sm text-muted-foreground">
            <Shield className="w-4 h-4" />
            <span>Business Central</span>
          </div>
        </div>
      </div>
    </footer>
  );
};

export default Footer;

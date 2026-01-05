import { Shield, History, LogIn, LogOut } from "lucide-react";
import { Link } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { isAuthEnabled, msalInstance } from "@/lib/msal";
import { Button } from "@/components/ui/button";

const Header = () => {
  const { instance, accounts } = useMsal();
  const isAuthenticated = accounts.length > 0;
  const authAvailable = isAuthEnabled && instance;

  const handleLogout = async () => {
    if (instance) {
      await instance.logoutRedirect();
    }
  };

  const handleLogin = async () => {
    if (instance) {
      await instance.loginRedirect({
        scopes: []
      });
    }
  };

  return (
    <header className="w-full py-4 px-6 border-b border-border/50 bg-card/50 backdrop-blur-md fixed top-0 z-50">
      <div className="max-w-7xl mx-auto flex items-center justify-between">
        <Link to="/" className="flex items-center gap-3">
          <div className="w-10 h-10 rounded-xl bg-primary flex items-center justify-center shadow-md">
            <Shield className="w-5 h-5 text-primary-foreground" />
          </div>
          <div>
            <h1 className="font-display font-bold text-lg text-foreground">
              VerifyPO
            </h1>
            <p className="text-xs text-muted-foreground">
              Verificación de Órdenes
            </p>
          </div>
        </Link>
        <div className="flex items-center gap-3">
          {authAvailable && (
            <>
              {isAuthenticated && (
                <Link to="/historial">
                  <Button variant="ghost" size="sm">
                    <History className="w-4 h-4 mr-2" />
                    Historial
                  </Button>
                </Link>
              )}
              {isAuthenticated ? (
                <Button variant="ghost" size="sm" onClick={handleLogout}>
                  <LogOut className="w-4 h-4 mr-2" />
                  Cerrar Sesión
                </Button>
              ) : (
                <Button variant="ghost" size="sm" onClick={handleLogin}>
                  <LogIn className="w-4 h-4 mr-2" />
                  Iniciar Sesión
                </Button>
              )}
            </>
          )}
          <span className="text-xs text-muted-foreground hidden sm:block">
            Portal de Proveedores
          </span>
          <div className="w-2 h-2 rounded-full bg-success animate-pulse" />
        </div>
      </div>
    </header>
  );
};

export default Header;

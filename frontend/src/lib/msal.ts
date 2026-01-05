import { PublicClientApplication, type Configuration, AuthError } from "@azure/msal-browser";
import { env } from "@/config/env";

export const isAuthEnabled = !env.authDisabled && !!env.b2cClientId && !!env.b2cAuthority;

// Función helper para detectar errores de configuración
export const isConfigurationError = (error: unknown): boolean => {
  if (error instanceof AuthError) {
    const errorCode = error.errorCode;
    return (
      errorCode === "endpoints_resolution_error" ||
      errorCode === "invalid_configuration" ||
      error.message?.includes("404") ||
      error.message?.includes("Not Found")
    );
  }
  
  if (error instanceof Error) {
    return (
      error.message.includes("404") ||
      error.message.includes("Not Found") ||
      error.message.includes("endpoints_resolution_error") ||
      error.message.includes("tu-tenant")
    );
  }
  
  return false;
};

// Función helper para obtener mensaje de error amigable
export const getFriendlyErrorMessage = (error: unknown): string => {
  if (isConfigurationError(error)) {
    return (
      "Error de configuración de Azure AD B2C. " +
      "Por favor, verifica que las variables de entorno en el archivo .env contengan los valores correctos de tu tenant."
    );
  }
  
  if (error instanceof Error) {
    return error.message;
  }
  
  return "Ocurrió un error inesperado al intentar iniciar sesión. Por favor, intenta nuevamente.";
};

const resolveKnownAuthorities = () => {
  if (env.b2cKnownAuthorities && env.b2cKnownAuthorities.length > 0) {
    return env.b2cKnownAuthorities;
  }

  try {
    const parsedUrl = new URL(env.b2cAuthority!);
    return [parsedUrl.hostname];
  } catch {
    return [];
  }
};

let msalInstance: PublicClientApplication | null = null;
let msalInitialization: Promise<void>;

if (isAuthEnabled) {
  const msalConfig: Configuration = {
    auth: {
      clientId: env.b2cClientId!,
      authority: env.b2cAuthority!,
      knownAuthorities: resolveKnownAuthorities(),
      redirectUri: env.b2cRedirectUri || window.location.origin
    },
    cache: {
      cacheLocation: "localStorage",
      storeAuthStateInCookie: false
    },
    system: {
      allowNativeBroker: false
    }
  };

  msalInstance = new PublicClientApplication(msalConfig);
  msalInitialization = msalInstance.initialize();
} else {
  msalInitialization = Promise.resolve();
}

const defaultScopes = env.b2cScopes && env.b2cScopes.length > 0 
  ? env.b2cScopes 
  : (env.b2cClientId ? [`${env.b2cClientId}/.default`] : []);

export { msalInstance, msalInitialization };

export const loginRequest = {
  scopes: isAuthEnabled ? defaultScopes : []
};

// Función para iniciar sesión con Microsoft (cuenta local de B2C o Microsoft Account)
export const loginWithMicrosoft = (instance: PublicClientApplication) => {
  return instance.loginRedirect({
    ...loginRequest
  });
};

// Función para iniciar sesión con Google usando domain_hint
export const loginWithGoogle = (instance: PublicClientApplication) => {
  return instance.loginRedirect({
    ...loginRequest,
    extraQueryParameters: {
      domain_hint: "google.com"
    }
  });
};

// Función para iniciar sesión con email/password (cuenta local de B2C)
export const loginWithEmailPassword = (instance: PublicClientApplication) => {
  return instance.loginRedirect({
    ...loginRequest
    // Sin domain_hint, B2C mostrará el formulario de login local
  });
};


const getEnvVar = (key: string, options?: { fallback?: string; optional?: boolean }) => {
  const value = import.meta.env[key as keyof ImportMetaEnv] as string | undefined;
  if (value && value.length > 0) {
    return value;
  }

  if (options?.fallback !== undefined) {
    return options.fallback;
  }

  if (options?.optional) {
    return undefined;
  }

  throw new Error(`La variable de entorno ${key} no está configurada.`);
};

const parseCsv = (value?: string) =>
  value
    ?.split(",")
    .map((item) => item.trim())
    .filter(Boolean) ?? [];

const parseBoolean = (value?: string) => value === "true" || value === "1";

// Autenticación opcional - puede estar deshabilitada si no se configura B2C
const authDisabled = parseBoolean(import.meta.env.VITE_AUTH_DISABLED) || false;
const allowLocalFallbacks = import.meta.env.DEV;

const withOptionalFallback = (fallback?: string) =>
  fallback ? { fallback } : undefined;

// Validar que las variables de entorno no contengan valores de ejemplo/placeholder
const validateB2CConfig = (clientId: string, authority: string) => {
  const placeholderPatterns = [
    /tu-tenant/i,
    /00000000-0000-0000-0000-000000000000/,
    /<tu_/i,
    /example/i,
    /placeholder/i,
    /your-tenant/i,
    /your-application/i,
    /\{.*\}/, // Placeholders con llaves
  ];

  const hasPlaceholder = placeholderPatterns.some(pattern => 
    pattern.test(clientId) || pattern.test(authority)
  );

  if (hasPlaceholder) {
    throw new Error(
      "Las variables de entorno de Azure AD B2C contienen valores de ejemplo. " +
      "Por favor, configura los valores reales en el archivo .env. " +
      "Consulta la documentación para obtener los valores correctos."
    );
  }
};

const b2cClientId = authDisabled 
  ? undefined 
  : getEnvVar("VITE_B2C_CLIENT_ID", { optional: true });
const b2cAuthority = authDisabled
  ? undefined
  : getEnvVar("VITE_B2C_AUTHORITY", {
      optional: true,
      fallback: allowLocalFallbacks ? "https://login.microsoftonline.com/tfp/tenant/policy" : undefined
    });

// Validar configuración solo si la autenticación está habilitada y configurada
if (!authDisabled && b2cClientId && b2cAuthority) {
  validateB2CConfig(b2cClientId, b2cAuthority);
}

export const env = {
  apiBaseUrl: getEnvVar(
    "VITE_API_URL",
    allowLocalFallbacks ? withOptionalFallback("http://localhost:9000") : undefined
  ),
  b2cClientId,
  b2cAuthority,
  b2cKnownAuthorities: authDisabled
    ? []
    : parseCsv(import.meta.env.VITE_B2C_KNOWN_AUTHORITY ?? ""),
  b2cRedirectUri: authDisabled
    ? undefined
    : getEnvVar(
        "VITE_B2C_REDIRECT_URI",
        allowLocalFallbacks ? withOptionalFallback("http://localhost:8081") : undefined
      ),
  b2cScopes: authDisabled ? [] : parseCsv(import.meta.env.VITE_B2C_SCOPES ?? ""),
  authDisabled
};


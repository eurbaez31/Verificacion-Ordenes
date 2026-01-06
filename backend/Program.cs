using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using OrderVerificationApi.Services;
using System.Collections.Generic;

var builder = WebApplication.CreateBuilder(args);

// Configuración de Business Central
builder.Services.Configure<BusinessCentralSettings>(
    builder.Configuration.GetSection("BusinessCentral"));

// Configuración de Azure AD B2C (solo para endpoints protegidos)
var b2cSection = builder.Configuration.GetSection("AzureAdB2C");
if (b2cSection.Exists())
{
    static bool LooksLikePlaceholder(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return true;
        return value.Contains('{') ||
               value.Contains('}') ||
               value.Contains("tenant-name", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("b2c-tenant-id", StringComparison.OrdinalIgnoreCase) ||
               value.Contains("backend-api-client-id", StringComparison.OrdinalIgnoreCase);
    }

    var instance = b2cSection["Instance"];
    var domain = b2cSection["Domain"];
    var tenantId = b2cSection["TenantId"];
    var clientId = b2cSection["ClientId"];
    var policyId = b2cSection["SignUpSignInPolicyId"];

    var b2cConfigured =
        !LooksLikePlaceholder(instance) &&
        !LooksLikePlaceholder(domain) &&
        !LooksLikePlaceholder(tenantId) &&
        !LooksLikePlaceholder(clientId) &&
        !string.IsNullOrWhiteSpace(policyId) &&
        Guid.TryParse(tenantId, out _) &&
        Guid.TryParse(clientId, out _);

    var authBuilder = builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme);

    if (b2cConfigured)
    {
        authBuilder.AddMicrosoftIdentityWebApi(b2cSection);
    }
    else
    {
        // Modo "B2C desactivado": no rompe el API y mantiene [Authorize] devolviendo 401.
        // Útil cuando appsettings tiene placeholders (ej: {tenant-name}) en desarrollo.
        authBuilder.AddJwtBearer(options =>
        {
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnMessageReceived = context =>
                {
                    context.NoResult();
                    return Task.CompletedTask;
                }
            };
        });

        Console.WriteLine(
            "WARNING: AzureAdB2C no está configurado (placeholders detectados). " +
            "Los endpoints protegidos requerirán autenticación y devolverán 401, " +
            "pero el endpoint público /api/verify-order seguirá funcionando.");
    }

    builder.Services.AddAuthorization();
}

// Registrar HttpClient para el servicio de BC
builder.Services.AddHttpClient<IBusinessCentralService, BusinessCentralService>();

// Registrar servicio de resolución de Vendor
builder.Services.AddScoped<IVendorResolverService, VendorResolverService>();

// Agregar controladores
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Usar camelCase para coincidir con el frontend
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// Configurar CORS para permitir peticiones desde el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("FrontendPolicy", policy =>
    {
        var allowedOrigins = new List<string>
        {
            // Desarrollo local
            "http://localhost:3000",
            "http://localhost:3002",
            "http://localhost:5173",
            "http://localhost:8080",
            "http://127.0.0.1:8080",
            "http://localhost:8081",
            "http://127.0.0.1:8081",
        };

        // Agregar orígenes desde configuración (para producción)
        var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (corsOrigins != null && corsOrigins.Length > 0)
        {
            allowedOrigins.AddRange(corsOrigins);
        }

        // Agregar URL del App Service si está configurada
        var appServiceUrl = builder.Configuration["AppService:FrontendUrl"];
        if (!string.IsNullOrWhiteSpace(appServiceUrl))
        {
            allowedOrigins.Add(appServiceUrl);
        }

        policy.WithOrigins(allowedOrigins.ToArray())
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

// Swagger para documentación
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() 
    { 
        Title = "Order Verification API", 
        Version = "v1",
        Description = "API para verificar órdenes de compra contra Business Central"
    });
});

var app = builder.Build();

// Configurar pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilitar CORS
app.UseCors("FrontendPolicy");

// Autenticación y autorización (solo si está configurado)
if (app.Configuration.GetSection("AzureAdB2C").Exists())
{
    app.UseAuthentication();
    app.UseAuthorization();
}

// Mapear controladores
app.MapControllers();

// Endpoint de health check básico
app.MapGet("/", () => Results.Ok(new 
{ 
    service = "Order Verification API",
    status = "running",
    timestamp = DateTime.UtcNow
}));

app.Run();

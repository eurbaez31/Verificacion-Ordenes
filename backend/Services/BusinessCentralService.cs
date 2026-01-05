using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using OrderVerificationApi.Models;

namespace OrderVerificationApi.Services;

/// <summary>
/// Configuración de Business Central
/// </summary>
public class BusinessCentralSettings
{
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string CompanyId { get; set; } = string.Empty;
    public string Environment { get; set; } = "Production";
    
    // Configuración de la API personalizada de Melcon
    public string ApiPublisher { get; set; } = "melcon";
    public string ApiGroup { get; set; } = "purchasing";
    public string ApiVersion { get; set; } = "v2.0";
}

/// <summary>
/// Servicio para conectarse a Business Central via API personalizada de Melcon
/// </summary>
public class BusinessCentralService : IBusinessCentralService
{
    private readonly HttpClient _httpClient;
    private readonly BusinessCentralSettings _settings;
    private readonly ILogger<BusinessCentralService> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;

    public BusinessCentralService(
        HttpClient httpClient,
        IOptions<BusinessCentralSettings> settings,
        ILogger<BusinessCentralService> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Construye la URL base para la API personalizada de Melcon, dada un Environment.
    /// En Business Central Online el segmento suele ser "Production" o el nombre exacto del entorno.
    /// </summary>
    private string GetMelconApiBaseUrl(string environment)
    {
        return $"https://api.businesscentral.dynamics.com/v2.0/{_settings.TenantId}/{environment}/api/{_settings.ApiPublisher}/{_settings.ApiGroup}/{_settings.ApiVersion}";
    }

    private IEnumerable<string> GetEnvironmentCandidates()
    {
        // Probar primero el valor configurado, luego valores comunes.
        // Usamos OrdinalIgnoreCase para evitar duplicados; BC puede ser sensible a mayúsculas,
        // por eso agregamos también variantes en minúscula.
        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var list = new List<string>();

        void Add(string? env)
        {
            if (string.IsNullOrWhiteSpace(env)) return;
            if (seen.Add(env)) list.Add(env);
        }

        Add(_settings.Environment);
        Add("Production");
        Add("Sandbox");
        Add("production");
        Add("sandbox");

        return list;
    }

    private static bool IsNoEnvironmentError(System.Net.HttpStatusCode statusCode, string content)
    {
        // BC devuelve 404 con {"error":{"code":"NoEnvironment","message":"Environment does not exist."}}
        return statusCode == System.Net.HttpStatusCode.NotFound &&
               content.Contains("\"NoEnvironment\"", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Obtiene una orden de compra de Business Central por su número
    /// Busca primero en órdenes activas, luego en archivadas
    /// </summary>
    public async Task<BCPurchaseOrder?> GetPurchaseOrderAsync(string orderNumber)
    {
        try
        {
            await EnsureAccessTokenAsync();

            // Primero buscar en órdenes activas (Purchase Header)
            var order = await SearchInPurchaseOrdersAsync(orderNumber);
            
            if (order != null)
            {
                _logger.LogInformation("Orden encontrada en Purchase Orders: {OrderNumber}", orderNumber);
                return order;
            }

            // Si no se encuentra, buscar en órdenes archivadas
            _logger.LogInformation("Orden no encontrada en activas, buscando en archivadas: {OrderNumber}", orderNumber);
            var archivedOrder = await SearchInArchivedOrdersAsync(orderNumber);
            
            if (archivedOrder != null)
            {
                _logger.LogInformation("Orden encontrada en Purchase Header Archive: {OrderNumber}", orderNumber);
                // Convertir orden archivada a formato estándar
                return ConvertArchivedToOrder(archivedOrder);
            }

            _logger.LogWarning("Orden no encontrada en ninguna tabla: {OrderNumber}", orderNumber);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener orden de compra {OrderNumber}", orderNumber);
            throw;
        }
    }

    /// <summary>
    /// Busca en la tabla Purchase Header (órdenes activas)
    /// </summary>
    private async Task<BCPurchaseOrder?> SearchInPurchaseOrdersAsync(string orderNumber)
    {
        var safeOrderNumber = orderNumber.Replace("'", "''");
        _logger.LogInformation("Consultando Purchase Orders para: {OrderNumber}", orderNumber);

        foreach (var env in GetEnvironmentCandidates())
        {
            var baseUrl = GetMelconApiBaseUrl(env);
            var endpoint = $"{baseUrl}/companies({_settings.CompanyId})/purchaseOrders?$filter=number eq '{safeOrderNumber}'";
            _logger.LogDebug("Endpoint (env {Env}): {Endpoint}", env, endpoint);

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (IsNoEnvironmentError(response.StatusCode, content))
                {
                    _logger.LogWarning("BC indicó NoEnvironment para env {Env}. Probando siguiente...", env);
                    continue;
                }

                _logger.LogWarning("Error al consultar Purchase Orders: {StatusCode} - {Content}", response.StatusCode, content);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Business Central rechazó las credenciales/permisos para Purchase Orders.");
                }

                return null;
            }

            _logger.LogDebug("Respuesta Purchase Orders: {Content}", content);
            var odataResponse = JsonSerializer.Deserialize<BCODataResponse<BCPurchaseOrder>>(content);
            _logger.LogInformation("Órdenes activas encontradas (env {Env}): {Count}", env, odataResponse?.Value.Count ?? 0);
            return odataResponse?.Value.FirstOrDefault();
        }

        throw new InvalidOperationException("Business Central respondió NoEnvironment para todos los entornos probados. Verifica el nombre exacto del Environment.");
    }

    /// <summary>
    /// Busca en la tabla Purchase Header Archive (órdenes archivadas)
    /// </summary>
    private async Task<BCPurchaseOrderArchive?> SearchInArchivedOrdersAsync(string orderNumber)
    {
        var safeOrderNumber = orderNumber.Replace("'", "''");
        // Filtrar por Document Type = Order y el número
        _logger.LogInformation("Consultando Purchase Header Archive para: {OrderNumber}", orderNumber);

        foreach (var env in GetEnvironmentCandidates())
        {
            var baseUrl = GetMelconApiBaseUrl(env);
            var endpoint = $"{baseUrl}/companies({_settings.CompanyId})/purchaseHeaderArchives?$filter=number eq '{safeOrderNumber}'&$orderby=versionNo desc&$top=1";
            _logger.LogDebug("Endpoint Archive (env {Env}): {Endpoint}", env, endpoint);

            var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                if (IsNoEnvironmentError(response.StatusCode, content))
                {
                    _logger.LogWarning("BC indicó NoEnvironment para env {Env} (Archive). Probando siguiente...", env);
                    continue;
                }

                _logger.LogWarning("Error al consultar Archive: {StatusCode} - {Content}", response.StatusCode, content);
                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                    response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    throw new UnauthorizedAccessException("Business Central rechazó las credenciales/permisos para Purchase Header Archive.");
                }
                return null;
            }

            _logger.LogDebug("Respuesta Archive: {Content}", content);
            var odataResponse = JsonSerializer.Deserialize<BCODataResponse<BCPurchaseOrderArchive>>(content);
            _logger.LogInformation("Órdenes archivadas encontradas (env {Env}): {Count}", env, odataResponse?.Value.Count ?? 0);
            return odataResponse?.Value.FirstOrDefault();
        }

        throw new InvalidOperationException("Business Central respondió NoEnvironment para todos los entornos probados (Archive). Verifica el nombre exacto del Environment.");
    }

    /// <summary>
    /// Convierte una orden archivada al formato estándar
    /// </summary>
    private BCPurchaseOrder ConvertArchivedToOrder(BCPurchaseOrderArchive archived)
    {
        return new BCPurchaseOrder
        {
            Id = archived.Id,
            Number = archived.Number,
            DocumentDate = archived.OrderDate,
            BuyFromVendorNo = archived.BuyFromVendorNo,
            BuyFromVendorName = archived.BuyFromVendorName,
            Status = $"{archived.Status} (Archivada v{archived.VersionNo})"
        };
    }

    /// <summary>
    /// Obtiene todas las órdenes de compra (solo para depuración)
    /// </summary>
    public async Task<List<BCPurchaseOrder>> GetAllPurchaseOrdersAsync(int top = 10)
    {
        try
        {
            await EnsureAccessTokenAsync();

            _logger.LogInformation("Listando órdenes de compra (top {Top}) desde API Melcon", top);

            foreach (var env in GetEnvironmentCandidates())
            {
                var baseUrl = GetMelconApiBaseUrl(env);
                var endpoint = $"{baseUrl}/companies({_settings.CompanyId})/purchaseOrders?$top={top}";
                _logger.LogDebug("Endpoint (env {Env}): {Endpoint}", env, endpoint);

                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (IsNoEnvironmentError(response.StatusCode, content))
                    {
                        _logger.LogWarning("BC indicó NoEnvironment para env {Env} (list). Probando siguiente...", env);
                        continue;
                    }

                    _logger.LogWarning("Error al listar órdenes: {StatusCode} - {Content}", response.StatusCode, content);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized ||
                        response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        throw new UnauthorizedAccessException("Business Central rechazó las credenciales/permisos al listar Purchase Orders.");
                    }
                    return new List<BCPurchaseOrder>();
                }

                _logger.LogDebug("Respuesta de BC: {Content}", content);
                var odataResponse = JsonSerializer.Deserialize<BCODataResponse<BCPurchaseOrder>>(content);
                return odataResponse?.Value ?? new List<BCPurchaseOrder>();
            }

            throw new InvalidOperationException("Business Central respondió NoEnvironment para todos los entornos probados (list). Verifica el nombre exacto del Environment.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al listar órdenes de compra");
            throw;
        }
    }

    /// <summary>
    /// Asegura que tenemos un token de acceso válido usando OAuth2 Client Credentials
    /// </summary>
    private async Task EnsureAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            return; // Token aún válido
        }

        var tokenEndpoint = $"https://login.microsoftonline.com/{_settings.TenantId}/oauth2/v2.0/token";

        var tokenRequest = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _settings.ClientId,
            ["client_secret"] = _settings.ClientSecret,
            ["scope"] = "https://api.businesscentral.dynamics.com/.default"
        };

        var response = await _httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(tokenRequest));
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Error al obtener token de Azure AD: {Error}", error);
            throw new InvalidOperationException("No se pudo autenticar con Azure AD");
        }

        var tokenResponse = await response.Content.ReadFromJsonAsync<TokenResponse>();
        
        if (tokenResponse == null)
        {
            throw new InvalidOperationException("Respuesta de token inválida");
        }

        _accessToken = tokenResponse.AccessToken;
        _tokenExpiry = DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn - 60); // 1 minuto de margen

        _logger.LogInformation("Token de Business Central obtenido correctamente");
    }

    public async Task<string?> GetVendorNoByPortalIdAsync(Guid vendorPortalId)
    {
        try
        {
            await EnsureAccessTokenAsync();

            _logger.LogInformation("Buscando Vendor No. por Portal ID: {VendorPortalId}", vendorPortalId);

            foreach (var env in GetEnvironmentCandidates())
            {
                var baseUrl = GetMelconApiBaseUrl(env);
                var endpoint = $"{baseUrl}/companies({_settings.CompanyId})/portalVendors?$filter=vendorPortalId eq {vendorPortalId}";
                _logger.LogDebug("Endpoint Portal Vendors (env {Env}): {Endpoint}", env, endpoint);

                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (IsNoEnvironmentError(response.StatusCode, content))
                    {
                        _logger.LogWarning("BC indicó NoEnvironment para env {Env} (Portal Vendors). Probando siguiente...", env);
                        continue;
                    }

                    _logger.LogWarning("Error al consultar Portal Vendors: {StatusCode} - {Content}", response.StatusCode, content);
                    return null;
                }

                _logger.LogDebug("Respuesta Portal Vendors: {Content}", content);
                var odataResponse = JsonSerializer.Deserialize<BCODataResponse<PortalVendor>>(content);
                var vendor = odataResponse?.Value.FirstOrDefault();

                if (vendor != null)
                {
                    _logger.LogInformation("Vendor encontrado: {VendorNo} para Portal ID {VendorPortalId}", vendor.VendorNo, vendorPortalId);
                    return vendor.VendorNo;
                }
            }

            _logger.LogWarning("Vendor no encontrado para Portal ID: {VendorPortalId}", vendorPortalId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener Vendor No. por Portal ID {VendorPortalId}", vendorPortalId);
            throw;
        }
    }

    public async Task<List<BCPurchaseOrder>> GetPurchaseOrdersByVendorAsync(string vendorNo, int top = 50)
    {
        try
        {
            await EnsureAccessTokenAsync();

            _logger.LogInformation("Listando órdenes de compra para proveedor {VendorNo} (top: {Top})", vendorNo, top);

            foreach (var env in GetEnvironmentCandidates())
            {
                var baseUrl = GetMelconApiBaseUrl(env);
                var safeVendorNo = vendorNo.Replace("'", "''");
                var endpoint = $"{baseUrl}/companies({_settings.CompanyId})/purchaseOrders?$filter=vendorId eq '{safeVendorNo}'&$top={top}";
                _logger.LogDebug("Endpoint (env {Env}): {Endpoint}", env, endpoint);

                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (IsNoEnvironmentError(response.StatusCode, content))
                    {
                        _logger.LogWarning("BC indicó NoEnvironment para env {Env} (vendor orders). Probando siguiente...", env);
                        continue;
                    }

                    _logger.LogWarning("Error al listar órdenes por proveedor: {StatusCode} - {Content}", response.StatusCode, content);
                    return new List<BCPurchaseOrder>();
                }

                _logger.LogDebug("Respuesta de BC: {Content}", content);
                var odataResponse = JsonSerializer.Deserialize<BCODataResponse<BCPurchaseOrder>>(content);
                return odataResponse?.Value ?? new List<BCPurchaseOrder>();
            }

            return new List<BCPurchaseOrder>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener órdenes por proveedor {VendorNo}", vendorNo);
            throw;
        }
    }

    public async Task<byte[]?> GetPurchaseOrderPdfAsync(string orderNumber)
    {
        try
        {
            await EnsureAccessTokenAsync();

            _logger.LogInformation("Obteniendo PDF de orden: {OrderNumber}", orderNumber);

            foreach (var env in GetEnvironmentCandidates())
            {
                var baseUrl = GetMelconApiBaseUrl(env);
                var safeOrderNumber = orderNumber.Replace("'", "''");
                var endpoint = $"{baseUrl}/companies({_settings.CompanyId})/purchaseOrderPdfs?$filter=number eq '{safeOrderNumber}'";
                _logger.LogDebug("Endpoint PDF (env {Env}): {Endpoint}", env, endpoint);

                var request = new HttpRequestMessage(HttpMethod.Get, endpoint);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _accessToken);
                request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await _httpClient.SendAsync(request);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    if (IsNoEnvironmentError(response.StatusCode, content))
                    {
                        _logger.LogWarning("BC indicó NoEnvironment para env {Env} (PDF). Probando siguiente...", env);
                        continue;
                    }

                    _logger.LogWarning("Error al obtener PDF: {StatusCode} - {Content}", response.StatusCode, content);
                    return null;
                }

                _logger.LogDebug("Respuesta PDF: {Content}", content);
                var odataResponse = JsonSerializer.Deserialize<BCODataResponse<PurchaseOrderPdf>>(content);
                var pdfData = odataResponse?.Value.FirstOrDefault();

                if (pdfData != null && !string.IsNullOrEmpty(pdfData.PdfBase64))
                {
                    try
                    {
                        var pdfBytes = Convert.FromBase64String(pdfData.PdfBase64);
                        _logger.LogInformation("PDF obtenido exitosamente para orden {OrderNumber} ({Size} bytes)", orderNumber, pdfBytes.Length);
                        return pdfBytes;
                    }
                    catch (FormatException ex)
                    {
                        _logger.LogError(ex, "Error al decodificar Base64 del PDF para orden {OrderNumber}", orderNumber);
                        return null;
                    }
                }
            }

            _logger.LogWarning("PDF no encontrado para orden: {OrderNumber}", orderNumber);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener PDF de orden {OrderNumber}", orderNumber);
            throw;
        }
    }

    private class TokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;

        [JsonPropertyName("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonPropertyName("token_type")]
        public string TokenType { get; set; } = string.Empty;
    }
}

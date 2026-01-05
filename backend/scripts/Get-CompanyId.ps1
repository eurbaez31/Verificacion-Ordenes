# Script para obtener el Company ID de Business Central
# Requiere: Módulo AzureAD o Az.Accounts instalado

param(
    [Parameter(Mandatory=$true)]
    [string]$TenantId,
    
    [Parameter(Mandatory=$true)]
    [string]$ClientId,
    
    [Parameter(Mandatory=$true)]
    [string]$ClientSecret,
    
    [Parameter(Mandatory=$true)]
    [ValidateSet("Production", "Sandbox")]
    [string]$Environment
)

Write-Host "Obteniendo Company ID de Business Central..." -ForegroundColor Cyan

# Construir URL de token
$tokenUrl = "https://login.microsoftonline.com/$TenantId/oauth2/v2.0/token"

# Solicitar token de acceso
$tokenBody = @{
    grant_type    = "client_credentials"
    client_id     = $ClientId
    client_secret = $ClientSecret
    scope         = "https://api.businesscentral.dynamics.com/.default"
}

Write-Host "Solicitando token de acceso..." -ForegroundColor Yellow

try {
    $tokenResponse = Invoke-RestMethod -Method Post -Uri $tokenUrl -Body $tokenBody -ContentType "application/x-www-form-urlencoded"
    $accessToken = $tokenResponse.access_token
    
    Write-Host "✓ Token obtenido correctamente" -ForegroundColor Green
} catch {
    Write-Host "✗ Error al obtener token: $_" -ForegroundColor Red
    exit 1
}

# Construir URL de API
$apiUrl = "https://api.businesscentral.dynamics.com/v2.0/$TenantId/$Environment/api/v2.0/companies"

Write-Host "Consultando compañías..." -ForegroundColor Yellow

try {
    $headers = @{
        Authorization = "Bearer $accessToken"
        Accept        = "application/json"
    }
    
    $companies = Invoke-RestMethod -Method Get -Uri $apiUrl -Headers $headers
    
    Write-Host "`n✓ Compañías encontradas:" -ForegroundColor Green
    Write-Host "`n" -NoNewline
    
    foreach ($company in $companies.value) {
        Write-Host "  ID:   $($company.id)" -ForegroundColor Cyan
        Write-Host "  Nombre: $($company.name)" -ForegroundColor White
        Write-Host "  ---" -ForegroundColor DarkGray
    }
    
    Write-Host "`nCopia el ID de la compañía que necesites y úsalo como CompanyId en appsettings.json" -ForegroundColor Yellow
    
} catch {
    Write-Host "✗ Error al consultar compañías: $_" -ForegroundColor Red
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "  Código de estado: $statusCode" -ForegroundColor Red
    }
    exit 1
}


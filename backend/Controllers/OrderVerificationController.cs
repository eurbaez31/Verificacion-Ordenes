using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderVerificationApi.Models;
using OrderVerificationApi.Services;

namespace OrderVerificationApi.Controllers;

/// <summary>
/// Controlador para verificación de órdenes de compra
/// </summary>
[ApiController]
[Route("api/verify-order")]
public class OrderVerificationController : ControllerBase
{
    private readonly IBusinessCentralService _bcService;
    private readonly IVendorResolverService _vendorResolver;
    private readonly ILogger<OrderVerificationController> _logger;

    public OrderVerificationController(
        IBusinessCentralService bcService,
        IVendorResolverService vendorResolver,
        ILogger<OrderVerificationController> logger)
    {
        _bcService = bcService;
        _vendorResolver = vendorResolver;
        _logger = logger;
    }

    /// <summary>
    /// Verifica una orden de compra por su código (público, sin autenticación)
    /// </summary>
    /// <param name="orderCode">Código de la orden (ej: PO-10023)</param>
    /// <returns>Respuesta de verificación con detalles de la orden</returns>
    [HttpGet("{orderCode}")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOrder(string orderCode)
    {
        try
        {
            _logger.LogInformation("Verificando orden: {OrderCode}", orderCode);

            // Obtener la orden de Business Central
            var bcOrder = await _bcService.GetPurchaseOrderAsync(orderCode);

            if (bcOrder == null)
            {
                _logger.LogWarning("Orden no encontrada: {OrderCode}", orderCode);
                return NotFound(new VerificationResponse
                {
                    Success = false,
                    Status = "not_found",
                    Message = "Orden no encontrada"
                });
            }

            // Verificar si la orden está aprobada
            if (string.IsNullOrWhiteSpace(bcOrder.Status))
            {
                _logger.LogWarning("Orden {OrderCode} tiene Status null o vacío. Permitiendo verificación.", orderCode);
                // Continuar con la verificación normal si el status está vacío
            }
            else
            {
                var statusLower = bcOrder.Status.ToLowerInvariant();
                if (statusLower == "open" || 
                    (statusLower.Contains("pending") && statusLower.Contains("approval")))
                {
                    _logger.LogInformation("Orden {OrderCode} no está aprobada. Estado: {Status}", orderCode, bcOrder.Status);
                    return Ok(new VerificationResponse
                    {
                        Success = false,
                        Status = "not_approved",
                        Message = "Esta orden aún no ha sido aprobada. Solo se pueden verificar órdenes que hayan sido aprobadas."
                    });
                }
            }

            // Mapear la respuesta de BC al formato del frontend
            var response = new VerificationResponse
            {
                Success = true,
                Status = "verified",
                Data = new OrderDetails
                {
                    OrderNumber = bcOrder.Number,
                    Date = bcOrder.DocumentDate != DateTime.MinValue 
                        ? bcOrder.DocumentDate.ToString("dd MMMM yyyy", new CultureInfo("es-ES"))
                        : "Sin fecha",
                    Vendor = !string.IsNullOrEmpty(bcOrder.BuyFromVendorName) 
                        ? bcOrder.BuyFromVendorName 
                        : bcOrder.BuyFromVendorNo,
                    Department = "Compras",
                    Items = bcOrder.PurchaseOrderLines?.Select(line => new OrderItemDto
                    {
                        Description = line.Description,
                        Quantity = line.Quantity,
                        UnitPrice = line.DirectUnitCost
                    }).ToList() ?? new List<OrderItemDto>(),
                    Total = bcOrder.AmountIncludingVAT,
                    Status = MapBCStatus(bcOrder.Status),
                    ApprovedBy = "Administrador"
                }
            };

            _logger.LogInformation("Orden verificada exitosamente: {OrderCode}", orderCode);
            return Ok(response);
        }
        catch (UnauthorizedAccessException uex)
        {
            _logger.LogError(uex, "Business Central rechazó la solicitud para orden: {OrderCode}", orderCode);
            return StatusCode(502, new VerificationResponse
            {
                Success = false,
                Status = "error",
                Message = "Business Central rechazó las credenciales/permisos. Revisa la aplicación Entra en BC y los permission sets por compañía/entorno."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al verificar orden: {OrderCode}. Excepción: {ExceptionType}, Mensaje: {Message}, StackTrace: {StackTrace}", 
                orderCode, ex.GetType().Name, ex.Message, ex.StackTrace);
            return StatusCode(500, new VerificationResponse
            {
                Success = false,
                Status = "error",
                Message = "Error de conexión con el servidor"
            });
        }
    }

    /// <summary>
    /// Endpoint de prueba para verificar que la API está funcionando
    /// </summary>
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }

    /// <summary>
    /// Endpoint de depuración para listar órdenes (temporal - eliminar en producción)
    /// </summary>
    [HttpGet("debug/list-orders")]
    public async Task<IActionResult> DebugListOrders([FromQuery] int top = 10)
    {
        try
        {
            _logger.LogInformation("Listando órdenes para depuración (top: {Top})", top);
            
            var orders = await _bcService.GetAllPurchaseOrdersAsync(top);
            
            return Ok(new { 
                count = orders.Count,
                orders = orders.Select(o => new {
                    o.Number,
                    o.OrderDate,
                    o.VendorName,
                    o.Status
                })
            });
        }
        catch (UnauthorizedAccessException uex)
        {
            _logger.LogError(uex, "Business Central rechazó el debug/list-orders");
            return StatusCode(502, new { error = "BC_Unauthorized", message = uex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error en debug");
            return StatusCode(500, new { error = ex.Message, details = ex.ToString() });
        }
    }

    /// <summary>
    /// Descarga el PDF de una orden de compra (requiere autenticación)
    /// </summary>
    /// <param name="orderCode">Código de la orden</param>
    /// <returns>PDF de la orden de compra</returns>
    [HttpGet("{orderCode}/pdf")]
    [Authorize]
    public async Task<IActionResult> DownloadOrderPdf(string orderCode)
    {
        try
        {
            _logger.LogInformation("Solicitud de descarga PDF para orden: {OrderCode}", orderCode);

            // Obtener Vendor Portal ID del claim
            var vendorPortalIdClaim = User.FindFirst("extension_vendorId")?.Value;
            if (string.IsNullOrEmpty(vendorPortalIdClaim) || !Guid.TryParse(vendorPortalIdClaim, out var vendorPortalId))
            {
                _logger.LogWarning("Usuario no tiene claim extension_vendorId válido");
                return Unauthorized(new { error = "Usuario no tiene Vendor ID asignado" });
            }

            // Resolver Vendor No. desde Portal ID
            var vendorNo = await _vendorResolver.GetVendorNoAsync(vendorPortalId);
            if (string.IsNullOrEmpty(vendorNo))
            {
                _logger.LogWarning("No se encontró Vendor No. para Portal ID: {VendorPortalId}", vendorPortalId);
                return Forbid("No se encontró el proveedor asociado a tu cuenta");
            }

            // Verificar que la orden pertenece al proveedor
            var order = await _bcService.GetPurchaseOrderAsync(orderCode);
            if (order == null)
            {
                return NotFound(new { error = "Orden no encontrada" });
            }

            // Comparar vendorId de la orden con el vendorNo del usuario
            if (!order.BuyFromVendorNo.Equals(vendorNo, StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Usuario {VendorNo} intentó descargar PDF de orden {OrderCode} que pertenece a {OrderVendorNo}", 
                    vendorNo, orderCode, order.BuyFromVendorNo);
                return Forbid("No tienes permiso para descargar esta orden");
            }

            // Obtener el PDF desde BC
            var pdfBytes = await _bcService.GetPurchaseOrderPdfAsync(orderCode);
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                _logger.LogWarning("No se pudo generar el PDF para orden: {OrderCode}", orderCode);
                return StatusCode(500, new { error = "No se pudo generar el PDF" });
            }

            _logger.LogInformation("PDF generado exitosamente para orden {OrderCode} ({Size} bytes)", orderCode, pdfBytes.Length);
            return File(pdfBytes, "application/pdf", $"Pedido_{orderCode}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al descargar PDF de orden: {OrderCode}", orderCode);
            return StatusCode(500, new { error = "Error al generar el PDF" });
        }
    }

    /// <summary>
    /// Obtiene el historial de órdenes del proveedor autenticado
    /// </summary>
    /// <param name="top">Número máximo de órdenes a devolver (default: 50)</param>
    /// <returns>Lista de órdenes del proveedor</returns>
    [HttpGet("vendor/orders")]
    [Authorize]
    public async Task<IActionResult> GetMyOrders([FromQuery] int top = 50)
    {
        try
        {
            _logger.LogInformation("Solicitud de historial de órdenes (top: {Top})", top);

            // Obtener Vendor Portal ID del claim
            var vendorPortalIdClaim = User.FindFirst("extension_vendorId")?.Value;
            if (string.IsNullOrEmpty(vendorPortalIdClaim) || !Guid.TryParse(vendorPortalIdClaim, out var vendorPortalId))
            {
                _logger.LogWarning("Usuario no tiene claim extension_vendorId válido");
                return Unauthorized(new { error = "Usuario no tiene Vendor ID asignado" });
            }

            // Resolver Vendor No. desde Portal ID
            var vendorNo = await _vendorResolver.GetVendorNoAsync(vendorPortalId);
            if (string.IsNullOrEmpty(vendorNo))
            {
                _logger.LogWarning("No se encontró Vendor No. para Portal ID: {VendorPortalId}", vendorPortalId);
                return Forbid("No se encontró el proveedor asociado a tu cuenta");
            }

            // Obtener órdenes del proveedor
            var orders = await _bcService.GetPurchaseOrdersByVendorAsync(vendorNo, top);

            _logger.LogInformation("Historial obtenido: {Count} órdenes para proveedor {VendorNo}", orders.Count, vendorNo);
            return Ok(new
            {
                vendorNo,
                count = orders.Count,
                orders = orders.Select(o => new
                {
                    o.Number,
                    date = o.DocumentDate != DateTime.MinValue 
                        ? o.DocumentDate.ToString("dd MMMM yyyy", new CultureInfo("es-ES"))
                        : "Sin fecha",
                    vendor = o.BuyFromVendorName,
                    total = o.AmountIncludingVAT,
                    status = MapBCStatus(o.Status)
                })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener historial de órdenes");
            return StatusCode(500, new { error = "Error al obtener el historial" });
        }
    }

    /// <summary>
    /// Mapea el estado de BC a un texto legible en español
    /// </summary>
    private static string MapBCStatus(string bcStatus)
    {
        if (string.IsNullOrWhiteSpace(bcStatus))
        {
            return "Desconocido";
        }
        
        return bcStatus.ToLowerInvariant() switch
        {
            "open" => "Abierta",
            "released" => "Aprobada",
            "pending approval" => "Pendiente de Aprobación",
            "pending prepayment" => "Aprobada",
            "pending_x0020_prepayment" => "Aprobada",
            _ => bcStatus
        };
    }
}


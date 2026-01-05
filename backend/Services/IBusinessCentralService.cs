using OrderVerificationApi.Models;

namespace OrderVerificationApi.Services;

/// <summary>
/// Interfaz para el servicio de conexión a Business Central
/// </summary>
public interface IBusinessCentralService
{
    /// <summary>
    /// Obtiene una orden de compra de Business Central por su número
    /// </summary>
    /// <param name="orderNumber">Número de la orden (ej: PO-10023)</param>
    /// <returns>Orden de compra o null si no se encuentra</returns>
    Task<BCPurchaseOrder?> GetPurchaseOrderAsync(string orderNumber);

    /// <summary>
    /// Obtiene todas las órdenes de compra (solo para depuración)
    /// </summary>
    /// <param name="top">Número máximo de órdenes a devolver</param>
    /// <returns>Lista de órdenes de compra</returns>
    Task<List<BCPurchaseOrder>> GetAllPurchaseOrdersAsync(int top = 10);

    /// <summary>
    /// Obtiene el Vendor No. desde el Vendor Portal ID (GUID)
    /// </summary>
    /// <param name="vendorPortalId">GUID del Vendor Portal ID</param>
    /// <returns>Vendor No. o null si no se encuentra</returns>
    Task<string?> GetVendorNoByPortalIdAsync(Guid vendorPortalId);

    /// <summary>
    /// Obtiene las órdenes de compra de un proveedor específico
    /// </summary>
    /// <param name="vendorNo">Código del proveedor</param>
    /// <param name="top">Número máximo de órdenes a devolver</param>
    /// <returns>Lista de órdenes de compra del proveedor</returns>
    Task<List<BCPurchaseOrder>> GetPurchaseOrdersByVendorAsync(string vendorNo, int top = 50);

    /// <summary>
    /// Obtiene el PDF de una orden de compra
    /// </summary>
    /// <param name="orderNumber">Número de la orden</param>
    /// <returns>PDF en formato Base64 o null si no se encuentra</returns>
    Task<byte[]?> GetPurchaseOrderPdfAsync(string orderNumber);
}


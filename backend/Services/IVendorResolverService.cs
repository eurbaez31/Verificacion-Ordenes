namespace OrderVerificationApi.Services;

/// <summary>
/// Servicio para resolver el Vendor No. desde los claims del usuario autenticado
/// </summary>
public interface IVendorResolverService
{
    /// <summary>
    /// Obtiene el Vendor No. del usuario autenticado desde el claim extension_vendorId
    /// </summary>
    /// <param name="vendorPortalId">GUID del Vendor Portal ID desde el claim</param>
    /// <returns>Vendor No. o null si no se encuentra</returns>
    Task<string?> GetVendorNoAsync(Guid vendorPortalId);
}


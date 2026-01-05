using OrderVerificationApi.Models;

namespace OrderVerificationApi.Services;

/// <summary>
/// Implementación del servicio para resolver Vendor No. desde B2C claims
/// </summary>
public class VendorResolverService : IVendorResolverService
{
    private readonly IBusinessCentralService _bcService;
    private readonly ILogger<VendorResolverService> _logger;

    public VendorResolverService(
        IBusinessCentralService bcService,
        ILogger<VendorResolverService> logger)
    {
        _bcService = bcService;
        _logger = logger;
    }

    public async Task<string?> GetVendorNoAsync(Guid vendorPortalId)
    {
        _logger.LogInformation("Resolviendo Vendor No. para Portal ID: {VendorPortalId}", vendorPortalId);
        return await _bcService.GetVendorNoByPortalIdAsync(vendorPortalId);
    }
}


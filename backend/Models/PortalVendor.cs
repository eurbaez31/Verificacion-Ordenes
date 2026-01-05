using System.Text.Json.Serialization;

namespace OrderVerificationApi.Models;

/// <summary>
/// Modelo para la API Portal Vendors de Business Central
/// </summary>
public class PortalVendor
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("vendorNo")]
    public string VendorNo { get; set; } = string.Empty;

    [JsonPropertyName("vendorName")]
    public string VendorName { get; set; } = string.Empty;

    [JsonPropertyName("vendorPortalId")]
    public Guid VendorPortalId { get; set; }
}


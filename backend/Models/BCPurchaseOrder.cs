using System.Text.Json.Serialization;

namespace OrderVerificationApi.Models;

/// <summary>
/// Modelo que representa una orden de compra de Business Central
/// Mapea la respuesta de la API personalizada de Melcon (Page 60366)
/// </summary>
public class BCPurchaseOrder
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("documentDate")]
    public DateTime DocumentDate { get; set; }

    [JsonPropertyName("buyFromVendorNo")]
    public string BuyFromVendorNo { get; set; } = string.Empty;

    [JsonPropertyName("vendorName")]
    public string BuyFromVendorName { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("amount")]
    public decimal Amount { get; set; }

    [JsonPropertyName("amountIncludingVAT")]
    public decimal AmountIncludingVAT { get; set; }

    [JsonPropertyName("currencyCode")]
    public string CurrencyCode { get; set; } = string.Empty;

    [JsonPropertyName("paymentTermsCode")]
    public string PaymentTermsCode { get; set; } = string.Empty;

    // Propiedades de compatibilidad con el código existente
    public DateTime OrderDate => DocumentDate;
    public string VendorNumber => BuyFromVendorNo;
    public string VendorName => BuyFromVendorName;
    public decimal TotalAmountIncludingVAT => AmountIncludingVAT;

    // Las líneas se cargarán por separado si es necesario
    public List<BCPurchaseOrderLine> PurchaseOrderLines { get; set; } = new();
}

/// <summary>
/// Modelo para órdenes archivadas (Page 60367)
/// </summary>
public class BCPurchaseOrderArchive
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("documentType")]
    public string DocumentType { get; set; } = string.Empty;

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("docNoOccurrence")]
    public int DocNoOccurrence { get; set; }

    [JsonPropertyName("versionNo")]
    public int VersionNo { get; set; }

    [JsonPropertyName("buyFromVendorNo")]
    public string BuyFromVendorNo { get; set; } = string.Empty;

    [JsonPropertyName("vendorName")]
    public string BuyFromVendorName { get; set; } = string.Empty;

    [JsonPropertyName("orderDate")]
    public DateTime OrderDate { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}

/// <summary>
/// Línea de orden de compra de Business Central
/// </summary>
public class BCPurchaseOrderLine
{
    [JsonPropertyName("sequence")]
    public int Sequence { get; set; }

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("quantity")]
    public decimal Quantity { get; set; }

    [JsonPropertyName("directUnitCost")]
    public decimal DirectUnitCost { get; set; }

    [JsonPropertyName("lineAmount")]
    public decimal LineAmount { get; set; }
}

/// <summary>
/// Respuesta OData que envuelve la lista de órdenes
/// </summary>
public class BCODataResponse<T>
{
    [JsonPropertyName("value")]
    public List<T> Value { get; set; } = new();
}

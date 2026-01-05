using System.Text.Json.Serialization;

namespace OrderVerificationApi.Models;

/// <summary>
/// Modelo para la API Purchase Order PDF de Business Central
/// </summary>
public class PurchaseOrderPdf
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; }

    [JsonPropertyName("number")]
    public string Number { get; set; } = string.Empty;

    [JsonPropertyName("pdfBase64")]
    public string PdfBase64 { get; set; } = string.Empty;
}


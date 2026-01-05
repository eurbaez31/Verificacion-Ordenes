namespace OrderVerificationApi.Models;

/// <summary>
/// Detalles completos de una orden de compra verificada
/// Coincide con la interfaz OrderDetails del frontend
/// </summary>
public class OrderDetails
{
    public string OrderNumber { get; set; } = string.Empty;
    public string Date { get; set; } = string.Empty;
    public string Vendor { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public List<OrderItemDto> Items { get; set; } = new();
    public decimal Total { get; set; }
    public string Status { get; set; } = string.Empty;
    public string ApprovedBy { get; set; } = string.Empty;
}


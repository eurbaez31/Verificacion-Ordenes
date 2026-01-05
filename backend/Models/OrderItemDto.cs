namespace OrderVerificationApi.Models;

/// <summary>
/// Representa un artículo de la orden de compra
/// </summary>
public class OrderItemDto
{
    public string Description { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}


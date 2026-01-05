namespace OrderVerificationApi.Models;

/// <summary>
/// Respuesta de verificación de orden
/// Coincide con la interfaz VerificationResponse del frontend
/// </summary>
public class VerificationResponse
{
    public bool Success { get; set; }
    public string Status { get; set; } = "error"; // "verified", "not_found", "not_approved", "error"
    public OrderDetails? Data { get; set; }
    public string? Message { get; set; }
}


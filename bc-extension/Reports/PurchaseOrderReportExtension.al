/// <summary>
/// Extensión del reporte estándar de Purchase Order (405) que agrega:
/// - Un layout RDLC adicional llamado "Pedido-QR"
/// - Columnas en el dataset para URL del portal y código QR en Base64
/// - Generación automática de QR en el footer del reporte
/// </summary>
/// <remarks>
/// El layout RDLC "Pedido-QR.rdlc" debe asignarse manualmente como Custom Report Layout
/// desde Business Central (Report Layout Selection) después de publicar la extensión.
/// </remarks>
reportextension 60372 "Pedido-QR" extends 405
{

    dataset
    {
        add("Purchase Header")
        {
            column(PortalBaseUrl; GetPortalBaseUrl())
            {
            }

            column(VerificationUrl; GetVerificationUrl("No."))
            {
            }

            column(QRCodeBase64; GetQRCodeBase64("No."))
            {
            }
        }
    }

    var
        PortalSetup: Record "Portal Setup";
        QRCodeGenerator: Codeunit "Melcon QR Code Generator";
        CachedOrderNo: Code[20];
        CachedPortalBaseUrl: Text[250];
        CachedVerificationUrl: Text[250];
        CachedQRCodeBase64: Text;

    /// <summary>
    /// Obtiene la URL base del portal desde Portal Setup (con caché).
    /// </summary>
    /// <returns>URL base del portal configurada en Portal Setup.</returns>
    local procedure GetPortalBaseUrl(): Text[250]
    begin
        if CachedPortalBaseUrl <> '' then
            exit(CachedPortalBaseUrl);

        CachedPortalBaseUrl := PortalSetup.GetPortalURL();
        exit(CachedPortalBaseUrl);
    end;

    /// <summary>
    /// Construye la URL de verificación completa para una orden de compra.
    /// Formato: {PortalBaseUrl}?code={OrderNo}
    /// </summary>
    /// <param name="OrderNo">Número de la orden de compra.</param>
    /// <returns>URL completa de verificación o cadena vacía si QR no está habilitado.</returns>
    local procedure GetVerificationUrl(OrderNo: Code[20]): Text[250]
    begin
        if (CachedOrderNo = OrderNo) and (CachedVerificationUrl <> '') then
            exit(CachedVerificationUrl);

        CachedOrderNo := OrderNo;
        CachedVerificationUrl := '';

        if not PortalSetup.IsQRCodeEnabled() then
            exit('');

        CachedVerificationUrl := PortalSetup.GetVerificationURL(OrderNo);
        exit(CachedVerificationUrl);
    end;

    /// <summary>
    /// Genera el código QR en formato Base64 para una orden de compra.
    /// El QR contiene la URL de verificación de la orden.
    /// </summary>
    /// <param name="OrderNo">Número de la orden de compra.</param>
    /// <returns>Cadena Base64 del QR o cadena vacía si QR no está habilitado.</returns>
    local procedure GetQRCodeBase64(OrderNo: Code[20]): Text
    var
        VerificationURL: Text[250];
    begin
        if (CachedOrderNo = OrderNo) and (CachedQRCodeBase64 <> '') then
            exit(CachedQRCodeBase64);

        CachedQRCodeBase64 := '';

        VerificationURL := GetVerificationUrl(OrderNo);
        if VerificationURL = '' then
            exit('');

        CachedQRCodeBase64 := QRCodeGenerator.GetQRCodeAsBase64(VerificationURL);
        exit(CachedQRCodeBase64);
    end;
}


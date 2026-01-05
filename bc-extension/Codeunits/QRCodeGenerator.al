/// <summary>
/// Codeunit para generar códigos QR usando un servicio HTTP externo.
/// Utiliza api.qrserver.com (gratuito y sin autenticación) para generar imágenes QR.
/// </summary>
codeunit 60371 "Melcon QR Code Generator"
{
    /// <summary>
    /// Genera un código QR a partir de un texto y lo retorna como cadena Base64.
    /// </summary>
    /// <param name="TextToEncode">El texto que se codificará en el QR (típicamente una URL).</param>
    /// <returns>Cadena Base64 que representa la imagen PNG del código QR (150x150 píxeles).</returns>
    procedure GetQRCodeAsBase64(TextToEncode: Text): Text
    var
        HttpClient: HttpClient;
        HttpResponseMessage: HttpResponseMessage;
        InStream: InStream;
        Base64Convert: Codeunit "Base64 Convert";
        RequestUri: Text;
    begin
        if TextToEncode = '' then
            exit('');

        // Usar api.qrserver.com para generar el QR
        // Parámetros: size (tamaño en píxeles), format (png, svg, etc)
        RequestUri := 'https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=' + TextToEncode;

        if not HttpClient.Get(RequestUri, HttpResponseMessage) then
            Error('Error al conectar con el servicio de generación de códigos QR.');

        if not HttpResponseMessage.IsSuccessStatusCode() then
            Error('Error al generar código QR: %1', HttpResponseMessage.ReasonPhrase());

        // Leer la respuesta (imagen) como stream y convertir a Base64
        HttpResponseMessage.Content().ReadAs(InStream);
        exit(Base64Convert.ToBase64(InStream));
    end;

    /// <summary>
    /// Retorna la URL directa del servicio de generación de QR para un texto dado.
    /// Útil para usar en reportes web o cuando se necesita la URL en lugar de la imagen.
    /// </summary>
    /// <param name="TextToEncode">El texto que se codificará en el QR.</param>
    /// <returns>URL del servicio que genera el QR (api.qrserver.com).</returns>
    procedure GetQRCodeImageURL(TextToEncode: Text): Text
    begin
        exit('https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=' + TextToEncode);
    end;
}


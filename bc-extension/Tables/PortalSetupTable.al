/// <summary>
/// Tabla de configuración singleton para el portal de verificación de órdenes.
/// Almacena la URL base del portal y controla si la generación de códigos QR está habilitada.
/// </summary>
/// <remarks>
/// Solo existe un registro con Primary Key = 'PORTAL' (patrón singleton).
/// </remarks>
table 60370 "Portal Setup"
{
    Caption = 'Portal QR Code Setup';
    DataClassification = CustomerContent;

    fields
    {
        field(1; "Primary Key"; Code[10])
        {
            Caption = 'Primary Key';
            DataClassification = SystemMetadata;
        }
        field(10; "Portal Base URL"; Text[250])
        {
            Caption = 'Portal Base URL';
            DataClassification = CustomerContent;

            trigger OnValidate()
            begin
                if "Portal Base URL" <> '' then begin
                    // Validación básica: debe comenzar con http:// o https://
                    if not (("Portal Base URL".StartsWith('http://')) or ("Portal Base URL".StartsWith('https://'))) then
                        Error('La URL debe comenzar con http:// o https:// (ej: https://verificacion.melcon.com)');
                end;
            end;
        }
        field(20; "Enabled"; Boolean)
        {
            Caption = 'Enabled';
            DataClassification = SystemMetadata;

            trigger OnValidate()
            begin
                if "Enabled" and ("Portal Base URL" = '') then
                    Error('Debe ingresar la URL base del portal antes de habilitar la generación de códigos QR.');
            end;
        }
    }

    keys
    {
        key(PK; "Primary Key")
        {
            Clustered = true;
        }
    }

    trigger OnInsert()
    begin
        if "Primary Key" = '' then
            "Primary Key" := 'PORTAL';
    end;

    /// <summary>
    /// Obtiene la URL base del portal. Si no existe el registro, lo crea automáticamente.
    /// </summary>
    /// <returns>URL base del portal configurada, o cadena vacía si no está configurada.</returns>
    procedure GetPortalURL(): Text[250]
    begin
        if not Get('PORTAL') then begin
            Init();
            "Primary Key" := 'PORTAL';
            Insert();
        end;
        exit("Portal Base URL");
    end;

    /// <summary>
    /// Verifica si la generación de códigos QR está habilitada y configurada correctamente.
    /// </summary>
    /// <returns>True si QR está habilitado y hay URL configurada, False en caso contrario.</returns>
    procedure IsQRCodeEnabled(): Boolean
    begin
        if not Get('PORTAL') then
            exit(false);
        exit("Enabled" and ("Portal Base URL" <> ''));
    end;

    /// <summary>
    /// Construye la URL completa de verificación para una orden de compra.
    /// Formato: {PortalBaseURL}?code={OrderNo}
    /// </summary>
    /// <param name="OrderNo">Número de la orden de compra.</param>
    /// <returns>URL completa de verificación o cadena vacía si no hay URL base configurada.</returns>
    procedure GetVerificationURL(OrderNo: Code[20]): Text[250]
    var
        BaseURL: Text[250];
    begin
        BaseURL := GetPortalURL();
        if BaseURL = '' then
            exit('');

        // Construir URL con parámetro code
        if BaseURL.EndsWith('/') then
            exit(BaseURL + '?code=' + OrderNo)
        else
            exit(BaseURL + '?code=' + OrderNo);
    end;
}


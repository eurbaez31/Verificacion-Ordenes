/// <summary>
/// Página de configuración para gestionar la URL del portal de verificación
/// y habilitar/deshabilitar la generación de códigos QR en los pedidos de compra.
/// </summary>
page 60370 "Portal Setup"
{
    PageType = Card;
    SourceTable = "Portal Setup";
    Caption = 'Portal QR Code Setup';
    UsageCategory = Administration;
    ApplicationArea = All;

    layout
    {
        area(Content)
        {
            group(General)
            {
                Caption = 'General';

                field("Portal Base URL"; Rec."Portal Base URL")
                {
                    ApplicationArea = All;
                    ToolTip = 'Ingrese la URL base del portal de verificación (ej: https://verificacion.melcon.com o http://localhost:8081 para desarrollo)';
                }
                field("Enabled"; Rec."Enabled")
                {
                    ApplicationArea = All;
                    ToolTip = 'Habilite esta opción para generar códigos QR en los pedidos de compra';
                }
            }
            group(Information)
            {
                Caption = 'Information';
                Editable = false;

                field(InfoText; 'El código QR generado en los pedidos de compra contendrá la URL:')
                {
                    ApplicationArea = All;
                    Caption = 'Formato del QR';
                    ToolTip = 'Formato de la URL que se incluirá en el código QR';
                }
                field(ExampleURL; GetExampleURL())
                {
                    ApplicationArea = All;
                    Caption = 'Ejemplo de URL';
                    ToolTip = 'Ejemplo de cómo se verá la URL en el código QR';
                }
            }
        }
    }

    actions
    {
        area(Processing)
        {
            action(TestURL)
            {
                ApplicationArea = All;
                Caption = 'Probar URL';
                Image = Web;
                ToolTip = 'Abre la URL del portal en el navegador para verificar que funciona correctamente';

                trigger OnAction()
                begin
                    if Rec."Portal Base URL" = '' then
                        Error('Debe ingresar la URL base del portal primero.');
                    HyperLink(Rec."Portal Base URL");
                end;
            }
        }
    }

    trigger OnOpenPage()
    begin
        // Asegurar que existe el registro singleton
        if not Rec.Get('PORTAL') then begin
            Rec.Init();
            Rec."Primary Key" := 'PORTAL';
            Rec.Insert();
        end;
    end;

    /// <summary>
    /// Genera una URL de ejemplo para mostrar en la página de configuración.
    /// </summary>
    /// <returns>URL de ejemplo con un número de orden ficticio.</returns>
    local procedure GetExampleURL(): Text[250]
    begin
        if Rec."Portal Base URL" = '' then
            exit('https://verificacion.melcon.com?code=PC0007998');

        if Rec."Portal Base URL".EndsWith('/') then
            exit(Rec."Portal Base URL" + '?code=PC0007998')
        else
            exit(Rec."Portal Base URL" + '?code=PC0007998');
    end;
}


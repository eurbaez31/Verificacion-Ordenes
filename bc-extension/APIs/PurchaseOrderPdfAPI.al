/// <summary>
/// API Page que expone la funcionalidad de generación de PDF para Purchase Orders.
/// Permite obtener el PDF de una orden de compra en formato Base64 mediante OData.
/// </summary>
/// <remarks>
/// El PDF se genera usando el mismo reporte configurado en Report Selections para Purchase Order.
/// Si no hay configuración, usa el reporte estándar 405.
/// </remarks>
page 60369 "Purchase Order PDF API"
{
    PageType = API;
    SourceTable = "Purchase Header";
    APIPublisher = 'melcon';
    APIGroup = 'purchasing';
    APIVersion = 'v2.0';
    EntityName = 'purchaseOrderPdf';
    EntitySetName = 'purchaseOrderPdfs';
    DelayedInsert = true;
    ODataKeyFields = SystemId;

    layout
    {
        area(Content)
        {
            repeater(GroupName)
            {
                field(id; Rec.SystemId)
                {
                    Caption = 'Id';
                }
                field(number; Rec."No.")
                {
                    Caption = 'Number';
                }
                field(pdfBase64; GetPurchaseOrderPdfBase64())
                {
                    Caption = 'PDF Base64';
                }
            }
        }
    }

    trigger OnAfterGetRecord()
    begin
        // Pre-generar el PDF cuando se obtiene el registro
        // Esto asegura que el PDF esté listo cuando se serialice
    end;

    /// <summary>
    /// Genera el PDF de la orden de compra actual y lo retorna como cadena Base64.
    /// </summary>
    /// <returns>Cadena Base64 que representa el PDF de la orden de compra.</returns>
    local procedure GetPurchaseOrderPdfBase64(): Text
    var
        TempBlob: Codeunit "Temp Blob";
        ReportSelection: Record "Report Selections";
        ReportId: Integer;
        InStream: InStream;
        OutStream: OutStream;
        Base64Convert: Codeunit "Base64 Convert";
        PdfContent: Text;
        PurchaseHeaderRecordRef: RecordRef;
    begin
        ReportSelection.Reset();
        ReportSelection.SetRange(Usage, ReportSelection.Usage::"P.Order");
        if ReportSelection.FindFirst() then
            ReportId := ReportSelection."Report ID"
        else
            // Fallback al reporte estándar si no hay configuración
            ReportId := 405; // Report 405 = "Purchase Order" (Purchase - Order)

        // Preparar el reporte
        Rec.SetRecFilter();

        // Convertir Record a RecordRef para Report.SaveAs
        PurchaseHeaderRecordRef.GetTable(Rec);

        // Generar PDF a TempBlob
        TempBlob.CreateOutStream(OutStream);
        Report.SaveAs(ReportId, '', ReportFormat::Pdf, OutStream, PurchaseHeaderRecordRef);

        // Leer el contenido del blob y convertir a Base64
        TempBlob.CreateInStream(InStream);
        PdfContent := Base64Convert.ToBase64(InStream);

        exit(PdfContent);
    end;
}


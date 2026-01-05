/// <summary>
/// API Page que expone información de proveedores para el portal de verificación.
/// Permite mapear Vendor Portal ID (GUID) a Vendor No. para autenticación.
/// </summary>
/// <remarks>
/// Solo expone proveedores que tengan el campo "Vendor ID" asignado (no vacío).
/// Este campo se pobla desde el backend usando VendorIdHelper.CreateFromVendorNo().
/// </remarks>
page 60368 "Portal Vendors API"
{
    PageType = API;
    SourceTable = Vendor;
    APIPublisher = 'melcon';
    APIGroup = 'purchasing';
    APIVersion = 'v2.0';
    EntityName = 'portalVendor';
    EntitySetName = 'portalVendors';
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
                field(vendorNo; Rec."No.")
                {
                    Caption = 'Vendor No.';
                }
                field(vendorName; Rec.Name)
                {
                    Caption = 'Vendor Name';
                }
                field(vendorPortalId; Rec."Vendor ID")
                {
                    Caption = 'Vendor Portal ID';
                }
            }
        }
    }
    
    trigger OnOpenPage()
    begin
        // Solo exponer proveedores que tengan Vendor ID asignado
        Rec.SetFilter("Vendor ID", '<>%1', CreateGuid());
    end;
}


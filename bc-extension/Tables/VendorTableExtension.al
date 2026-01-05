/// <summary>
/// Extensión de la tabla Vendor que agrega el campo "Vendor ID" (GUID).
/// Este campo almacena un GUID determinístico generado desde el Vendor No.
/// </summary>
/// <remarks>
/// El campo se pobla desde el backend usando VendorIdHelper.CreateFromVendorNo(),
/// que genera el mismo GUID para el mismo Vendor No. (algoritmo determinístico).
/// Este GUID se usa para vincular usuarios de Azure AD B2C con proveedores en BC.
/// </remarks>
tableextension 60368 "Vendor Portal Extension" extends Vendor
{
    fields
    {
        /// <summary>
        /// GUID determinístico que identifica al proveedor en el portal.
        /// Se genera desde el Vendor No. usando el mismo algoritmo que CustomerIdHelper.
        /// </summary>
        field(60368; "Vendor ID"; Guid)
        {
            Caption = 'Vendor ID';
            DataClassification = CustomerContent;
            Editable = false;
            
            trigger OnValidate()
            begin
                // Este campo se poblará automáticamente desde el backend
                // usando VendorIdHelper.CreateFromVendorNo()
            end;
        }
    }
}


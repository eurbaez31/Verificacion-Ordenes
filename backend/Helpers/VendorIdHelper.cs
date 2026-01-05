using System.Security.Cryptography;
using System.Text;

namespace OrderVerificationApi.Helpers;

/// <summary>
/// Helper para generar identificadores determinísticos a partir de códigos de proveedor de Business Central
/// Usa el mismo algoritmo que CustomerIdHelper para mantener consistencia
/// </summary>
public static class VendorIdHelper
{
    // Namespace UUID para generar GUIDs determinísticos (UUID v5)
    // Este es un namespace UUID estándar para nombres DNS, pero lo usamos para nuestros propósitos
    private static readonly Guid NamespaceGuid = new Guid("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

    /// <summary>
    /// Genera un GUID determinístico a partir del código de proveedor de Business Central.
    /// El mismo VendorNo siempre generará el mismo GUID.
    /// </summary>
    /// <param name="vendorNo">Código del proveedor en Business Central (ej: "PRV000069")</param>
    /// <returns>GUID determinístico generado desde el VendorNo</returns>
    public static Guid CreateFromVendorNo(string vendorNo)
    {
        if (string.IsNullOrWhiteSpace(vendorNo))
        {
            throw new ArgumentException("El código de proveedor no puede estar vacío.", nameof(vendorNo));
        }

        // Normalizar el código de proveedor (mayúsculas, sin espacios)
        var normalized = vendorNo.Trim().ToUpperInvariant();

        // Usar SHA1 para generar un hash determinístico (similar a UUID v5)
        // Convertir el namespace GUID y el nombre a bytes
        var namespaceBytes = NamespaceGuid.ToByteArray();
        var nameBytes = Encoding.UTF8.GetBytes(normalized);

        // Combinar namespace + nombre
        var combinedBytes = new byte[namespaceBytes.Length + nameBytes.Length];
        Array.Copy(namespaceBytes, 0, combinedBytes, 0, namespaceBytes.Length);
        Array.Copy(nameBytes, 0, combinedBytes, namespaceBytes.Length, nameBytes.Length);

        // Generar hash SHA1
        byte[] hash;
        using (var sha1 = SHA1.Create())
        {
            hash = sha1.ComputeHash(combinedBytes);
        }

        // Convertir los primeros 16 bytes del hash a GUID
        // Ajustar algunos bits según el estándar UUID v5
        var guidBytes = new byte[16];
        Array.Copy(hash, 0, guidBytes, 0, 16);

        // Establecer la versión (5 para UUID v5) en el byte 6
        guidBytes[6] = (byte)((guidBytes[6] & 0x0F) | 0x50);

        // Establecer la variante (10xxxxxx) en el byte 8
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }
}


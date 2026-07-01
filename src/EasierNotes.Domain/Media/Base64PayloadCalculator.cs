using System;

namespace EasierNotes.Domain.Media
{
    /// <summary>
    /// Calcula, de forma determinista, el tamaño que ocupará una imagen una vez
    /// convertida a Base64 e incrustada como Data URL dentro del HTML de una nota.
    ///
    /// CONTEXTO (CP-PE-13 — Utilización de Recursos):
    /// El frontend usa FileReader.readAsDataURL, que codifica la imagen en Base64
    /// y la incrusta inline en el campo 'html' (MEDIUMTEXT). Base64 infla el tamaño
    /// ~33 %. Esta clase permite anticipar, SIN ejecutar el navegador ni la BD,
    /// cuánto pesará el payload y si seguirá cabiendo en MEDIUMTEXT.
    ///
    /// DISEÑO: Pure Function / Calculator. No depende de archivos, red ni BD:
    /// recibe números y devuelve números. Es unitariamente testeable en sentido estricto.
    /// </summary>
    public static class Base64PayloadCalculator
    {
        /// <summary>Capacidad del tipo MySQL MEDIUMTEXT, en bytes (2^24 - 1).</summary>
        public const long MediumTextCapacityBytes = 16_777_215;

        /// <summary>
        /// Número de caracteres Base64 que produce una entrada de <paramref name="sizeInBytes"/> bytes.
        /// Fórmula exacta: 4 * ceil(n / 3). Cada bloque de 3 bytes -> 4 caracteres, con padding.
        /// </summary>
        public static long Base64Length(long sizeInBytes)
        {
            if (sizeInBytes < 0)
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "El tamaño no puede ser negativo.");

            // ceil(n/3) sin aritmética de punto flotante: (n + 2) / 3 en enteros.
            long blocks = (sizeInBytes + 2) / 3;
            return blocks * 4;
        }

        /// <summary>
        /// Longitud total del Data URL = longitud del prefijo (ej. "data:image/jpeg;base64,")
        /// más la longitud del cuerpo Base64.
        /// </summary>
        /// <param name="sizeInBytes">Tamaño de la imagen original en bytes.</param>
        /// <param name="mimeType">Tipo MIME usado para construir el prefijo del Data URL.</param>
        public static long DataUrlLength(long sizeInBytes, string mimeType)
        {
            if (string.IsNullOrWhiteSpace(mimeType))
                throw new ArgumentException("El tipo MIME es obligatorio.", nameof(mimeType));

            // Prefijo real generado por readAsDataURL: "data:" + mime + ";base64,"
            int prefixLength = "data:".Length + mimeType.Length + ";base64,".Length;
            return prefixLength + Base64Length(sizeInBytes);
        }

        /// <summary>
        /// Factor de inflación (proporción) del Data URL respecto al tamaño original.
        /// Ej.: ~1.333 para imágenes grandes (33 % de aumento).
        /// </summary>
        public static double InflationFactor(long sizeInBytes, string mimeType)
        {
            if (sizeInBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(sizeInBytes), "Se requiere un tamaño positivo.");

            return (double)DataUrlLength(sizeInBytes, mimeType) / sizeInBytes;
        }

        /// <summary>
        /// Indica si una única imagen, ya codificada como Data URL, cabe en un campo MEDIUMTEXT.
        /// </summary>
        public static bool FitsInMediumText(long sizeInBytes, string mimeType)
            => DataUrlLength(sizeInBytes, mimeType) <= MediumTextCapacityBytes;

        /// <summary>
        /// Número máximo de imágenes de un tamaño dado que caben en un MEDIUMTEXT
        /// (asumiendo solo los Data URLs, sin contar otro contenido de la nota).
        /// </summary>
        public static long MaxImagesInMediumText(long sizeInBytes, string mimeType)
        {
            // DataUrlLength siempre incluye el prefijo del Data URL, por lo que perImage > 0.
            long perImage = DataUrlLength(sizeInBytes, mimeType);
            return MediumTextCapacityBytes / perImage;
        }
    }
}

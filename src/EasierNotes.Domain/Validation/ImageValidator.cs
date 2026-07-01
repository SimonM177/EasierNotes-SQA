using System;
using System.Collections.Generic;

namespace EasierNotes.Domain.Validation
{
    /// <summary>
    /// Valida que una imagen cumpla las reglas de la historia de usuario "Insertar imagen":
    /// tamaño máximo de 5 MB y formato restringido a .jpg / .png.
    ///
    /// DISEÑO (patrones aplicados):
    ///  - Guard Clauses: cada regla se evalúa en orden y retorna temprano ante el primer fallo.
    ///  - Specification: las reglas (tamaño, formato) son explícitas y nombradas.
    ///  - Pure Function: no depende de BD, red ni sistema de archivos. Solo recibe
    ///    (tamaño en bytes, tipo MIME) y devuelve un resultado. Esto la hace
    ///    UNITARIAMENTE TESTEABLE en sentido estricto: determinista, rápida y aislada.
    ///
    /// Esta clase NO existe en el sistema original (el código del autor no valida imágenes).
    /// Se introduce como refactorización para extraer la regla de negocio a una unidad pura,
    /// conforme a la decisión de estrategia de pruebas.
    /// </summary>
    public sealed class ImageValidator
    {
        /// <summary>Tamaño máximo permitido: 5 MB expresados en bytes (5 * 1024 * 1024).</summary>
        public const long MaxSizeBytes = 5L * 1024 * 1024; // 5.242.880

        /// <summary>Conjunto de tipos MIME permitidos (comparación sin distinción de mayúsculas).</summary>
        private static readonly HashSet<string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg",
            "image/png"
        };

        /// <summary>
        /// Evalúa una imagen por su tamaño y tipo MIME.
        /// </summary>
        /// <param name="sizeInBytes">Tamaño del archivo en bytes.</param>
        /// <param name="mimeType">Tipo MIME declarado del archivo (ej. "image/jpeg").</param>
        /// <returns>Un <see cref="ImageValidationResult"/> con el veredicto y la causa.</returns>
        public ImageValidationResult Validate(long sizeInBytes, string? mimeType)
        {
            // Guard 1: tamaño no positivo (archivo vacío o corrupto).
            if (sizeInBytes <= 0)
                return ImageValidationResult.Failure(ImageRejectionReason.EmptyOrInvalidSize);

            // Guard 2: formato. Un MIME nulo/vacío o fuera de la lista es rechazado.
            if (string.IsNullOrWhiteSpace(mimeType) || !AllowedMimeTypes.Contains(mimeType))
                return ImageValidationResult.Failure(ImageRejectionReason.UnsupportedFormat);

            // Guard 3: tamaño máximo. El límite es inclusivo: exactamente 5 MB es válido.
            if (sizeInBytes > MaxSizeBytes)
                return ImageValidationResult.Failure(ImageRejectionReason.ExceedsMaxSize);

            // Todas las reglas se cumplen.
            return ImageValidationResult.Success();
        }
    }
}

namespace EasierNotes.Domain.Validation
{
    /// <summary>
    /// Resultado inmutable de una validación de imagen (Result Object Pattern).
    /// En lugar de devolver un bool opaco o lanzar excepciones para el flujo normal,
    /// encapsula el veredicto (IsValid) y la causa (Reason). Esto hace que las
    /// pruebas unitarias puedan afirmar el resultado exacto sin ambigüedad.
    /// </summary>
    public sealed class ImageValidationResult
    {
        public bool IsValid { get; }
        public ImageRejectionReason Reason { get; }

        private ImageValidationResult(bool isValid, ImageRejectionReason reason)
        {
            IsValid = isValid;
            Reason = reason;
        }

        /// <summary>Crea un resultado de éxito (imagen aceptada).</summary>
        public static ImageValidationResult Success()
            => new(true, ImageRejectionReason.None);

        /// <summary>Crea un resultado de fallo con la razón del rechazo.</summary>
        public static ImageValidationResult Failure(ImageRejectionReason reason)
            => new(false, reason);

        public override string ToString()
            => IsValid ? "Valid" : $"Invalid({Reason})";
    }
}

namespace EasierNotes.Domain.Validation
{
    /// <summary>
    /// Enumera las razones por las que una imagen puede ser rechazada.
    /// Forma parte del patrón Result Object: permite que las pruebas verifiquen
    /// no solo QUE se rechazó, sino POR QUÉ se rechazó (aserciones precisas).
    /// </summary>
    public enum ImageRejectionReason
    {
        /// <summary>La imagen es válida; no hay razón de rechazo.</summary>
        None = 0,

        /// <summary>El tamaño del archivo excede el máximo permitido (5 MB).</summary>
        ExceedsMaxSize = 1,

        /// <summary>El formato (tipo MIME) no está en la lista de permitidos (.jpg, .png).</summary>
        UnsupportedFormat = 2,

        /// <summary>El archivo está vacío o tiene tamaño no positivo.</summary>
        EmptyOrInvalidSize = 3
    }
}
